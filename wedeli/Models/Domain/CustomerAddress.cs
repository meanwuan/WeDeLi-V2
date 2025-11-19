using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("customer_addresses")]
[Index("CustomerId", Name = "customer_id")]
public partial class CustomerAddress
{
    [Key]
    [Column("address_id")]
    public int AddressId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    /// <summary>
    /// Nhà riêng, Văn phòng, etc
    /// </summary>
    [Column("address_label")]
    [StringLength(100)]
    public string? AddressLabel { get; set; }

    [Column("full_address", TypeName = "text")]
    public string FullAddress { get; set; } = null!;

    [Column("province")]
    [StringLength(100)]
    public string? Province { get; set; }

    [Column("district")]
    [StringLength(100)]
    public string? District { get; set; }

    [Column("ward")]
    [StringLength(100)]
    public string? Ward { get; set; }

    [Column("latitude")]
    [Precision(10, 8)]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    [Precision(11, 8)]
    public decimal? Longitude { get; set; }

    [Column("is_default")]
    public bool? IsDefault { get; set; }

    /// <summary>
    /// Số lần sử dụng địa chỉ này
    /// </summary>
    [Column("usage_count")]
    public int? UsageCount { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerAddresses")]
    public virtual Customer Customer { get; set; } = null!;
}
