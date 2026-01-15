using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO.Common;
using wedeli.Service.Interface;

namespace wedeli.Controllers
{
    [Route("api/v1/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get admin dashboard statistics
        /// </summary>
        /// <param name="companyId">Optional company ID to filter statistics</param>
        /// <returns>Dashboard statistics for admin</returns>
        [HttpGet("admin")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdminDashboard([FromQuery] int? companyId = null)
        {
            try
            {
                var stats = await _dashboardService.GetAdminDashboardAsync(companyId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard, CompanyId: {CompanyId}", companyId);
                return StatusCode(500, new { message = "An error occurred while retrieving admin dashboard statistics" });
            }
        }

        /// <summary>
        /// Get driver dashboard statistics
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <returns>Dashboard statistics for specific driver</returns>
        [HttpGet("driver/{driverId}")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDriverDashboard(int driverId)
        {
            try
            {
                var stats = await _dashboardService.GetDriverDashboardAsync(driverId);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver dashboard: {DriverId}", driverId);
                return StatusCode(500, new { message = "An error occurred while retrieving driver dashboard statistics" });
            }
        }

        /// <summary>
        /// Get customer dashboard statistics
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Dashboard statistics for specific customer</returns>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerDashboard(int customerId)
        {
            try
            {
                var stats = await _dashboardService.GetCustomerDashboardAsync(customerId);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer dashboard: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while retrieving customer dashboard statistics" });
            }
        }

        /// <summary>
        /// Get transport company dashboard statistics
        /// </summary>
        /// <param name="companyId">Transport Company ID</param>
        /// <returns>Dashboard statistics for specific transport company</returns>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompanyDashboard(int companyId)
        {
            try
            {
                var stats = await _dashboardService.GetCompanyDashboardAsync(companyId);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company dashboard: {CompanyId}", companyId);
                return StatusCode(500, new { message = "An error occurred while retrieving company dashboard statistics" });
            }
        }

        /// <summary>
        /// Get dashboard statistics based on user role
        /// </summary>
        /// <param name="userRole">User role (admin, driver, customer, company)</param>
        /// <param name="userId">User ID or Entity ID</param>
        /// <returns>Dashboard statistics based on role</returns>
        [HttpGet("role/{userRole}")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardByRole(string userRole, [FromQuery] int? userId = null)
        {
            try
            {
                switch (userRole.ToLower())
                {
                    case "admin":
                        var adminStats = await _dashboardService.GetAdminDashboardAsync();
                        return Ok(adminStats);

                    case "driver":
                        if (!userId.HasValue)
                            return BadRequest(new { message = "Driver ID is required" });
                        var driverStats = await _dashboardService.GetDriverDashboardAsync(userId.Value);
                        return Ok(driverStats);

                    case "customer":
                        if (!userId.HasValue)
                            return BadRequest(new { message = "Customer ID is required" });
                        var customerStats = await _dashboardService.GetCustomerDashboardAsync(userId.Value);
                        return Ok(customerStats);

                    case "company":
                        if (!userId.HasValue)
                            return BadRequest(new { message = "Company ID is required" });
                        var companyStats = await _dashboardService.GetCompanyDashboardAsync(userId.Value);
                        return Ok(companyStats);

                    default:
                        return BadRequest(new { message = "Invalid user role. Valid roles: admin, driver, customer, company" });
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard by role: {UserRole}, UserId: {UserId}", userRole, userId);
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics" });
            }
        }

        /// <summary>
        /// Get summary dashboard statistics (overview for all roles)
        /// </summary>
        /// <returns>Summary statistics</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                // Get overall statistics without filtering
                var stats = await _dashboardService.GetAdminDashboardAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard summary" });
            }
        }
    }
}