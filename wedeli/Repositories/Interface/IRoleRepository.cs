using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        /// <summary>
        /// Get role by name
        /// </summary>
        Task<Role?> GetRoleByNameAsync(string roleName);

        /// <summary>
        /// Get all active roles
        /// </summary>
        //Task<List<Role>> GetActiveRolesAsync();

        /// <summary>
        /// Check if role name exists
        /// </summary>
        Task<bool> RoleExistsAsync(string roleName);

        /// <summary>
        /// Get users count for a role
        /// </summary>
        Task<int> GetUsersCountByRoleAsync(int roleId);

        /// <summary>
        /// Get roles by permission level
        /// </summary>
        //Task<List<Role>> GetRolesByPermissionLevelAsync(int minLevel, int maxLevel);
    }
}