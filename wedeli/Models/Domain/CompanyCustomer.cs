using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

/// <summary>
/// Lưu thông tin khách hàng cục bộ của company + định giá riêng.
/// CustomerId = null nghĩa là guest customer.
/// </summary>
[Table("company_customers")]
[Index("CompanyId", "CustomerId", Name = "idx_company_customer_unique", IsUnique = true)]
[Index("CompanyId", "Phone", Name = "idx_company_customer_phone")]
public partial class CompanyCustomer
{
    [Key]
    [Column("company_customer_id")]
    public int CompanyCustomerId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    /// <summary>
    /// Null = guest customer (đặt hàng không đăng ký)
    /// </summary>
    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Column("full_name")]
    [StringLength(200)]
    public string FullName { get; set; } = null!;

    [Column("phone")]
    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Giá riêng cho khách hàng này (null = dùng giá mặc định)
    /// </summary>
    [Column("custom_price")]
    [Precision(15, 2)]
    public decimal? CustomPrice { get; set; }

    /// <summary>
    /// % giảm giá cho khách hàng này
    /// </summary>
    [Column("discount_percent")]
    [Precision(5, 2)]
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Ghi chú nội bộ về khách hàng
    /// </summary>
    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    /// <summary>
    /// Khách hàng VIP / quen thuộc của company
    /// </summary>
    [Column("is_vip")]
    public bool IsVip { get; set; } = false;

    [Column("total_orders")]
    public int TotalOrders { get; set; } = 0;

    [Column("total_revenue")]
    [Precision(15, 2)]
    public decimal TotalRevenue { get; set; } = 0;

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    // Cross-DB reference: CustomerId references Customer in Platform DB
    [NotMapped]
    public virtual Customer? Customer { get; set; }
}
