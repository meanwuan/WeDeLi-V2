using System;
using System.Threading.Tasks;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Dashboard service interface
    /// </summary>
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetAdminDashboardAsync(int? companyId = null);
        Task<DashboardStatsDto> GetDriverDashboardAsync(int driverId);
        Task<DashboardStatsDto> GetCustomerDashboardAsync(int customerId);
        Task<DashboardStatsDto> GetCompanyDashboardAsync(int companyId);
    }
}
