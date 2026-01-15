using wedeli.Models.Domain;
using System.Security.Claims;

namespace wedeli.Infrastructure
{
    public interface IJwtService
    {
        /// <summary>
        /// Generate access token for platform users (SuperAdmin, Customer)
        /// </summary>
        string GenerateAccessToken(User user);
        
        /// <summary>
        /// Generate access token for company users (CompanyAdmin, Driver, WarehouseStaff)
        /// </summary>
        string GenerateAccessToken(CompanyUser companyUser, Driver? driver = null, WarehouseStaff? warehouseStaff = null);
        
        string GenerateRefreshToken();
        ClaimsPrincipal ValidateToken(string token);
        Task<RefreshToken> SaveRefreshTokenAsync(int userId, string token);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
        Task<bool> IsRefreshTokenValidAsync(string token);
        int? GetUserIdFromToken(string token);
    }
    
}

