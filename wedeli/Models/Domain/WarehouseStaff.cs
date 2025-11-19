using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("warehouse_staff")]
[Index("CompanyId", Name = "company_id")]
[Index("UserId", Name = "user_id")]
public partial class WarehouseStaff
{
    [Key]
    [Column("staff_id")]
    public int StaffId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("warehouse_location")]
    [StringLength(200)]
    public string? WarehouseLocation { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("WarehouseStaffs")]
    public virtual TransportCompany Company { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("WarehouseStaffs")]
    public virtual User User { get; set; } = null!;
}
