using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controllers;

/// <summary>
/// API Controller for managing company-specific customers and their pricing
/// </summary>
[ApiController]
[Route("api/v1/company-customers")]
[Authorize]
public class CompanyCustomersController : ControllerBase
{
    private readonly ICompanyCustomerService _service;
    private readonly ILogger<CompanyCustomersController> _logger;

    public CompanyCustomersController(
        ICompanyCustomerService service,
        ILogger<CompanyCustomersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers for a company
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CompanyCustomerResponseDto>>>> GetByCompany(
        [FromQuery] int companyId)
    {
        try
        {
            var customers = await _service.GetCompanyCustomersAsync(companyId);
            return Ok(ApiResponse<IEnumerable<CompanyCustomerResponseDto>>.SuccessResponse(customers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company customers for company {CompanyId}", companyId);
            return StatusCode(500, ApiResponse<IEnumerable<CompanyCustomerResponseDto>>.ErrorResponse("Lỗi khi lấy danh sách khách hàng"));
        }
    }

    /// <summary>
    /// Get VIP customers for a company
    /// </summary>
    [HttpGet("vip")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CompanyCustomerResponseDto>>>> GetVipCustomers(
        [FromQuery] int companyId)
    {
        try
        {
            var customers = await _service.GetVipCustomersAsync(companyId);
            return Ok(ApiResponse<IEnumerable<CompanyCustomerResponseDto>>.SuccessResponse(customers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting VIP customers for company {CompanyId}", companyId);
            return StatusCode(500, ApiResponse<IEnumerable<CompanyCustomerResponseDto>>.ErrorResponse("Lỗi khi lấy danh sách khách VIP"));
        }
    }

    /// <summary>
    /// Get a company customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CompanyCustomerResponseDto>>> GetById(int id)
    {
        try
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
                return NotFound(ApiResponse<CompanyCustomerResponseDto>.ErrorResponse("Không tìm thấy khách hàng"));
            
            return Ok(ApiResponse<CompanyCustomerResponseDto>.SuccessResponse(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company customer {Id}", id);
            return StatusCode(500, ApiResponse<CompanyCustomerResponseDto>.ErrorResponse("Lỗi khi lấy thông tin khách hàng"));
        }
    }

    /// <summary>
    /// Create or update a company customer (upsert by phone)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CompanyCustomerResponseDto>>> CreateOrUpdate(
        [FromQuery] int companyId,
        [FromBody] CompanyCustomerRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CompanyCustomerResponseDto>.ErrorResponse("Dữ liệu không hợp lệ"));
            
            var result = await _service.CreateOrUpdateAsync(companyId, request);
            return Ok(ApiResponse<CompanyCustomerResponseDto>.SuccessResponse(result, "Lưu thông tin khách hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating company customer for company {CompanyId}", companyId);
            return StatusCode(500, ApiResponse<CompanyCustomerResponseDto>.ErrorResponse("Lỗi khi lưu thông tin khách hàng"));
        }
    }

    /// <summary>
    /// Update pricing for a company customer
    /// </summary>
    [HttpPut("{id}/pricing")]
    public async Task<ActionResult<ApiResponse<CompanyCustomerResponseDto>>> SetPricing(
        int id,
        [FromBody] CompanyCustomerPricingDto request)
    {
        try
        {
            var result = await _service.SetPricingAsync(id, request.CustomPrice, request.DiscountPercent);
            return Ok(ApiResponse<CompanyCustomerResponseDto>.SuccessResponse(result, "Cập nhật giá thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CompanyCustomerResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting pricing for company customer {Id}", id);
            return StatusCode(500, ApiResponse<CompanyCustomerResponseDto>.ErrorResponse("Lỗi khi cập nhật giá"));
        }
    }

    /// <summary>
    /// Update VIP status for a company customer
    /// </summary>
    [HttpPut("{id}/vip")]
    public async Task<ActionResult<ApiResponse<CompanyCustomerResponseDto>>> SetVipStatus(
        int id,
        [FromBody] CompanyCustomerVipDto request)
    {
        try
        {
            var result = await _service.SetVipStatusAsync(id, request.IsVip);
            return Ok(ApiResponse<CompanyCustomerResponseDto>.SuccessResponse(result, 
                request.IsVip ? "Đã đánh dấu khách VIP" : "Đã bỏ đánh dấu khách VIP"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CompanyCustomerResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting VIP status for company customer {Id}", id);
            return StatusCode(500, ApiResponse<CompanyCustomerResponseDto>.ErrorResponse("Lỗi khi cập nhật trạng thái VIP"));
        }
    }

    /// <summary>
    /// Calculate price for a customer
    /// </summary>
    [HttpPost("calculate-price")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CalculatePriceResponseDto>>> CalculatePrice(
        [FromBody] CalculatePriceRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CalculatePriceResponseDto>.ErrorResponse("Dữ liệu không hợp lệ"));
            
            var finalPrice = await _service.CalculatePriceAsync(request.CompanyId, request.Phone, request.BasePrice);
            
            var response = new CalculatePriceResponseDto
            {
                BasePrice = request.BasePrice,
                FinalPrice = finalPrice,
                PricingType = finalPrice < request.BasePrice ? "Giá ưu đãi" : "Giá mặc định",
                IsCustomerFound = finalPrice != request.BasePrice
            };
            
            return Ok(ApiResponse<CalculatePriceResponseDto>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for company {CompanyId}, phone {Phone}", 
                request.CompanyId, request.Phone);
            return StatusCode(500, ApiResponse<CalculatePriceResponseDto>.ErrorResponse("Lỗi khi tính giá"));
        }
    }

    /// <summary>
    /// Delete a company customer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Không tìm thấy khách hàng"));
            
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Xóa khách hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company customer {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa khách hàng"));
        }
    }
}
