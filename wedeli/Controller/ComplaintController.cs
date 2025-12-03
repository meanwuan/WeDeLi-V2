using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Complaint;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    /// <summary>
    /// Complaints controller for managing order complaints
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/complaints")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly ILogger<ComplaintController> _logger;

        public ComplaintController(
            IComplaintService complaintService,
            ILogger<ComplaintController> logger)
        {
            _complaintService = complaintService;
            _logger = logger;
        }

        /// <summary>
        /// Create new complaint
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ComplaintResponseDto>), 201)]
        public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Complaint data is required" });

                var result = await _complaintService.CreateComplaintAsync(dto);
                if (result == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to create complaint" });

                _logger.LogInformation("Complaint created for order: {OrderId}", dto.OrderId);

                return CreatedAtAction(nameof(GetComplaintDetail), new { id = result.ComplaintId },
                    new ApiResponse<ComplaintResponseDto>
                    {
                        Success = true,
                        Message = "Complaint created successfully",
                        Data = result
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating complaint");
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error creating complaint" });
            }
        }

        /// <summary>
        /// Get complaint by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ComplaintResponseDto>), 200)]
        public async Task<IActionResult> GetComplaintDetail([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid complaint ID" });

                var result = await _complaintService.GetComplaintByIdAsync(id);
                if (result == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Complaint not found" });

                return Ok(new ApiResponse<ComplaintResponseDto>
                {
                    Success = true,
                    Message = "Complaint retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complaint: {ComplaintId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving complaint" });
            }
        }

        /// <summary>
        /// Get complaints by order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComplaintResponseDto>>), 200)]
        public async Task<IActionResult> GetComplaintsByOrder([FromRoute] int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid order ID" });

                var result = await _complaintService.GetComplaintsByOrderAsync(orderId);

                return Ok(new ApiResponse<IEnumerable<ComplaintResponseDto>>
                {
                    Success = true,
                    Message = "Complaints retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complaints for order: {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving complaints" });
            }
        }

        /// <summary>
        /// Get complaints by customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComplaintResponseDto>>), 200)]
        public async Task<IActionResult> GetComplaintsByCustomer([FromRoute] int customerId)
        {
            try
            {
                if (customerId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid customer ID" });

                var result = await _complaintService.GetComplaintsByCustomerAsync(customerId);

                return Ok(new ApiResponse<IEnumerable<ComplaintResponseDto>>
                {
                    Success = true,
                    Message = "Complaints retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complaints for customer: {CustomerId}", customerId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving complaints" });
            }
        }

        /// <summary>
        /// Get complaints by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComplaintResponseDto>>), 200)]
        public async Task<IActionResult> GetComplaintsByStatus(
            [FromRoute] string status,
            [FromQuery] int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Status is required" });

                var result = await _complaintService.GetComplaintsByStatusAsync(status, companyId);

                return Ok(new ApiResponse<IEnumerable<ComplaintResponseDto>>
                {
                    Success = true,
                    Message = "Complaints retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complaints by status: {Status}", status);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving complaints" });
            }
        }

        /// <summary>
        /// Get pending complaints
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComplaintResponseDto>>), 200)]
        public async Task<IActionResult> GetPendingComplaints([FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _complaintService.GetPendingComplaintsAsync(companyId);

                return Ok(new ApiResponse<IEnumerable<ComplaintResponseDto>>
                {
                    Success = true,
                    Message = "Pending complaints retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending complaints");
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving pending complaints" });
            }
        }

        /// <summary>
        /// Resolve complaint
        /// </summary>
        [HttpPost("{id}/resolve")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> ResolveComplaint(
            [FromRoute] int id,
            [FromBody] ResolveComplaintDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid complaint ID" });

                if (dto == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Resolution data is required" });

                var result = await _complaintService.ResolveComplaintAsync(dto);
                if (!result)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to resolve complaint" });

                _logger.LogInformation("Complaint resolved: {ComplaintId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Complaint resolved successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving complaint: {ComplaintId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error resolving complaint" });
            }
        }

        /// <summary>
        /// Reject complaint
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> RejectComplaint(
            [FromRoute] int id,
            [FromBody] RejectComplaintDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid complaint ID" });

                if (dto == null || string.IsNullOrEmpty(dto.Reason))
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Rejection reason is required" });

                var result = await _complaintService.RejectComplaintAsync(id, dto.Reason, dto.RejectedBy);
                if (!result)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to reject complaint" });

                _logger.LogInformation("Complaint rejected: {ComplaintId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Complaint rejected successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting complaint: {ComplaintId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error rejecting complaint" });
            }
        }
    }

    /// <summary>
    /// DTO for rejecting complaint
    /// </summary>
    public class RejectComplaintDto
    {
        public string? Reason { get; set; }
        public int RejectedBy { get; set; }
    }
}
