using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Common;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    /// <summary>
    /// Warehouse Staff controller for managing warehouse staff
    /// </summary>
    [Route("api/v1/staff")]
    [ApiController]
    [Authorize]
    public class WarehouseStaffController : ControllerBase
    {
        private readonly IWarehouseStaffService _staffService;
        private readonly ILogger<WarehouseStaffController> _logger;

        public WarehouseStaffController(
            IWarehouseStaffService staffService,
            ILogger<WarehouseStaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        /// <summary>
        /// Get staff by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<WarehouseStaffDto>>> GetStaffById(int id)
        {
            try
            {
                var staff = await _staffService.GetStaffByIdAsync(id);
                return Ok(new ApiResponse<WarehouseStaffDto>
                {
                    Success = true,
                    Message = "Staff retrieved successfully",
                    Data = staff
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Staff not found: {StaffId}", id);
                return NotFound(new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff: {StaffId}", id);
                return StatusCode(500, new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = "Error retrieving staff"
                });
            }
        }

        /// <summary>
        /// Get all staff (for SuperAdmin/CompanyAdmin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WarehouseStaffDto>>>> GetAllStaff()
        {
            try
            {
                var staffList = await _staffService.GetAllStaffAsync();
                return Ok(new ApiResponse<IEnumerable<WarehouseStaffDto>>
                {
                    Success = true,
                    Message = "All staff retrieved successfully",
                    Data = staffList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all staff");
                return StatusCode(500, new ApiResponse<IEnumerable<WarehouseStaffDto>>
                {
                    Success = false,
                    Message = "Error retrieving staff"
                });
            }
        }


        /// <summary>
        /// Get staff by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<WarehouseStaffDto>>> GetStaffByUserId(int userId)
        {
            try
            {
                var staff = await _staffService.GetStaffByUserIdAsync(userId);
                return Ok(new ApiResponse<WarehouseStaffDto>
                {
                    Success = true,
                    Message = "Staff retrieved successfully",
                    Data = staff
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Staff not found for user: {UserId}", userId);
                return NotFound(new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff by user ID: {UserId}", userId);
                return StatusCode(500, new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = "Error retrieving staff"
                });
            }
        }

        /// <summary>
        /// Get staff by company
        /// </summary>
        [HttpGet("company/{companyId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WarehouseStaffDto>>>> GetStaffByCompany(int companyId)
        {
            try
            {
                var staffList = await _staffService.GetStaffByCompanyAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<WarehouseStaffDto>>
                {
                    Success = true,
                    Message = "Staff retrieved successfully",
                    Data = staffList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff for company: {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<IEnumerable<WarehouseStaffDto>>
                {
                    Success = false,
                    Message = "Error retrieving staff"
                });
            }
        }

        /// <summary>
        /// Create staff
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<WarehouseStaffDto>>> CreateStaff(CreateWarehouseStaffDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<WarehouseStaffDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var staff = await _staffService.CreateStaffAsync(dto);

                _logger.LogInformation("Staff created successfully: {StaffId}", staff.StaffId);
                return CreatedAtAction(nameof(GetStaffById), new { id = staff.StaffId },
                    new ApiResponse<WarehouseStaffDto>
                    {
                        Success = true,
                        Message = "Staff created successfully",
                        Data = staff
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found when creating staff");
                return NotFound(new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating staff");
                return BadRequest(new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff");
                return StatusCode(500, new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = "Error creating staff"
                });
            }
        }

        /// <summary>
        /// Update staff
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<WarehouseStaffDto>>> UpdateStaff(int id, UpdateWarehouseStaffDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<WarehouseStaffDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var staff = await _staffService.UpdateStaffAsync(id, dto);

                _logger.LogInformation("Staff updated successfully: {StaffId}", id);
                return Ok(new ApiResponse<WarehouseStaffDto>
                {
                    Success = true,
                    Message = "Staff updated successfully",
                    Data = staff
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Staff not found: {StaffId}", id);
                return NotFound(new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating staff: {StaffId}", id);
                return BadRequest(new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff: {StaffId}", id);
                return StatusCode(500, new ApiResponse<WarehouseStaffDto>
                {
                    Success = false,
                    Message = "Error updating staff"
                });
            }
        }

        /// <summary>
        /// Toggle staff status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleStaffStatus(int id, [FromBody] ToggleStatusRequestDto dto)
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

                var result = await _staffService.ToggleStaffStatusAsync(id, dto.IsActive);

                _logger.LogInformation("Staff status toggled: {StaffId}, IsActive: {IsActive}", id, dto.IsActive);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Staff status toggled successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Staff not found: {StaffId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling staff status: {StaffId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error toggling staff status"
                });
            }
        }

        /// <summary>
        /// Delete staff
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteStaff(int id)
        {
            try
            {
                var result = await _staffService.DeleteStaffAsync(id);

                _logger.LogInformation("Staff deleted successfully: {StaffId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Staff deleted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Staff not found: {StaffId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete staff: {StaffId}", id);
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff: {StaffId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting staff"
                });
            }
        }
    }
}
