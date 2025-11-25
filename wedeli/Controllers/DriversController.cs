using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO;
using wedeli.service.Interface;

namespace wedeli.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DriversController : ControllerBase
    {
        private readonly IDriverService _driverService;
        private readonly ILogger<DriversController> _logger;

        public DriversController(IDriverService driverService, ILogger<DriversController> logger)
        {
            _driverService = driverService;
            _logger = logger;
        }

        // ===== CRUD Operations =====

        /// <summary>
        /// Get all drivers with filtering and pagination
        /// GET /api/drivers
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetDrivers([FromQuery] DriverFilterDto filter)
        {
            try
            {
                var (drivers, totalCount) = await _driverService.GetDriversAsync(filter);

                return Ok(new
                {
                    data = drivers,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drivers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get driver by ID
        /// GET /api/drivers/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(int id)
        {
            try
            {
                var driver = await _driverService.GetDriverByIdAsync(id);
                if (driver == null)
                    return NotFound(new { message = "Driver not found" });

                return Ok(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get driver by user ID
        /// GET /api/drivers/user/{userId}
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDriverByUserId(int userId)
        {
            try
            {
                var driver = await _driverService.GetDriverByUserIdAsync(userId);
                if (driver == null)
                    return NotFound(new { message = "Driver not found for this user" });

                return Ok(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver by user ID: {userId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get drivers by company
        /// GET /api/drivers/company/{companyId}
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetDriversByCompany(int companyId)
        {
            try
            {
                var drivers = await _driverService.GetDriversByCompanyAsync(companyId);
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting drivers for company: {companyId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new driver
        /// POST /api/drivers
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            try
            {
                var driver = await _driverService.CreateDriverAsync(dto);
                return CreatedAtAction(nameof(GetDriverById), new { id = driver.DriverId }, driver);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update driver information
        /// PUT /api/drivers/{id}
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDto dto)
        {
            try
            {
                var driver = await _driverService.UpdateDriverAsync(id, dto);
                return Ok(driver);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Driver not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating driver: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete driver (soft delete)
        /// DELETE /api/drivers/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                var deleted = await _driverService.DeleteDriverAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Driver not found" });

                return Ok(new { message = "Driver deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting driver: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Performance & Statistics =====

        /// <summary>
        /// Get driver performance statistics
        /// GET /api/drivers/{id}/performance?startDate=2025-01-01&endDate=2025-01-31
        /// </summary>
        [HttpGet("{id}/performance")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetDriverPerformance(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var performance = await _driverService.GetDriverPerformanceAsync(id, startDate, endDate);
                return Ok(performance);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Driver not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver performance: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get driver's assigned orders
        /// GET /api/drivers/{id}/orders?status=in_transit
        /// </summary>
        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetDriverOrders(int id, [FromQuery] string? status = null)
        {
            try
            {
                // Allow driver to see their own orders
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var driver = await _driverService.GetDriverByUserIdAsync(userId);
                    if (driver != null && driver.DriverId != id && !User.IsInRole("admin") && !User.IsInRole("warehouse_staff"))
                    {
                        return Forbid();
                    }
                }

                var orders = await _driverService.GetDriverOrdersAsync(id, status);
                return Ok(orders);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Driver not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver orders: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get driver's daily summary
        /// GET /api/drivers/{id}/daily-summary?date=2025-01-24
        /// </summary>
        [HttpGet("{id}/daily-summary")]
        [Authorize(Roles = "admin,warehouse_staff,driver")]
        public async Task<IActionResult> GetDriverDailySummary(int id, [FromQuery] DateTime? date = null)
        {
            try
            {
                var summaryDate = date ?? DateTime.Now;
                var summary = await _driverService.GetDriverDailySummaryAsync(id, summaryDate);
                return Ok(summary);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Driver not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver daily summary: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Availability Management =====

        /// <summary>
        /// Get available drivers
        /// GET /api/drivers/available?companyId=1
        /// </summary>
        [HttpGet("available")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetAvailableDrivers([FromQuery] int? companyId = null)
        {
            try
            {
                var drivers = await _driverService.GetAvailableDriversAsync(companyId);
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available drivers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if driver is available
        /// GET /api/drivers/{id}/is-available
        /// </summary>
        [HttpGet("{id}/is-available")]
        public async Task<IActionResult> IsDriverAvailable(int id)
        {
            try
            {
                var isAvailable = await _driverService.IsDriverAvailableAsync(id);
                return Ok(new { driverId = id, isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking driver availability: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== License Management =====

        /// <summary>
        /// Get drivers with expiring licenses
        /// GET /api/drivers/expiring-licenses?daysThreshold=30
        /// </summary>
        [HttpGet("expiring-licenses")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetDriversWithExpiringLicense([FromQuery] int daysThreshold = 30)
        {
            try
            {
                var drivers = await _driverService.GetDriversWithExpiringLicenseAsync(daysThreshold);
                return Ok(new
                {
                    daysThreshold,
                    count = drivers.Count,
                    drivers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drivers with expiring licenses");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== COD Management =====

        /// <summary>
        /// Get driver's pending COD amount
        /// GET /api/drivers/{id}/pending-cod
        /// </summary>
        [HttpGet("{id}/pending-cod")]
        [Authorize(Roles = "admin,warehouse_staff,driver")]
        public async Task<IActionResult> GetDriverPendingCod(int id)
        {
            try
            {
                // Allow driver to see their own COD
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var driver = await _driverService.GetDriverByUserIdAsync(userId);
                    if (driver != null && driver.DriverId != id && !User.IsInRole("admin") && !User.IsInRole("warehouse_staff"))
                    {
                        return Forbid();
                    }
                }

                var pendingAmount = await _driverService.GetDriverPendingCodAsync(id);
                return Ok(new
                {
                    driverId = id,
                    pendingCodAmount = pendingAmount,
                    currency = "VND"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver pending COD: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Current Driver Info (for logged-in driver) =====

        /// <summary>
        /// Get current logged-in driver's information
        /// GET /api/drivers/me
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> GetMyDriverInfo()
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var driver = await _driverService.GetDriverByUserIdAsync(userId);
                if (driver == null)
                    return NotFound(new { message = "Driver profile not found" });

                return Ok(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current driver info");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current driver's orders
        /// GET /api/drivers/me/orders?status=in_transit
        /// </summary>
        [HttpGet("me/orders")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> GetMyOrders([FromQuery] string? status = null)
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var driver = await _driverService.GetDriverByUserIdAsync(userId);
                if (driver == null)
                    return NotFound(new { message = "Driver profile not found" });

                var orders = await _driverService.GetDriverOrdersAsync(driver.DriverId, status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current driver orders");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current driver's performance
        /// GET /api/drivers/me/performance?startDate=2025-01-01&endDate=2025-01-31
        /// </summary>
        [HttpGet("me/performance")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> GetMyPerformance(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var driver = await _driverService.GetDriverByUserIdAsync(userId);
                if (driver == null)
                    return NotFound(new { message = "Driver profile not found" });

                var performance = await _driverService.GetDriverPerformanceAsync(driver.DriverId, startDate, endDate);
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current driver performance");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current driver's daily summary
        /// GET /api/drivers/me/daily-summary?date=2025-01-24
        /// </summary>
        [HttpGet("me/daily-summary")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> GetMyDailySummary([FromQuery] DateTime? date = null)
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var driver = await _driverService.GetDriverByUserIdAsync(userId);
                if (driver == null)
                    return NotFound(new { message = "Driver profile not found" });

                var summaryDate = date ?? DateTime.Now;
                var summary = await _driverService.GetDriverDailySummaryAsync(driver.DriverId, summaryDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current driver daily summary");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current driver's pending COD
        /// GET /api/drivers/me/pending-cod
        /// </summary>
        [HttpGet("me/pending-cod")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> GetMyPendingCod()
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var driver = await _driverService.GetDriverByUserIdAsync(userId);
                if (driver == null)
                    return NotFound(new { message = "Driver profile not found" });

                var pendingAmount = await _driverService.GetDriverPendingCodAsync(driver.DriverId);
                return Ok(new
                {
                    driverId = driver.DriverId,
                    pendingCodAmount = pendingAmount,
                    currency = "VND"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current driver pending COD");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}