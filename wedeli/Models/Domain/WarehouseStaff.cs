using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("warehouse_staff")]
[Index("CompanyId", Name = "company_id")]
[Index("CompanyUserId", Name = "company_user_id")]
public partial class WarehouseStaff
{
    [Key]
    [Column("staff_id")]
    public int StaffId { get; set; }

    /// <summary>
    /// Link to CompanyUser instead of platform User
    /// </summary>
    [Column("company_user_id")]
    public int CompanyUserId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("warehouse_location")]
    [StringLength(200)]
    public string? WarehouseLocation { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    [ForeignKey("CompanyUserId")]
    [InverseProperty("WarehouseStaffs")]
    public virtual CompanyUser CompanyUser { get; set; } = null!;
}

