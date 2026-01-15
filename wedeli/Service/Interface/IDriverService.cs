using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Driver service interface for driver management
    /// </summary>
    public interface IDriverService
    {
        Task<DriverResponseDto> GetDriverByIdAsync(int driverId);
        Task<DriverResponseDto> GetDriverByUserIdAsync(int userId);
        Task<IEnumerable<DriverResponseDto>> GetAllDriversAsync();
        Task<IEnumerable<DriverResponseDto>> GetDriversByCompanyAsync(int companyId);
        Task<IEnumerable<DriverResponseDto>> GetActiveDriversAsync(int companyId);
        Task<IEnumerable<DriverResponseDto>> GetTopPerformingDriversAsync(int companyId, int topN = 10);
        Task<DriverResponseDto> CreateDriverAsync(CreateDriverDto dto);
        Task<DriverResponseDto> UpdateDriverAsync(int driverId, UpdateDriverDto dto);
        Task<bool> DeleteDriverAsync(int driverId);
        Task<bool> ToggleDriverStatusAsync(int driverId, bool isActive);
        Task<DriverPerformanceDto> GetDriverPerformanceAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> UpdateDriverStatisticsAsync(int driverId);
        Task<bool> UpdateDriverRatingAsync(int driverId);
    }
}
