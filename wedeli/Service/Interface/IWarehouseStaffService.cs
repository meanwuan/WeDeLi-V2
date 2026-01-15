using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Warehouse staff service interface
    /// </summary>
    public interface IWarehouseStaffService
    {
        Task<WarehouseStaffDto> GetStaffByIdAsync(int staffId);
        Task<WarehouseStaffDto> GetStaffByUserIdAsync(int userId);
        Task<IEnumerable<WarehouseStaffDto>> GetAllStaffAsync();
        Task<IEnumerable<WarehouseStaffDto>> GetStaffByCompanyAsync(int companyId);
        Task<WarehouseStaffDto> CreateStaffAsync(CreateWarehouseStaffDto dto);
        Task<WarehouseStaffDto> UpdateStaffAsync(int staffId, UpdateWarehouseStaffDto dto);
        Task<bool> DeleteStaffAsync(int staffId);
        Task<bool> ToggleStaffStatusAsync(int staffId, bool isActive);
    }
}
