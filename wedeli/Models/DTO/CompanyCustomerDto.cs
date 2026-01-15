using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO;

/// <summary>
/// Request DTO for creating/updating a company customer
/// </summary>
public class CompanyCustomerRequestDto
{
    /// <summary>
    /// Optional: Link to existing platform customer
    /// </summary>
    public int? CustomerId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = null!;
    
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = null!;
    
    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    /// Fixed custom price (overrides base price)
    /// </summary>
    public decimal? CustomPrice { get; set; }
    
    /// <summary>
    /// Discount percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public decimal? DiscountPercent { get; set; }
    
    /// <summary>
    /// Mark as VIP customer
    /// </summary>
    public bool IsVip { get; set; } = false;
    
    /// <summary>
    /// Internal notes about the customer
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Response DTO for company customer data
/// </summary>
public class CompanyCustomerResponseDto
{
    public int CompanyCustomerId { get; set; }
    public int CompanyId { get; set; }
    public int? CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public decimal? CustomPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool IsVip { get; set; }
    public string? Notes { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Description of the pricing rule applied
    /// </summary>
    public string PricingDescription => GetPricingDescription();
    
    private string GetPricingDescription()
    {
        if (CustomPrice.HasValue && CustomPrice.Value > 0)
            return $"Giá cố định: {CustomPrice.Value:N0}đ";
        if (DiscountPercent.HasValue && DiscountPercent.Value > 0)
            return $"Giảm {DiscountPercent.Value}%";
        return "Giá mặc định";
    }
}

/// <summary>
/// DTO for updating pricing only
/// </summary>
public class CompanyCustomerPricingDto
{
    /// <summary>
    /// Fixed custom price (null to remove)
    /// </summary>
    public decimal? CustomPrice { get; set; }
    
    /// <summary>
    /// Discount percentage (0-100, null to remove)
    /// </summary>
    [Range(0, 100)]
    public decimal? DiscountPercent { get; set; }
}

/// <summary>
/// DTO for updating VIP status
/// </summary>
public class CompanyCustomerVipDto
{
    public bool IsVip { get; set; }
}

/// <summary>
/// DTO for calculating price
/// </summary>
public class CalculatePriceRequestDto
{
    [Required]
    public int CompanyId { get; set; }
    
    [Required]
    public string Phone { get; set; } = null!;
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }
}

/// <summary>
/// DTO for price calculation result
/// </summary>
public class CalculatePriceResponseDto
{
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal DiscountAmount => BasePrice - FinalPrice;
    public string PricingType { get; set; } = null!;
    public bool IsCustomerFound { get; set; }
}
