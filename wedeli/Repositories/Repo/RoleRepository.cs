using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext context, ILogger<RoleRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            try
            {
                return await _dbSet
                    .FirstOrDefaultAsync(r => r.RoleName == roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting role by name: {roleName}");
                throw;
            }
        }

        //public async Task<List<Role>> GetActiveRolesAsync()
        //{
        //    try
        //    {
        //        return await _dbSet
        //            .Where(r => r.IsActive == true)
        //            .OrderBy(r => r.RoleName)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting active roles");
        //        throw;
        //    }
        //}

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            try
            {
                return await _dbSet
                    .AnyAsync(r => r.RoleName == roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if role exists: {roleName}");
                throw;
            }
        }

        public async Task<int> GetUsersCountByRoleAsync(int roleId)
        {
            try
            {
                var role = await _dbSet
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.RoleId == roleId);

                return role?.Users?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users count for role: {roleId}");
                throw;
            }
        }

        //public async Task<List<Role>> GetRolesByPermissionLevelAsync(int minLevel, int maxLevel)
        //{
        //    try
        //    {
        //        return await _dbSet
        //            .Where(r => r.PermissionLevel >= minLevel && r.PermissionLevel <= maxLevel)
        //            .OrderBy(r => r.PermissionLevel)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error getting roles by permission level: {minLevel}-{maxLevel}");
        //        throw;
        //    }
        //}
    }
}