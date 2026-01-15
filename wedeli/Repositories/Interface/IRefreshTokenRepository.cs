using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<RefreshToken> GetByUserIdAsync(int userId);
        Task<bool> RevokeAsync(string token);
        Task<bool> RevokeAllUserTokensAsync(int userId);
        Task<bool> DeleteExpiredTokensAsync();
    }
}
