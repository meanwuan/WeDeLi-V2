using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO;
using wedeli.service.Interface;

namespace wedeli.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CodController : ControllerBase
    {
        private readonly ICodTransactionService _codService;
        private readonly ILogger<CodController> _logger;

        public CodController(ICodTransactionService codService, ILogger<CodController> logger)
        {
            _codService = codService;
            _logger = logger;
        }

        /// <summary>
        /// Get COD transaction by ID
        /// GET /api/cod/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCodTransactionById(int id)
        {
            try
            {
                var cod = await _codService.GetCodTransactionByIdAsync(id);
                if (cod == null)
                    return NotFound(new { message = "COD transaction not found" });

                return Ok(cod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting COD transaction: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get COD transaction by order ID
        /// GET /api/cod/order/{orderId}
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetCodTransactionByOrderId(int orderId)
        {
            try
            {
                var cod = await _codService.GetCodTransactionByOrderIdAsync(orderId);
                if (cod == null)
                    return NotFound(new { message = "COD transaction not found for this order" });

                return Ok(cod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting COD transaction for order: {orderId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get COD transactions with filtering
        /// GET /api/cod?companyId=1&status=pending&pageNumber=1&pageSize=20
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin,warehouse_staff,driver")]
        public async Task<IActionResult> GetCodTransactions([FromQuery] CodTransactionFilterDto filter)
        {
            try
            {
                var (cods, totalCount) = await _codService.GetCodTransactionsAsync(filter);

                return Ok(new
                {
                    data = cods,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting COD transactions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get driver's pending COD
        /// GET /api/cod/driver/{driverId}/pending
        /// </summary>
        [HttpGet("driver/{driverId}/pending")]
        [Authorize(Roles = "driver,admin")]
        public async Task<IActionResult> GetDriverPendingCod(int driverId)
        {
            try
            {
                var result = await _codService.GetDriverPendingCodAsync(driverId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Driver not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver pending COD: {driverId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Record COD collection by driver
        /// POST /api/cod/record-collection
        /// </summary>
        [HttpPost("record-collection")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> RecordCodCollection([FromBody] RecordCodCollectionDto dto)
        {
            try
            {
                var result = await _codService.RecordCodCollectionAsync(dto.CodTransactionId, dto.ProofPhotoUrl);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "COD transaction not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording COD collection");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Submit COD to company by driver
        /// POST /api/cod/submit-to-company
        /// </summary>
        [HttpPost("submit-to-company")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> SubmitCodToCompany([FromBody] SubmitCodToCompanyDto dto)
        {
            try
            {
                if (!dto.CodTransactionIds.Any())
                    return BadRequest(new { message = "No COD transactions to submit" });

                var result = await _codService.SubmitCodToCompanyAsync(dto.DriverId, dto.CodTransactionIds);
                return Ok(new { message = "COD submitted successfully", transaction = result });
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
                _logger.LogError(ex, "Error submitting COD to company");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get company's pending COD
        /// GET /api/cod/company/{companyId}/pending
        /// </summary>
        [HttpGet("company/{companyId}/pending")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetCompanyPendingCod(int companyId)
        {
            try
            {
                var result = await _codService.GetCompanyPendingCodAsync(companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting company pending COD: {companyId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get COD reconciliation for company
        /// GET /api/cod/company/{companyId}/reconciliation?date=2025-11-25
        /// </summary>
        [HttpGet("company/{companyId}/reconciliation")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetCompanyCodReconciliation(int companyId, [FromQuery] DateTime? date = null)
        {
            try
            {
                var reconciliationDate = date ?? DateTime.Now;
                var result = await _codService.GetCompanyCodReconciliationAsync(companyId, reconciliationDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting COD reconciliation: {companyId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Receive COD from driver
        /// PUT /api/cod/{codTransactionId}/receive
        /// </summary>
        [HttpPut("{codTransactionId}/receive")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> ReceiveCodFromDriver(int codTransactionId, [FromBody] int receivedByUserId)
        {
            try
            {
                var result = await _codService.ReceiveCodFromDriverAsync(codTransactionId, receivedByUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "COD transaction not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error receiving COD: {codTransactionId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Transfer COD to sender
        /// PUT /api/cod/{codTransactionId}/transfer
        /// </summary>
        [HttpPut("{codTransactionId}/transfer")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> TransferCodToSender(int codTransactionId, [FromBody] TransferCodToSenderDto dto)
        {
            try
            {
                var result = await _codService.TransferCodToSenderAsync(
                    codTransactionId,
                    dto.TransferMethod,
                    dto.TransferReference,
                    dto.TransferProofUrl
                );
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "COD transaction not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error transferring COD: {codTransactionId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
