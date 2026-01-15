using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Common;
using wedeli.Service.Interface;
using wedeli.Models.Response;

namespace wedeli.Controller
{
    /// <summary>
    /// Drivers controller for managing drivers
    /// </summary>
    [Route("api/v1/drivers")]
    [ApiController]
    [Authorize]
    public class DriversController : ControllerBase
    {
        private readonly IDriverService _driverService;
        private readonly ILogger<DriversController> _logger;

        public DriversController(
            IDriverService driverService,
            ILogger<DriversController> logger)
        {
            _driverService = driverService;
            _logger = logger;
        }

        /// <summary>
        /// Get driver by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DriverResponseDto>>> GetDriverById(int id)
        {
            try
            {
                var driver = await _driverService.GetDriverByIdAsync(id);
                return Ok(new ApiResponse<DriverResponseDto>
                {
                    Success = true,
                    Message = "Driver retrieved successfully",
                    Data = driver
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", id);
                return NotFound(new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver: {DriverId}", id);
                return StatusCode(500, new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving driver"
                });
            }
        }

        /// <summary>
        /// Get all drivers (for SuperAdmin/CompanyAdmin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DriverResponseDto>>>> GetAllDrivers()
        {
            try
            {
                var drivers = await _driverService.GetAllDriversAsync();
                return Ok(new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = true,
                    Message = "All drivers retrieved successfully",
                    Data = drivers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all drivers");
                return StatusCode(500, new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving drivers"
                });
            }
        }


        /// <summary>
        /// Get driver by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<DriverResponseDto>>> GetDriverByUserId(int userId)
        {
            try
            {
                var driver = await _driverService.GetDriverByUserIdAsync(userId);
                return Ok(new ApiResponse<DriverResponseDto>
                {
                    Success = true,
                    Message = "Driver retrieved successfully",
                    Data = driver
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found for user: {UserId}", userId);
                return NotFound(new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver by user ID: {UserId}", userId);
                return StatusCode(500, new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving driver"
                });
            }
        }

        /// <summary>
        /// Get drivers by company
        /// </summary>
        [HttpGet("company/{companyId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DriverResponseDto>>>> GetDriversByCompany(int companyId)
        {
            try
            {
                var drivers = await _driverService.GetDriversByCompanyAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = true,
                    Message = "Drivers retrieved successfully",
                    Data = drivers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers for company: {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving drivers"
                });
            }
        }

        /// <summary>
        /// Get active drivers by company
        /// </summary>
        [HttpGet("company/{companyId}/active")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DriverResponseDto>>>> GetActiveDrivers(int companyId)
        {
            try
            {
                var drivers = await _driverService.GetActiveDriversAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = true,
                    Message = "Active drivers retrieved successfully",
                    Data = drivers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active drivers for company: {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving active drivers"
                });
            }
        }

        /// <summary>
        /// Get top performing drivers
        /// </summary>
        [HttpGet("company/{companyId}/top-performers")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DriverResponseDto>>>> GetTopPerformingDrivers(
            int companyId,
            [FromQuery] int topN = 10)
        {
            try
            {
                var drivers = await _driverService.GetTopPerformingDriversAsync(companyId, topN);
                return Ok(new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = true,
                    Message = "Top performing drivers retrieved successfully",
                    Data = drivers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top performing drivers for company: {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<IEnumerable<DriverResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving top performing drivers"
                });
            }
        }

        /// <summary>
        /// Create driver
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<DriverResponseDto>>> CreateDriver(CreateDriverDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DriverResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var driver = await _driverService.CreateDriverAsync(dto);

                _logger.LogInformation("Driver created successfully: {DriverId}", driver.DriverId);
                return CreatedAtAction(nameof(GetDriverById), new { id = driver.DriverId },
                    new ApiResponse<DriverResponseDto>
                    {
                        Success = true,
                        Message = "Driver created successfully",
                        Data = driver
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found when creating driver");
                return NotFound(new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating driver");
                return BadRequest(new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                return StatusCode(500, new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = "Error creating driver"
                });
            }
        }

        /// <summary>
        /// Update driver
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<DriverResponseDto>>> UpdateDriver(int id, UpdateDriverDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DriverResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var driver = await _driverService.UpdateDriverAsync(id, dto);

                _logger.LogInformation("Driver updated successfully: {DriverId}", id);
                return Ok(new ApiResponse<DriverResponseDto>
                {
                    Success = true,
                    Message = "Driver updated successfully",
                    Data = driver
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", id);
                return NotFound(new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating driver: {DriverId}", id);
                return BadRequest(new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver: {DriverId}", id);
                return StatusCode(500, new ApiResponse<DriverResponseDto>
                {
                    Success = false,
                    Message = "Error updating driver"
                });
            }
        }

        /// <summary>
        /// Toggle driver status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleDriverStatus(int id, [FromBody] ToggleStatusRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var result = await _driverService.ToggleDriverStatusAsync(id, dto.IsActive);

                _logger.LogInformation("Driver status toggled: {DriverId}, IsActive: {IsActive}", id, dto.IsActive);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Driver status toggled successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling driver status: {DriverId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error toggling driver status"
                });
            }
        }

        /// <summary>
        /// Delete driver
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDriver(int id)
        {
            try
            {
                var result = await _driverService.DeleteDriverAsync(id);

                _logger.LogInformation("Driver deleted successfully: {DriverId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Driver deleted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete driver: {DriverId}", id);
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver: {DriverId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting driver"
                });
            }
        }

        /// <summary>
        /// Get driver performance
        /// </summary>
        [HttpGet("{id}/performance")]
        public async Task<ActionResult<ApiResponse<DriverPerformanceDto>>> GetDriverPerformance(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var performance = await _driverService.GetDriverPerformanceAsync(id, startDate, endDate);
                return Ok(new ApiResponse<DriverPerformanceDto>
                {
                    Success = true,
                    Message = "Driver performance retrieved successfully",
                    Data = performance
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", id);
                return NotFound(new ApiResponse<DriverPerformanceDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver performance: {DriverId}", id);
                return StatusCode(500, new ApiResponse<DriverPerformanceDto>
                {
                    Success = false,
                    Message = "Error retrieving driver performance"
                });
            }
        }

        /// <summary>
        /// Update driver statistics
        /// </summary>
        [HttpPost("{id}/update-statistics")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateDriverStatistics(int id)
        {
            try
            {
                var result = await _driverService.UpdateDriverStatisticsAsync(id);

                _logger.LogInformation("Driver statistics updated: {DriverId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Driver statistics updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver statistics: {DriverId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating driver statistics"
                });
            }
        }

        /// <summary>
        /// Update driver rating
        /// </summary>
        [HttpPost("{id}/update-rating")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateDriverRating(int id)
        {
            try
            {
                var result = await _driverService.UpdateDriverRatingAsync(id);

                _logger.LogInformation("Driver rating updated: {DriverId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Driver rating updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver rating: {DriverId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating driver rating"
                });
            }
        }
    }
}
