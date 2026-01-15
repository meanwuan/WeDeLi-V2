using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO;
using wedeli.Models.DTO.Payment;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    /// <summary>
    /// Payments controller for managing order payments
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Create new payment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), 201)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Payment data is required" });

                var result = await _paymentService.CreatePaymentAsync(dto);
                if (result == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to create payment" });

                _logger.LogInformation("Payment created for order: {OrderId}", dto.OrderId);

                return CreatedAtAction(nameof(GetPaymentDetail), new { id = result.PaymentId },
                    new ApiResponse<PaymentResponseDto>
                    {
                        Success = true,
                        Message = "Payment created successfully",
                        Data = result
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error creating payment" });
            }
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), 200)]
        public async Task<IActionResult> GetPaymentDetail([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid payment ID" });

                var result = await _paymentService.GetPaymentByIdAsync(id);
                if (result == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Payment not found" });

                return Ok(new ApiResponse<PaymentResponseDto>
                {
                    Success = true,
                    Message = "Payment retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment: {PaymentId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving payment" });
            }
        }

        /// <summary>
        /// Get payments by order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentResponseDto>>), 200)]
        public async Task<IActionResult> GetPaymentsByOrder([FromRoute] int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid order ID" });

                var result = await _paymentService.GetPaymentsByOrderAsync(orderId);

                return Ok(new ApiResponse<IEnumerable<PaymentResponseDto>>
                {
                    Success = true,
                    Message = "Payments retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for order: {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving payments" });
            }
        }

        /// <summary>
        /// Get payments by customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentResponseDto>>), 200)]
        public async Task<IActionResult> GetPaymentsByCustomer([FromRoute] int customerId)
        {
            try
            {
                if (customerId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid customer ID" });

                var result = await _paymentService.GetPaymentsByCustomerAsync(customerId);

                return Ok(new ApiResponse<IEnumerable<PaymentResponseDto>>
                {
                    Success = true,
                    Message = "Payments retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for customer: {CustomerId}", customerId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving payments" });
            }
        }

        /// <summary>
        /// Get payments by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentResponseDto>>), 200)]
        public async Task<IActionResult> GetPaymentsByStatus(
            [FromRoute] string status,
            [FromQuery] int? companyId)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Status is required" });

                var result = await _paymentService.GetPaymentsByStatusAsync(status, companyId);

                return Ok(new ApiResponse<IEnumerable<PaymentResponseDto>>
                {
                    Success = true,
                    Message = "Payments retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments by status: {Status}", status);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving payments" });
            }
        }

        /// <summary>
        /// Process payment
        /// </summary>
        [HttpPost("{id}/process")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> ProcessPayment(
            [FromRoute] int id,
            [FromBody] ProcessPaymentDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid payment ID" });

                if (dto == null || string.IsNullOrEmpty(dto.TransactionReference))
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Transaction reference is required" });

                var result = await _paymentService.ProcessPaymentAsync(id, dto.TransactionReference);
                if (!result)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to process payment" });

                _logger.LogInformation("Payment processed: {PaymentId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Payment processed successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment: {PaymentId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error processing payment" });
            }
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> UpdatePaymentStatus(
            [FromRoute] int id,
            [FromBody] UpdatePaymentStatusDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid payment ID" });

                if (dto == null || string.IsNullOrEmpty(dto.PaymentStatus))
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Status is required" });

                var result = await _paymentService.UpdatePaymentStatusAsync(id, dto.PaymentStatus);
                if (!result)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to update payment status" });

                _logger.LogInformation("Payment status updated: {PaymentId}, Status: {Status}", id, dto.PaymentStatus);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Payment status updated successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status: {PaymentId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error updating payment status" });
            }
        }

        /// <summary>
        /// Refund payment
        /// </summary>
        [HttpPost("{id}/refund")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> RefundPayment(
            [FromRoute] int id,
            [FromBody] UpdatePaymentStatusDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid payment ID" });

                if (dto == null || string.IsNullOrEmpty(dto.Notes))
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Refund reason is required" });

                var result = await _paymentService.RefundPaymentAsync(id, dto.Notes);
                if (!result)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to refund payment" });

                _logger.LogInformation("Payment refunded: {PaymentId}, Reason: {Reason}", id, dto.Notes);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Payment refunded successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment: {PaymentId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error refunding payment" });
            }
        }
    }

    /// <summary>
    /// DTO for processing payments
    /// </summary>
    public class ProcessPaymentDto
    {
        public string? TransactionReference { get; set; }
    }

    /// <summary>
    /// DTO for refunding payments
    /// </summary>

}
