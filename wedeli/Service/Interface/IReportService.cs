using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Report;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Report service interface
    /// </summary>
    public interface IReportService
    {
        // Daily reports
        Task<DailyReportSummaryDto> GetDailySummaryAsync(DateTime date);
        Task<IEnumerable<DailyReportSummaryDto>> GetDailySummariesAsync(DateTime startDate, DateTime endDate);
        Task<bool> GenerateDailySummaryAsync(DateTime date);
        
        // Summary report
        Task<ReportSummaryDto> GetSummaryAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null);
        
        // Performance reports
        Task<DriverreportPerformanceDto> GetDriverPerformanceReportAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<DriverreportPerformanceDto>> GetTopDriversReportAsync(int companyId, int topN = 10);
    }
}
