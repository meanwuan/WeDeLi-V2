using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.COD;
using wedeli.Models.DTO.Common;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    /// <summary>
    /// COD (Cash On Delivery) Controller for managing COD transactions
    /// </summary>
    [ApiController]
    [Route("api/v1/cod")]
    [Authorize]
    public class CODController : ControllerBase
    {
        private readonly ICODService _codService;
        private readonly ILogger<CODController> _logger;

        public CODController(ICODService codService, ILogger<CODController> logger)
        {
            _codService = codService;
            _logger = logger;
        }

        /// <summary>
        /// Get COD transaction by ID
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>COD transaction details</returns>
        [HttpGet("{transactionId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<CodTransactionResponseDto>>> GetCODTransaction(int transactionId)
        {
            try
            {
                var result = await _codService.GetCODTransactionAsync(transactionId);
                return Ok(new ApiResponse<CodTransactionResponseDto>
                {
                    Success = true,
                    Message = "COD transaction retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("COD transaction not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD transaction");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving COD transaction"
                });
            }
        }

        /// <summary>
        /// Get COD by order ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>COD transaction details</returns>
        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<CodTransactionResponseDto>>> GetCODByOrder(int orderId)
        {
            try
            {
                var result = await _codService.GetCODByOrderAsync(orderId);
                return Ok(new ApiResponse<CodTransactionResponseDto>
                {
                    Success = true,
                    Message = "COD transaction retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("COD transaction not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD transaction by order");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving COD transaction"
                });
            }
        }

        /// <summary>
        /// Get driver COD transactions
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <param name="status">Optional status filter</param>
        /// <returns>List of COD transactions</returns>
        [HttpGet("driver/{driverId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CodTransactionResponseDto>>>> GetDriverCODTransactions(int driverId, [FromQuery] string? status = null)
        {
            try
            {
                var result = await _codService.GetDriverCODTransactionsAsync(driverId, status ?? "");
                return Ok(new ApiResponse<IEnumerable<CodTransactionResponseDto>>
                {
                    Success = true,
                    Message = "Driver COD transactions retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver COD transactions: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving driver COD transactions"
                });
            }
        }

        /// <summary>
        /// Get pending COD collections for driver
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <returns>List of pending COD transactions</returns>
        [HttpGet("driver/{driverId}/pending-collections")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CodTransactionResponseDto>>>> GetPendingCollections(int driverId)
        {
            try
            {
                var result = await _codService.GetPendingCollectionsAsync(driverId);
                return Ok(new ApiResponse<IEnumerable<CodTransactionResponseDto>>
                {
                    Success = true,
                    Message = "Pending COD collections retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending COD collections: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving pending COD collections"
                });
            }
        }

        /// <summary>
        /// Collect COD at delivery
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="driverId">Driver ID</param>
        /// <param name="collectionProofPhoto">Optional proof photo URL</param>
        /// <returns>Success status</returns>
        [HttpPost("{orderId}/collect")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> CollectCOD(int orderId, [FromBody] CollectCodDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _codService.CollectCODAsync(dto.OrderId, orderId, dto.CollectionProofPhotoUrl);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "COD collected successfully",
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting COD: {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error collecting COD"
                });
            }
        }

        /// <summary>
        /// Submit collected COD to company
        /// </summary>
        /// <param name="dto">Submit COD request data</param>
        /// <returns>Success status</returns>
        [HttpPost("submit")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> SubmitCODToCompany([FromBody] SubmitCodDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _codService.SubmitCODToCompanyAsync(dto);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "COD submitted to company successfully",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
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
                _logger.LogError(ex, "Error submitting COD to company");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error submitting COD to company"
                });
            }
        }

        /// <summary>
        /// Get driver pending COD amount
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <returns>Pending amount</returns>
        [HttpGet("driver/{driverId}/pending-amount")]
        [Authorize(Roles = "Driver,Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetDriverPendingCOD(int driverId)
        {
            try
            {
                var result = await _codService.GetDriverPendingCODAsync(driverId);
                return Ok(new ApiResponse<decimal>
                {
                    Success = true,
                    Message = "Driver pending COD amount retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver pending COD: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving driver pending COD"
                });
            }
        }

        /// <summary>
        /// Confirm COD receipt (Company/Admin)
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="receivedBy">User ID of receiver</param>
        /// <returns>Success status</returns>
        [HttpPatch("{transactionId}/confirm-receipt")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmCODReceipt(int transactionId, [FromQuery] int receivedBy)
        {
            try
            {
                var result = await _codService.ConfirmCODReceiptAsync(transactionId, receivedBy);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "COD receipt confirmed successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("COD transaction not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming COD receipt: {TransactionId}", transactionId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error confirming COD receipt"
                });
            }
        }

        /// <summary>
        /// Transfer COD to sender
        /// </summary>
        /// <param name="dto">Transfer request data</param>
        /// <returns>Success status</returns>
        [HttpPost("transfer")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> TransferToSender([FromBody] TransferToSenderDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _codService.TransferToSenderAsync(dto);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "COD transferred to sender successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("COD transaction not found: {Message}", ex.Message);
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
                _logger.LogError(ex, "Error transferring COD to sender");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error transferring COD to sender"
                });
            }
        }

        /// <summary>
        /// Get driver COD summary for date
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <param name="date">Date to get summary for</param>
        /// <returns>COD summary</returns>
        [HttpGet("driver/{driverId}/summary")]
        [Authorize(Roles = "Driver,Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<CodDashboardDto>>> GetDriverCODSummary(int driverId, [FromQuery] DateTime? date = null)
        {
            try
            {
                var queryDate = date ?? DateTime.UtcNow.Date;
                var result = await _codService.GetDriverCODSummaryAsync(driverId, queryDate);
                return Ok(new ApiResponse<CodDashboardDto>
                {
                    Success = true,
                    Message = "Driver COD summary retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver COD summary: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving driver COD summary"
                });
            }
        }

        /// <summary>
        /// Get pending reconciliations
        /// </summary>
        /// <param name="companyId">Optional company filter</param>
        /// <returns>List of pending reconciliations</returns>
        [HttpGet("pending-reconciliations")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CodDashboardDto>>>> GetPendingReconciliations([FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _codService.GetPendingReconciliationsAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<CodDashboardDto>>
                {
                    Success = true,
                    Message = "Pending reconciliations retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reconciliations");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving pending reconciliations"
                });
            }
        }

        /// <summary>
        /// Reconcile driver COD
        /// </summary>
        /// <param name="summaryId">Summary ID</param>
        /// <param name="reconciledBy">User ID of reconciler</param>
        /// <returns>Success status</returns>
        [HttpPost("{summaryId}/reconcile")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> ReconcileDriverCOD(int summaryId, [FromQuery] int reconciledBy)
        {
            try
            {
                var result = await _codService.ReconcileDriverCODAsync(summaryId, reconciledBy);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Driver COD reconciled successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling driver COD: {SummaryId}", summaryId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error reconciling driver COD"
                });
            }
        }

        /// <summary>
        /// Reconcile all drivers COD for date
        /// </summary>
        /// <param name="date">Date to reconcile</param>
        /// <param name="companyId">Company ID</param>
        /// <param name="reconciledBy">User ID of reconciler</param>
        /// <returns>Success status</returns>
        [HttpPost("reconcile-all")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> ReconcileAllDrivers([FromQuery] DateTime date, [FromQuery] int companyId, [FromQuery] int reconciledBy)
        {
            try
            {
                var result = await _codService.ReconcileAllDriversAsync(date, companyId, reconciledBy);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "All drivers COD reconciled successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling all drivers COD");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error reconciling all drivers COD"
                });
            }
        }

        /// <summary>
        /// Get COD dashboard
        /// </summary>
        /// <param name="companyId">Optional company filter</param>
        /// <returns>COD dashboard data</returns>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<CodDashboardDto>>> GetCODDashboard([FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _codService.GetCODDashboardAsync(companyId);
                return Ok(new ApiResponse<CodDashboardDto>
                {
                    Success = true,
                    Message = "COD dashboard retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD dashboard");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving COD dashboard"
                });
            }
        }
    }
}
