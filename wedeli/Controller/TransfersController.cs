using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Partnership;
using wedeli.Models.Response;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    [Authorize(Roles = "Admin,SuperAdmin,Company")]
    [ApiController]
    [Route("api/v1/transfers")]
    public class TransfersController : ControllerBase
    {
        private readonly IOrderTransferService _orderTransferService;
        private readonly ICompanyPartnershipRepository _partnershipRepository;
        private readonly IOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<TransfersController> _logger;

        public TransfersController(
            IOrderTransferService orderTransferService,
            ICompanyPartnershipRepository partnershipRepository,
            IOrderService orderService,
            IVehicleService vehicleService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<TransfersController> logger)
        {
            _orderTransferService = orderTransferService;
            _partnershipRepository = partnershipRepository;
            _orderService = orderService;
            _vehicleService = vehicleService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Transfer order to partner company
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderTransferResponseDto>), 201)]
        public async Task<IActionResult> TransferOrder([FromBody] TransferOrderDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Transfer data is required" });

                var order = await _orderService.GetOrderByIdAsync(dto.OrderId);
                if (order == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Order not found" });

                var result = await _orderTransferService.TransferOrderAsync(dto);

                // Send notification to destination company
                await _notificationService.CreateNotificationAsync(new NotificationDto
                {
                    UserId = 0,
                    OrderId = dto.OrderId,
                    NotificationType = "order_transfer",
                    Title = "Nhận yêu cầu chuyển đơn hàng",
                    Message = $"Đơn hàng {order.TrackingCode} được chuyển cho công ty của bạn",
                    SentVia = "push",
                    CreatedAt = DateTime.UtcNow
                });

                _logger.LogInformation("Order transferred successfully: {OrderId} to company {ToCompanyId}", 
                    dto.OrderId, dto.ToCompanyId);

                return CreatedAtAction(nameof(GetTransferDetail), new { id = result.TransferId }, 
                    new ApiResponse<OrderTransferResponseDto> 
                    { 
                        Success = true, 
                        Message = "Order transferred successfully", 
                        Data = result 
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid transfer operation");
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring order");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error transferring order" 
                });
            }
        }

        /// <summary>
        /// Get outgoing transfers (transfers sent by this company)
        /// </summary>
        [HttpGet("outgoing")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderTransferResponseDto>>), 200)]
        public async Task<IActionResult> GetOutgoingTransfers(
            [FromQuery] int companyId,
            [FromQuery] string? status = null,
            [FromQuery] int? toCompanyId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var transfers = await _orderTransferService.GetCompanyTransfersAsync(companyId, outgoing: true);

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    transfers = transfers.Where(t => t.TransferStatus?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);

                if (toCompanyId.HasValue)
                    transfers = transfers.Where(t => t.ToCompanyId == toCompanyId);

                var pagedTransfers = transfers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                _logger.LogInformation("Retrieved {Count} outgoing transfers for company: {CompanyId}", 
                    pagedTransfers.Count, companyId);

                return Ok(new ApiResponse<IEnumerable<OrderTransferResponseDto>>
                {
                    Success = true,
                    Message = "Outgoing transfers retrieved successfully",
                    Data = pagedTransfers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving outgoing transfers");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error retrieving outgoing transfers" 
                });
            }
        }

        /// <summary>
        /// Get incoming transfers (transfers received by this company)
        /// </summary>
        [HttpGet("incoming")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderTransferResponseDto>>), 200)]
        public async Task<IActionResult> GetIncomingTransfers(
            [FromQuery] int companyId,
            [FromQuery] string? status = null,
            [FromQuery] int? fromCompanyId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var transfers = await _orderTransferService.GetCompanyTransfersAsync(companyId, outgoing: false);

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    transfers = transfers.Where(t => t.TransferStatus?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);

                if (fromCompanyId.HasValue)
                    transfers = transfers.Where(t => t.FromCompanyId == fromCompanyId);

                var pagedTransfers = transfers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                _logger.LogInformation("Retrieved {Count} incoming transfers for company: {CompanyId}", 
                    pagedTransfers.Count, companyId);

                return Ok(new ApiResponse<IEnumerable<OrderTransferResponseDto>>
                {
                    Success = true,
                    Message = "Incoming transfers retrieved successfully",
                    Data = pagedTransfers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incoming transfers");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error retrieving incoming transfers" 
                });
            }
        }

        /// <summary>
        /// Get transfer detail by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderTransferResponseDto>), 200)]
        public async Task<IActionResult> GetTransferDetail([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid transfer ID" });

                var transfer = await _orderTransferService.GetTransferByIdAsync(id);
                if (transfer == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Transfer not found" });

                _logger.LogInformation("Retrieved transfer detail: {TransferId}", id);

                return Ok(new ApiResponse<OrderTransferResponseDto>
                {
                    Success = true,
                    Message = "Transfer retrieved successfully",
                    Data = transfer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfer detail");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error retrieving transfer" 
                });
            }
        }

        /// <summary>
        /// Accept transfer - destination company accepts the order transfer
        /// </summary>
        [HttpPost("{id}/accept")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> AcceptTransfer([FromRoute] int id, [FromBody] AcceptTransferDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid transfer ID" });

                if (dto == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Acceptance data is required" });

                var result = await _orderTransferService.AcceptTransferAsync(id, dto.NewVehicleId);
                if (!result)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Transfer not found" });

                _logger.LogInformation("Transfer accepted: {TransferId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Transfer accepted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting transfer");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error accepting transfer" 
                });
            }
        }

        /// <summary>
        /// Reject transfer - destination company rejects the order transfer
        /// </summary>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> RejectTransfer([FromRoute] int id, [FromBody] RejectTransferDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid transfer ID" });

                if (dto == null || string.IsNullOrEmpty(dto.Reason))
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Rejection reason is required" });

                var result = await _orderTransferService.RejectTransferAsync(id, dto.Reason);
                if (!result)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Transfer not found" });

                _logger.LogInformation("Transfer rejected: {TransferId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Transfer rejected successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting transfer");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error rejecting transfer" 
                });
            }
        }

        /// <summary>
        /// Cancel transfer - source company cancels the transfer (only if pending)
        /// </summary>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> CancelTransfer([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid transfer ID" });

                var transfer = await _orderTransferService.GetTransferByIdAsync(id);
                if (transfer == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Transfer not found" });

                if (transfer.TransferStatus != "pending")
                    return BadRequest(new ApiResponse<string> 
                    { 
                        Success = false, 
                        Message = "Can only cancel pending transfers" 
                    });

                await _orderTransferService.RejectTransferAsync(id, "Cancelled by source company");

                _logger.LogInformation("Transfer cancelled: {TransferId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Transfer cancelled successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling transfer");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error cancelling transfer" 
                });
            }
        }

        /// <summary>
        /// Get pending transfers requiring action
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderTransferResponseDto>>), 200)]
        public async Task<IActionResult> GetPendingTransfers([FromQuery] int companyId)
        {
            try
            {
                var transfers = await _orderTransferService.GetPendingTransfersAsync(companyId);
                var pendingTransfers = transfers.ToList();

                _logger.LogInformation("Retrieved {Count} pending transfers for company: {CompanyId}", 
                    pendingTransfers.Count, companyId);

                return Ok(new ApiResponse<IEnumerable<OrderTransferResponseDto>>
                {
                    Success = true,
                    Message = "Pending transfers retrieved successfully",
                    Data = pendingTransfers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending transfers");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error retrieving pending transfers" 
                });
            }
        }

        /// <summary>
        /// Get transfer history by order ID
        /// </summary>
        [HttpGet("history/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderTransferResponseDto>>), 200)]
        public async Task<IActionResult> GetTransferHistory([FromRoute] int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid order ID" });

                var transfers = await _orderTransferService.GetTransfersByOrderAsync(orderId);
                if (transfers == null || !transfers.Any())
                    return NotFound(new ApiResponse<string> { Success = false, Message = "No transfer history found" });

                _logger.LogInformation("Retrieved transfer history for order: {OrderId}", orderId);

                return Ok(new ApiResponse<IEnumerable<OrderTransferResponseDto>>
                {
                    Success = true,
                    Message = "Transfer history retrieved successfully",
                    Data = transfers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfer history");
                return StatusCode(500, new ApiResponse<string> 
                { 
                    Success = false, 
                    Message = "Error retrieving transfer history" 
                });
            }
        }
    }

    /// <summary>
    /// DTO for accepting transfer
    /// </summary>
    public class AcceptTransferDto
    {
        public int? NewVehicleId { get; set; }
    }

    /// <summary>
    /// DTO for rejecting transfer
    /// </summary>
    public class RejectTransferDto
    {
        public string? Reason { get; set; }
    }
}
