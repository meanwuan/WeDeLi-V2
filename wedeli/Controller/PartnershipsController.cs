using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Company;
using wedeli.Models.DTO.Partnership;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using wedeli.Models.Response;

namespace wedeli.Controller
{
    /// <summary>
    /// Partnerships Controller for managing company partnerships and order transfers
    /// </summary>
    [ApiController]
    [Route("api/v1/partnerships")]
    [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
    public class PartnershipsController : ControllerBase
    {
        private readonly IPartnershipService _partnershipService;
        private readonly IOrderTransferService _transferService;
        private readonly ITransportCompanyService _companyService;
        private readonly ITransportCompanyRepository _companyRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly ILogger<PartnershipsController> _logger;

        public PartnershipsController(
            IPartnershipService partnershipService,
            IOrderTransferService transferService,
            ITransportCompanyService companyService,
            ITransportCompanyRepository companyRepository,
            IRouteRepository routeRepository,
            ILogger<PartnershipsController> logger)
        {
            _partnershipService = partnershipService;
            _transferService = transferService;
            _companyService = companyService;
            _companyRepository = companyRepository;
            _routeRepository = routeRepository;
            _logger = logger;
        }

        // ============================================
        // PARTNERSHIP ENDPOINTS
        // ============================================

        /// <summary>
        /// Get company partnerships with paging and filtering
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="companyId">Company ID</param>
        /// <param name="partnershipLevel">Filter by level (preferred, regular, backup)</param>
        /// <param name="isActive">Filter by active status</param>
        /// <returns>Paginated partnerships</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PartnershipResponseDto>>>> GetPartnerships(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? companyId = null,
            [FromQuery] string? partnershipLevel = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                if (!companyId.HasValue || companyId <= 0)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Company ID is required"
                    });

                IEnumerable<PartnershipResponseDto> result;

                if (!string.IsNullOrEmpty(partnershipLevel))
                {
                    result = await _partnershipService.GetPartnersByLevelAsync(companyId.Value, partnershipLevel);
                }
                else
                {
                    result = await _partnershipService.GetCompanyPartnershipsAsync(companyId.Value);
                }

                // Apply paging
                var pagedResult = result
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new ApiResponse<IEnumerable<PartnershipResponseDto>>
                {
                    Success = true,
                    Message = "Partnerships retrieved successfully",
                    Data = pagedResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partnerships");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving partnerships"
                });
            }
        }

        /// <summary>
        /// Get partnership by ID
        /// </summary>
        /// <param name="id">Partnership ID</param>
        /// <returns>Partnership details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PartnershipResponseDto>>> GetPartnership(int id)
        {
            try
            {
                var result = await _partnershipService.GetPartnershipByIdAsync(id);
                return Ok(new ApiResponse<PartnershipResponseDto>
                {
                    Success = true,
                    Message = "Partnership retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Partnership not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partnership");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving partnership"
                });
            }
        }

        /// <summary>
        /// Create partnership
        /// </summary>
        /// <param name="dto">Create partnership request</param>
        /// <returns>Created partnership</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PartnershipResponseDto>>> CreatePartnership(
            [FromBody] CreatePartnershipDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _partnershipService.CreatePartnershipAsync(dto);
                return CreatedAtAction(nameof(GetPartnership), new { id = result.PartnershipId },
                    new ApiResponse<PartnershipResponseDto>
                    {
                        Success = true,
                        Message = "Partnership created successfully",
                        Data = result
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Company not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partnership");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error creating partnership"
                });
            }
        }

        /// <summary>
        /// Update partnership
        /// </summary>
        /// <param name="id">Partnership ID</param>
        /// <param name="dto">Update partnership request</param>
        /// <returns>Updated partnership</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PartnershipResponseDto>>> UpdatePartnership(
            int id,
            [FromBody] UpdatePartnershipDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _partnershipService.UpdatePartnershipAsync(id, dto);
                return Ok(new ApiResponse<PartnershipResponseDto>
                {
                    Success = true,
                    Message = "Partnership updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Partnership not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partnership");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating partnership"
                });
            }
        }

        /// <summary>
        /// Delete partnership
        /// </summary>
        /// <param name="id">Partnership ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePartnership(int id)
        {
            try
            {
                var result = await _partnershipService.DeletePartnershipAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Partnership deleted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Partnership not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partnership");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting partnership"
                });
            }
        }

        /// <summary>
        /// Toggle partnership status
        /// </summary>
        /// <param name="id">Partnership ID</param>
        /// <returns>New active status</returns>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse<object>>> ToggleStatus(int id)
        {
            try
            {
                var partnership = await _partnershipService.GetPartnershipByIdAsync(id);
                var newStatus = !(partnership.IsActive);

                var updateDto = new UpdatePartnershipDto { IsActive = newStatus };
                await _partnershipService.UpdatePartnershipAsync(id, updateDto);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Partnership status toggled successfully",
                    Data = new { isActive = newStatus }
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Partnership not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling partnership status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error toggling partnership status"
                });
            }
        }

        /// <summary>
        /// Update commission rate
        /// </summary>
        /// <param name="id">Partnership ID</param>
        /// <param name="dto">Commission rate update request</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}/commission-rate")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCommissionRate(
            int id,
            [FromBody] UpdateCommissionRateDto dto)
        {
            try
            {
                if (dto == null || dto.CommissionRate < 0 || dto.CommissionRate > 100)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Commission rate must be between 0 and 100"
                    });

                var result = await _partnershipService.UpdateCommissionRateAsync(id, dto.CommissionRate);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Commission rate updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Partnership not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating commission rate");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating commission rate"
                });
            }
        }

        /// <summary>
        /// Update priority order
        /// </summary>
        /// <param name="id">Partnership ID</param>
        /// <param name="dto">Priority update request</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}/priority")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdatePriority(
            int id,
            [FromBody] UpdatePriorityDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _partnershipService.UpdatePriorityAsync(id, dto.PriorityOrder);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Priority updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Partnership not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating priority");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating priority"
                });
            }
        }

        /// <summary>
        /// Get preferred partners
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of preferred partners</returns>
        [HttpGet("preferred")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PartnershipResponseDto>>>> GetPreferredPartners(
            [FromQuery] int companyId)
        {
            try
            {
                if (companyId <= 0)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Company ID is required"
                    });

                var result = await _partnershipService.GetPreferredPartnersAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<PartnershipResponseDto>>
                {
                    Success = true,
                    Message = "Preferred partners retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferred partners");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving preferred partners"
                });
            }
        }

        // ============================================
        // COMPANY ENDPOINTS
        // ============================================

        /// <summary>
        /// Get all companies for partner selection
        /// </summary>
        /// <param name="search">Search by company name</param>
        /// <param name="isActive">Filter by active status</param>
        /// <returns>List of companies</returns>
        [HttpGet("companies")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CompanyResponseDto>>>> GetCompanies(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = true)
        {
            try
            {
                var companies = await _companyRepository.GetActiveCompaniesAsync();
                
                if (!string.IsNullOrEmpty(search))
                {
                    companies = companies.Where(c => c.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                var companyDtos = companies.Select(c => new CompanyResponseDto
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    BusinessLicense = c.BusinessLicense ?? "",
                    Address = c.Address ?? "",
                    Phone = c.Phone ?? "",
                    Email = c.Email ?? "",
                    IsActive = c.IsActive ?? true,
                    Rating = c.Rating ?? 0,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow
                }).ToList();
                return Ok(new ApiResponse<IEnumerable<CompanyResponseDto>>
                {
                    Success = true,
                    Message = "Companies retrieved successfully",
                    Data = companyDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving companies");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving companies"
                });
            }
        }

        /// <summary>
        /// Get company details
        /// </summary>
        /// <param name="id">Company ID</param>
        /// <returns>Company details</returns>
        [HttpGet("companies/{id}")]
        public async Task<ActionResult<ApiResponse<CompanyResponseDto>>> GetCompanyDetails(int id)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(id);
                
                var companyDto = new CompanyResponseDto
                {
                    CompanyId = company.CompanyId,
                    CompanyName = company.CompanyName,
                    BusinessLicense = company.BusinessLicense,
                    Address = company.Address,
                    Phone = company.Phone,
                    Email = company.Email,
                    IsActive = company.IsActive ?? true,
                    Rating = company.Rating ?? 0,
                    Latitude = company.Latitude,
                    Longitude = company.Longitude,
                    CreatedAt = company.CreatedAt ?? DateTime.UtcNow
                };

                return Ok(new ApiResponse<CompanyResponseDto>
                {
                    Success = true,
                    Message = "Company retrieved successfully",
                    Data = companyDto
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Company not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving company"
                });
            }
        }

        // ============================================
        // ORDER TRANSFER ENDPOINTS
        // ============================================

        /// <summary>
        /// Transfer order to partner company
        /// </summary>
        /// <param name="dto">Transfer order request</param>
        /// <returns>Created transfer</returns>
        [HttpPost("orders/transfer")]
        public async Task<ActionResult<ApiResponse<OrderTransferResponseDto>>> TransferOrder(
            [FromBody] TransferOrderDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _transferService.TransferOrderAsync(dto);
                return CreatedAtAction(nameof(GetTransfer), new { id = result.TransferId },
                    new ApiResponse<OrderTransferResponseDto>
                    {
                        Success = true,
                        Message = "Order transferred successfully",
                        Data = result
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Resource not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring order");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error transferring order"
                });
            }
        }

        /// <summary>
        /// Get transfer by ID
        /// </summary>
        /// <param name="id">Transfer ID</param>
        /// <returns>Transfer details</returns>
        [HttpGet("transfers/{id}")]
        public async Task<ActionResult<ApiResponse<OrderTransferResponseDto>>> GetTransfer(int id)
        {
            try
            {
                var result = await _transferService.GetTransferByIdAsync(id);
                return Ok(new ApiResponse<OrderTransferResponseDto>
                {
                    Success = true,
                    Message = "Transfer retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Transfer not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfer");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving transfer"
                });
            }
        }

        /// <summary>
        /// Get transfers for company (outgoing or incoming)
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="outgoing">Outgoing transfers (true) or incoming (false)</param>
        /// <returns>List of transfers</returns>
        [HttpGet("transfers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderTransferResponseDto>>>> GetCompanyTransfers(
            [FromQuery] int companyId,
            [FromQuery] bool outgoing = true)
        {
            try
            {
                if (companyId <= 0)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Company ID is required"
                    });

                var result = await _transferService.GetCompanyTransfersAsync(companyId, outgoing);
                return Ok(new ApiResponse<IEnumerable<OrderTransferResponseDto>>
                {
                    Success = true,
                    Message = "Transfers retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfers");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving transfers"
                });
            }
        }

        /// <summary>
        /// Get pending transfers for company
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of pending transfers</returns>
        [HttpGet("transfers/pending/{companyId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderTransferResponseDto>>>> GetPendingTransfers(
            int companyId)
        {
            try
            {
                if (companyId <= 0)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Company ID is required"
                    });

                var result = await _transferService.GetPendingTransfersAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<OrderTransferResponseDto>>
                {
                    Success = true,
                    Message = "Pending transfers retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending transfers");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving pending transfers"
                });
            }
        }

        /// <summary>
        /// Accept transfer
        /// </summary>
        /// <param name="id">Transfer ID</param>
        /// <param name="dto">Accept transfer request</param>
        /// <returns>Success status</returns>
        [HttpPost("transfers/{id}/accept")]
        public async Task<ActionResult<ApiResponse<bool>>> AcceptTransfer(
            int id,
            [FromBody] AcceptTransferDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _transferService.AcceptTransferAsync(id, dto.NewVehicleId);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Transfer accepted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Resource not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting transfer");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error accepting transfer"
                });
            }
        }

        /// <summary>
        /// Reject transfer
        /// </summary>
        /// <param name="id">Transfer ID</param>
        /// <param name="dto">Reject transfer request</param>
        /// <returns>Success status</returns>
        [HttpPost("transfers/{id}/reject")]
        public async Task<ActionResult<ApiResponse<bool>>> RejectTransfer(
            int id,
            [FromBody] RejectTransferDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Reason))
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Rejection reason is required"
                    });

                var result = await _transferService.RejectTransferAsync(id, dto.Reason);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Transfer rejected successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Transfer not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting transfer");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error rejecting transfer"
                });
            }
        }
    }

    // ============================================
    // HELPER DTOs
    // ============================================

    /// <summary>
    /// Update commission rate DTO
    /// </summary>
    public class UpdateCommissionRateDto
    {
        public decimal CommissionRate { get; set; }
    }

    /// <summary>
    /// Update priority DTO
    /// </summary>
    public class UpdatePriorityDto
    {
        public int PriorityOrder { get; set; }
    }
}
