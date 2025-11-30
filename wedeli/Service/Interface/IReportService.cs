using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Rating;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Report;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Report service interface
    /// </summary>
    public interface IReportService
    {
        // Daily reports
        Task<DailySummaryDto> GetDailySummaryAsync(DateTime date);
        Task<IEnumerable<DailySummaryDto>> GetDailySummariesAsync(DateTime startDate, DateTime endDate);
        Task<bool> GenerateDailySummaryAsync(DateTime date);
        
        // Performance reports
        Task<DriverPerformanceDto> GetDriverPerformanceReportAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<DriverPerformanceDto>> GetTopDriversReportAsync(int companyId, int topN = 10);
    }
}
