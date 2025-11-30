using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<Role> GetByNameAsync(string roleName);
        Task<IEnumerable<Role>> GetAllActiveRolesAsync();
    }
}
