using wedeli.Models.Domain;
using System.Security.Claims;

namespace wedeli.Infrastructure
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        int? GetUserIdFromToken(string token);
        String? GetEmailFromToken(string token);
        bool IsTokenExpired(string token);
        string? GetRoleFromToken(string token);
    }
}
