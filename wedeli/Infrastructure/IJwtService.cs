using wedeli.Models.Domain;
using System.Security.Claims;

namespace wedeli.Infrastructure
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal ValidateToken(string token);
        Task<RefreshToken> SaveRefreshTokenAsync(int userId, string token);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
        Task<bool> IsRefreshTokenValidAsync(string token);
        int? GetUserIdFromToken(string token);
    }
    
}
