using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.Models;

namespace AuthService.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private string GenerateJwtToken()
    {
        var key = _config["Jwt:Key"]
                  ?? throw new Exception("Jwt:Key is missing");

        var issuer = _config["Jwt:Issuer"]
                     ?? throw new Exception("Jwt:Issuer is missing");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "nesibe"),
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

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var accessToken = GenerateJwtToken();
        var refreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            Username = "nesibe",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken
        });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] string refreshToken)
    {
        var tokenInDb = _context.RefreshTokens
            .FirstOrDefault(x => x.Token == refreshToken);

        if (tokenInDb == null)
            return Unauthorized();

        if (tokenInDb.ExpiresAt < DateTime.UtcNow)
            return Unauthorized();

        var newAccessToken = GenerateJwtToken();

        return Ok(new
        {
            accessToken = newAccessToken
        });
    }
}