using wedeli.Models.DTO;

namespace wedeli.service.Interface
{
    public interface IDriverService
    {
        // CRUD Operations
        Task<DriverDto?> GetDriverByIdAsync(int driverId);
        Task<DriverDto?> GetDriverByUserIdAsync(int userId);
        Task<(List<DriverDto> Drivers, int TotalCount)> GetDriversAsync(DriverFilterDto filter);
        Task<List<DriverDto>> GetDriversByCompanyAsync(int companyId);
        Task<DriverDto> CreateDriverAsync(CreateDriverDto dto);
        Task<DriverDto> UpdateDriverAsync(int driverId, UpdateDriverDto dto);
        Task<bool> DeleteDriverAsync(int driverId);

        // Performance & Statistics
        Task<DriverPerformanceDto> GetDriverPerformanceAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);
        Task<DriverOrdersDto> GetDriverOrdersAsync(int driverId, string? status = null);
        Task<DriverDailySummaryDto> GetDriverDailySummaryAsync(int driverId, DateTime date);

        // Availability
        Task<List<DriverDto>> GetAvailableDriversAsync(int? companyId = null);
        Task<bool> IsDriverAvailableAsync(int driverId);

        // License Management
        Task<List<DriverDto>> GetDriversWithExpiringLicenseAsync(int daysThreshold = 30);

        // COD Management
        Task<decimal> GetDriverPendingCodAsync(int driverId);
    }
}