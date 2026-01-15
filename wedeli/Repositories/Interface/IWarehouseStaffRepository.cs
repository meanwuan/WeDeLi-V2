using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IWarehouseStaffRepository : IBaseRepository<WarehouseStaff>
    {
        Task<WarehouseStaff> GetByUserIdAsync(int userId);
        Task<IEnumerable<WarehouseStaff>> GetByCompanyIdAsync(int companyId);
        Task<IEnumerable<WarehouseStaff>> GetByLocationAsync(string location);
        Task<bool> ToggleActiveStatusAsync(int staffId, bool isActive);
        Task SaveChangesAsync();
    }
}
