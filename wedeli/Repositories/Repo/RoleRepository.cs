using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Role repository implementation
    /// </summary>
    public class RoleRepository : IBaseRepository<Role>, IRoleRepository
    {
        private readonly PlatformDbContext _context;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(PlatformDbContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        public async Task<Role> GetByIdAsync(int id)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
                if (role == null)
                {
                    _logger.LogWarning("Role not found {RoleId}", id);
                    throw new KeyNotFoundException($"Role with ID {id} not found.");
                }
                _logger.LogDebug("Found role {RoleId}: {RoleName}", id, role.RoleName);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        public async Task<Role> GetByNameAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    throw new ArgumentException("Role name is required");

                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == roleName);

                if (role == null)
                {
                    _logger.LogWarning("Role not found by name: {RoleName}", roleName);
                    throw new KeyNotFoundException($"Role '{roleName}' not found");
                }

                _logger.LogDebug("Found role by name: {RoleName}", roleName);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by name: {RoleName}", roleName);
                throw;
            }
        }

        /// <summary>
        /// Get all active roles
        /// </summary>
        public async Task<IEnumerable<Role>> GetAllActiveRolesAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} roles", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                throw;
            }
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} roles", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                throw;
            }
        }

        /// <summary>
        /// Add role
        /// </summary>
        public async Task<Role> AddAsync(Role entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (string.IsNullOrWhiteSpace(entity.RoleName))
                    throw new ArgumentException("Role name is required");

                // Check if role already exists
                if (await _context.Roles.AnyAsync(r => r.RoleName == entity.RoleName))
                    throw new InvalidOperationException($"Role '{entity.RoleName}' already exists");

                await _context.Roles.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new role: {RoleId} ({RoleName})", entity.RoleId, entity.RoleName);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", entity?.RoleName);
                throw;
            }
        }

        /// <summary>
        /// Update role
        /// </summary>
        public async Task<Role> UpdateAsync(Role entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleId == entity.RoleId);

                if (existingRole == null)
                    throw new KeyNotFoundException($"Role {entity.RoleId} not found");

                // Check if new name is already taken by another role
                if (existingRole.RoleName != entity.RoleName &&
                    await _context.Roles.AnyAsync(r => r.RoleName == entity.RoleName))
                    throw new InvalidOperationException($"Role '{entity.RoleName}' already exists");

                existingRole.RoleName = entity.RoleName;
                existingRole.Description = entity.Description;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated role: {RoleId} ({RoleName})", entity.RoleId, entity.RoleName);
                return existingRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleId}", entity?.RoleId);
                throw;
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent role: {RoleId}", id);
                    return false;
                }

                // Check if role has users
                var usersWithRole = await _context.Users
                    .CountAsync(u => u.RoleId == id);

                if (usersWithRole > 0)
                {
                    _logger.LogWarning("Cannot delete role {RoleId} - has {Count} users", id, usersWithRole);
                    throw new InvalidOperationException("Cannot delete role with associated users");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted role: {RoleId} ({RoleName})", id, role.RoleName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if role exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Roles.AnyAsync(r => r.RoleId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role existence: {RoleId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count all roles
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Roles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting roles");
                throw;
            }
        }
    }
}
