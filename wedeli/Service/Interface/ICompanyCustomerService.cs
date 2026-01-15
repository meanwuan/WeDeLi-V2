using wedeli.Models.Domain;
using wedeli.Models.DTO;

namespace wedeli.Service.Interface;

/// <summary>
/// Service interface for CompanyCustomer - business logic for customer-specific pricing
/// </summary>
public interface ICompanyCustomerService
{
    /// <summary>
    /// Get all customers for a company
    /// </summary>
    Task<IEnumerable<CompanyCustomerResponseDto>> GetCompanyCustomersAsync(int companyId);
    
    /// <summary>
    /// Get VIP customers for a company
    /// </summary>
    Task<IEnumerable<CompanyCustomerResponseDto>> GetVipCustomersAsync(int companyId);
    
    /// <summary>
    /// Get company customer by ID
    /// </summary>
    Task<CompanyCustomerResponseDto?> GetByIdAsync(int companyCustomerId);
    
    /// <summary>
    /// Create or update a company customer (upsert by phone)
    /// </summary>
    Task<CompanyCustomerResponseDto> CreateOrUpdateAsync(int companyId, CompanyCustomerRequestDto request);
    
    /// <summary>
    /// Set custom pricing for a customer
    /// </summary>
    Task<CompanyCustomerResponseDto> SetPricingAsync(int companyCustomerId, decimal? customPrice, decimal? discountPercent);
    
    /// <summary>
    /// Set VIP status for a customer
    /// </summary>
    Task<CompanyCustomerResponseDto> SetVipStatusAsync(int companyCustomerId, bool isVip);
    
    /// <summary>
    /// Calculate final price for a customer based on their custom pricing settings
    /// </summary>
    Task<decimal> CalculatePriceAsync(int companyId, string phone, decimal basePrice);
    
    /// <summary>
    /// Calculate final price using company customer data directly
    /// </summary>
    decimal CalculatePrice(decimal basePrice, CompanyCustomer? companyCustomer);
    
    /// <summary>
    /// Delete a company customer
    /// </summary>
    Task<bool> DeleteAsync(int companyCustomerId);
    
    /// <summary>
    /// Update order statistics for a customer after order completion
    /// </summary>
    Task UpdateOrderStatsAsync(int companyId, string phone, decimal orderAmount);
}
