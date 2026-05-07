using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Data;
using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly ILogEventPublisher _logEventPublisher;

    public AuthController(
        IConfiguration config,
        AppDbContext context,
        UserManager<User> userManager,
        ILogEventPublisher logEventPublisher)
    {
        _config = config;
        _context = context;
        _userManager = userManager;
        _logEventPublisher = logEventPublisher;
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var key = _config["JWT_KEY"]
                  ?? throw new Exception("JWT_KEY is missing");

        var audience = _config["JWT_AUDIENCE"]
                       ?? throw new Exception("JWT_AUDIENCE is missing");

        var issuer = _config["JWT_ISSUER"]
                     ?? throw new Exception("JWT_ISSUER is missing");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByNameAsync(request.Username);

        if (user == null)
            return Unauthorized();

        var isValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isValid)
            return Unauthorized();

        var accessToken = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = HashToken(refreshToken),
            Username = user.UserName!,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        await _logEventPublisher.PublishAsync(new LogEvent
        {
            ServiceName = "AuthService",
            EventType = "UserLoggedIn",
            Level = "INFO",
            Message = $"User logged in: {user.UserName}",
            UserName = user.UserName,
            ResourceId = user.Id
        });

        return Ok(new
        {
            accessToken,
            refreshToken
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest();
        }

        var existingUser = await _userManager.FindByNameAsync(request.Username);

        if (existingUser != null)
        {
            return Conflict("User already exists");
        }

        var user = new User
        {
            UserName = request.Username
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        await _logEventPublisher.PublishAsync(new LogEvent
        {
            ServiceName = "AuthService",
            EventType = "UserRegistered",
            Level = "INFO",
            Message = $"User registered: {user.UserName}",
            UserName = user.UserName,
            ResourceId = user.Id
        });

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("users/{username}/promote")]
    public async Task<IActionResult> PromoteToAdmin(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return NotFound("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Admin"))
            return BadRequest("User is already an admin");

        var result = await _userManager.AddToRoleAsync(user, "Admin");

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _logEventPublisher.PublishAsync(new LogEvent
        {
            ServiceName = "AuthService",
            EventType = "UserPromotedToAdmin",
            Level = "INFO",
            Message = $"User promoted to admin: {user.UserName}",
            UserName = User.Identity?.Name,
            ResourceId = user.Id
        });

        return Ok($"{username} is now an Admin");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest();

        var hashed = HashToken(refreshToken);

        var tokenInDb = _context.RefreshTokens
            .FirstOrDefault(x => x.Token == hashed);

        if (tokenInDb == null)
            return Unauthorized();

        if (tokenInDb.ExpiresAt < DateTime.UtcNow)
            return Unauthorized();

        var user = await _userManager.FindByNameAsync(tokenInDb.Username);

        if (user == null)
            return Unauthorized();

        var newAccessToken = await GenerateJwtToken(user);

        _context.RefreshTokens.Remove(tokenInDb);

        var newRefreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = HashToken(newRefreshToken),
            Username = tokenInDb.Username,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        await _logEventPublisher.PublishAsync(new LogEvent
        {
            ServiceName = "AuthService",
            EventType = "TokenRefreshed",
            Level = "INFO",
            Message = $"Token refreshed for user: {user.UserName}",
            UserName = user.UserName,
            ResourceId = user.Id
        });

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(User.Identity?.Name);
    }
}