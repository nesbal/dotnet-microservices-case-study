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
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    public AuthController(
        IConfiguration config,
        AppDbContext context,
        UserManager<User> userManager)
    {
        _config = config;
        _context = context;
        _userManager = userManager;
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateJwtToken(string username)    {
        var key = _config["Jwt:Key"]
                  ?? throw new Exception("Jwt:Key is missing");

        var issuer = _config["Jwt:Issuer"]
                     ?? throw new Exception("Jwt:Issuer is missing");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),            
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
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

        var accessToken = GenerateJwtToken(user.UserName!);
        var refreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            Username = user.UserName!,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

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

        return Ok();
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

        var newAccessToken = GenerateJwtToken(tokenInDb.Username);

        _context.RefreshTokens.Remove(tokenInDb);

        var newRefreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = HashToken(newRefreshToken),
            Username = tokenInDb.Username,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

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