using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;

    public TokenService(
        IConfiguration config,
        UserManager<User> userManager)
    {
        _config = config;
        _userManager = userManager;
    }

    public async Task<string> GenerateJwtTokenAsync(User user)
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

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();

        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(bytes);
    }
}