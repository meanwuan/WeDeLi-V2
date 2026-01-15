using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Role service interface for role management
    /// </summary>
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int roleId);
        Task<RoleDto> GetRoleByNameAsync(string roleName);
    }
}
