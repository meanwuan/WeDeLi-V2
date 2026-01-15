using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Report;
using wedeli.Service.Interface;

namespace wedeli.Controllers
{
    [Route("api/v1/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IReportService reportService,
            ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // ===== Daily Reports =====

        /// <summary>
        /// Get daily summary report for a specific date
        /// </summary>
        /// <param name="date">Report date (format: yyyy-MM-dd)</param>
        /// <returns>Daily summary statistics</returns>
        [HttpGet("daily")]
        [ProducesResponseType(typeof(DailyReportSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailySummary([FromQuery] DateTime date)
        {
            try
            {
                if (date == default)
                {
                    return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd" });
                }

                var summary = await _reportService.GetDailySummaryAsync(date);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily summary for date: {Date}", date);
                return StatusCode(500, new { message = "An error occurred while retrieving daily summary" });
            }
        }

        /// <summary>
        /// Get daily summary report for today
        /// </summary>
        /// <returns>Today's summary statistics</returns>
        [HttpGet("daily/today")]
        [ProducesResponseType(typeof(DailyReportSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTodaySummary()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var summary = await _reportService.GetDailySummaryAsync(today);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's summary");
                return StatusCode(500, new { message = "An error occurred while retrieving today's summary" });
            }
        }

        /// <summary>
        /// Get daily summaries for a date range
        /// </summary>
        /// <param name="startDate">Start date (format: yyyy-MM-dd)</param>
        /// <param name="endDate">End date (format: yyyy-MM-dd)</param>
        /// <returns>List of daily summaries</returns>
        [HttpGet("daily/range")]
        [ProducesResponseType(typeof(IEnumerable<DailyReportSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailySummaries(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate == default || endDate == default)
                {
                    return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd for both dates" });
                }

                if (startDate > endDate)
                {
                    return BadRequest(new { message = "Start date must be before or equal to end date" });
                }

                // Limit to 365 days to prevent performance issues
                if ((endDate - startDate).TotalDays > 365)
                {
                    return BadRequest(new { message = "Date range cannot exceed 365 days" });
                }

                var summaries = await _reportService.GetDailySummariesAsync(startDate, endDate);
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily summaries for range: {StartDate} - {EndDate}", startDate, endDate);
                return StatusCode(500, new { message = "An error occurred while retrieving daily summaries" });
            }
        }

        /// <summary>
        /// Get daily summaries for the last N days
        /// </summary>
        /// <param name="days">Number of days (default: 7, max: 90)</param>
        /// <returns>List of daily summaries</returns>
        [HttpGet("daily/last-days")]
        [ProducesResponseType(typeof(IEnumerable<DailyReportSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLastDaysSummaries([FromQuery] int days = 7)
        {
            try
            {
                if (days <= 0 || days > 90)
                {
                    return BadRequest(new { message = "Days must be between 1 and 90" });
                }

                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days + 1);

                var summaries = await _reportService.GetDailySummariesAsync(startDate, endDate);
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last {Days} days summaries", days);
                return StatusCode(500, new { message = "An error occurred while retrieving daily summaries" });
            }
        }

        /// <summary>
        /// Get monthly report for a specific year and month
        /// </summary>
        /// <param name="year">Report year (e.g., 2025)</param>
        /// <param name="month">Report month (1-12)</param>
        /// <returns>Monthly summary statistics</returns>
        [HttpGet("monthly")]
        [ProducesResponseType(typeof(IEnumerable<DailyReportSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                if (year < 2000 || year > 2100)
                {
                    return BadRequest(new { message = "Year must be between 2000 and 2100" });
                }

                if (month < 1 || month > 12)
                {
                    return BadRequest(new { message = "Month must be between 1 and 12" });
                }

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var summaries = await _reportService.GetDailySummariesAsync(startDate, endDate);
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly report for: {Year}-{Month}", year, month);
                return StatusCode(500, new { message = "An error occurred while retrieving monthly report" });
            }
        }

        /// <summary>
        /// Get monthly summary (current month)
        /// </summary>
        /// <returns>Summary for current month</returns>
        [HttpGet("monthly/current")]
        [ProducesResponseType(typeof(IEnumerable<DailyReportSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentMonthSummary()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startDate = new DateTime(now.Year, now.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var summaries = await _reportService.GetDailySummariesAsync(startDate, endDate);
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current month summary");
                return StatusCode(500, new { message = "An error occurred while retrieving monthly summary" });
            }
        }

        /// <summary>
        /// Generate daily summary report for a specific date
        /// </summary>
        /// <param name="date">Report date</param>
        /// <returns>Success status</returns>
        [HttpPost("daily/generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateDailySummary([FromQuery] DateTime date)
        {
            try
            {
                if (date == default)
                {
                    return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd" });
                }

                var result = await _reportService.GenerateDailySummaryAsync(date);
                return Ok(new { message = "Daily summary generated successfully", success = result, date });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily summary for date: {Date}", date);
                return StatusCode(500, new { message = "An error occurred while generating daily summary" });
            }
        }

        // ===== Summary Report =====

        /// <summary>
        /// Get overall summary report for a company
        /// </summary>
        /// <param name="companyId">Transport Company ID</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>Summary statistics</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ReportSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSummary(
            [FromQuery] int companyId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { message = "Start date must be before or equal to end date" });
                }

                var summary = await _reportService.GetSummaryAsync(companyId, startDate, endDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary report for company: {CompanyId}", companyId);
                return StatusCode(500, new { message = "An error occurred while retrieving summary report" });
            }
        }

        // ===== Driver Performance Reports =====

        /// <summary>
        /// Get performance report for a specific driver
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>Driver performance statistics</returns>
        [HttpGet("driver/{driverId}/performance")]
        [ProducesResponseType(typeof(DriverPerformanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDriverPerformance(
            int driverId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { message = "Start date must be before or equal to end date" });
                }

                var performance = await _reportService.GetDriverPerformanceReportAsync(driverId, startDate, endDate);
                return Ok(performance);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver performance: {DriverId}", driverId);
                return StatusCode(500, new { message = "An error occurred while retrieving driver performance" });
            }
        }

        /// <summary>
        /// Get driver performance for current month
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <returns>Driver performance statistics for current month</returns>
        [HttpGet("driver/{driverId}/performance/monthly")]
        [ProducesResponseType(typeof(DriverPerformanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDriverMonthlyPerformance(int driverId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startDate = new DateTime(now.Year, now.Month, 1);
                var endDate = now.Date;

                var performance = await _reportService.GetDriverPerformanceReportAsync(driverId, startDate, endDate);
                return Ok(performance);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver monthly performance: {DriverId}", driverId);
                return StatusCode(500, new { message = "An error occurred while retrieving driver performance" });
            }
        }

        /// <summary>
        /// Get top performing drivers for a company
        /// </summary>
        /// <param name="companyId">Transport Company ID</param>
        /// <param name="topN">Number of top drivers to return (default: 10, max: 50)</param>
        /// <returns>List of top performing drivers</returns>
        [HttpGet("company/{companyId}/top-drivers")]
        [ProducesResponseType(typeof(IEnumerable<DriverPerformanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTopDrivers(int companyId, [FromQuery] int topN = 10)
        {
            try
            {
                if (topN <= 0 || topN > 50)
                {
                    return BadRequest(new { message = "topN must be between 1 and 50" });
                }

                var topDrivers = await _reportService.GetTopDriversReportAsync(companyId, topN);
                return Ok(topDrivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top drivers for company: {CompanyId}", companyId);
                return StatusCode(500, new { message = "An error occurred while retrieving top drivers" });
            }
        }

        /// <summary>
        /// Get all drivers performance for a company
        /// </summary>
        /// <param name="companyId">Transport Company ID</param>
        /// <returns>List of all drivers with their performance</returns>
        [HttpGet("company/{companyId}/drivers-performance")]
        [ProducesResponseType(typeof(IEnumerable<DriverPerformanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDriversPerformance(int companyId)
        {
            try
            {
                // Get all drivers (no limit)
                var allDrivers = await _reportService.GetTopDriversReportAsync(companyId, int.MaxValue);
                return Ok(allDrivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all drivers performance for company: {CompanyId}", companyId);
                return StatusCode(500, new { message = "An error occurred while retrieving drivers performance" });
            }
        }

        // ===== Export Reports (Future Enhancement) =====

        /// <summary>
        /// Export daily report as CSV (placeholder for future implementation)
        /// </summary>
        [HttpGet("daily/export/csv")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult ExportDailyReportCsv()
        {
            return StatusCode(501, new { message = "CSV export feature coming soon" });
        }

        /// <summary>
        /// Export driver performance as PDF (placeholder for future implementation)
        /// </summary>
        [HttpGet("driver/{driverId}/export/pdf")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult ExportDriverPerformancePdf(int driverId)
        {
            return StatusCode(501, new { message = "PDF export feature coming soon" });
        }
    }
}