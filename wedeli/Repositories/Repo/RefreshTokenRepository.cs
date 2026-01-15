using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Implementation
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly PlatformDbContext _context;

        public RefreshTokenRepository(PlatformDbContext context)
        {
            _context = context;
        }

        #region Base Repository Methods

        public async Task<RefreshToken> GetByIdAsync(int id)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.RefreshTokenId == id);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .ToListAsync();
        }

        public async Task<RefreshToken> AddAsync(RefreshToken entity)
        {
            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<RefreshToken> UpdateAsync(RefreshToken entity)
        {
            _context.RefreshTokens.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var token = await _context.RefreshTokens.FindAsync(id);
            if (token == null)
                return false;

            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.RefreshTokens.AnyAsync(rt => rt.RefreshTokenId == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.RefreshTokens.CountAsync();
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Lấy refresh token theo token string
        /// </summary>
        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// Lấy refresh token mới nhất của user
        /// </summary>
        public async Task<RefreshToken> GetByUserIdAsync(int userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .OrderByDescending(rt => rt.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Revoke một refresh token
        /// </summary>
        public async Task<bool> RevokeAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
                return false;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Revoke tất cả refresh tokens của một user
        /// </summary>
        public async Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            if (!tokens.Any())
                return false;

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xóa tất cả refresh tokens đã hết hạn
        /// </summary>
        public async Task<bool> DeleteExpiredTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (!expiredTokens.Any())
                return false;

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}