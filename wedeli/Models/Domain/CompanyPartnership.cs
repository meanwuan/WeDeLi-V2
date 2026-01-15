using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("company_partnerships")]
[Index("CreatedBy", Name = "created_by")]
[Index("PartnerCompanyId", Name = "partner_company_id")]
[Index("CompanyId", "PartnerCompanyId", Name = "unique_partnership", IsUnique = true)]
public partial class CompanyPartnership
{
    [Key]
    [Column("partnership_id")]
    public int PartnershipId { get; set; }

    /// <summary>
    /// Nhà xe chính
    /// </summary>
    [Column("company_id")]
    public int CompanyId { get; set; }

    /// <summary>
    /// Nhà xe đối tác quen
    /// </summary>
    [Column("partner_company_id")]
    public int PartnerCompanyId { get; set; }

    [Column("partnership_level", TypeName = "enum('preferred','regular','backup')")]
    public string? PartnershipLevel { get; set; }

    /// <summary>
    /// % hoa hồng khi chuyển hàng
    /// </summary>
    [Column("commission_rate")]
    [Precision(5, 2)]
    public decimal? CommissionRate { get; set; }

    /// <summary>
    /// Thứ tự ưu tiên (số càng nhỏ càng ưu tiên)
    /// </summary>
    [Column("priority_order")]
    public int? PriorityOrder { get; set; }

    [Column("total_transferred_orders")]
    public int? TotalTransferredOrders { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    /// <summary>
    /// Admin tạo quan hệ
    /// </summary>
    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    // Cross-DB reference: CreatedBy references User in Platform DB
    [NotMapped]
    public virtual User? CreatedByNavigation { get; set; }

    // Cross-DB reference: PartnerCompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? PartnerCompany { get; set; }
}
