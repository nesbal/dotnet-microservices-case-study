using AuthService.Models;

namespace AuthService.Services;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(User user);

    string GenerateRefreshToken();

    string HashToken(string token);
}