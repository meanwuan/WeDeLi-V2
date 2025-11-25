using wedeli.Models.Domain;
using wedeli.Models.DTO;

namespace wedeli.Repositories.Interface
{
    public interface IDriverRepository
    {
        // CRUD Operations
        Task<Driver?> GetDriverByIdAsync(int driverId);
        Task<Driver?> GetDriverByUserIdAsync(int userId);
        Task<(List<Driver> Drivers, int TotalCount)> GetDriversAsync(DriverFilterDto filter);
        Task<List<Driver>> GetDriversByCompanyAsync(int companyId);
        Task<Driver> CreateDriverAsync(Driver driver);
        Task<Driver> UpdateDriverAsync(Driver driver);
        Task<bool> DeleteDriverAsync(int driverId);

        // Performance & Statistics
        Task<DriverPerformanceDto> GetDriverPerformanceAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<Order>> GetDriverOrdersAsync(int driverId, string? status = null);
        Task<List<Order>> GetDriverOrdersByDateAsync(int driverId, DateTime date);
        Task<DriverDailySummaryDto> GetDriverDailySummaryAsync(int driverId, DateTime date);
        Task UpdateDriverStatisticsAsync(int driverId, bool isSuccess);

        // Availability & Status
        Task<List<Driver>> GetAvailableDriversAsync(int? companyId = null);
        Task<bool> IsDriverAvailableAsync(int driverId);
        Task<List<Driver>> GetDriversWithExpiringLicenseAsync(int daysThreshold = 30);

        // Ratings & Reviews
        Task UpdateDriverRatingAsync(int driverId, int newRatingScore);
        Task<decimal> GetDriverAverageRatingAsync(int driverId);

        // COD Management
        Task<decimal> GetDriverPendingCodAsync(int driverId);
        Task<List<CodTransaction>> GetDriverCodTransactionsAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);

        // Helper Methods
        Task<bool> DriverExistsAsync(int driverId);
        Task<bool> UserIsDriverAsync(int userId);
        Task<int> GetDriverCountByCompanyAsync(int companyId);
    }
}