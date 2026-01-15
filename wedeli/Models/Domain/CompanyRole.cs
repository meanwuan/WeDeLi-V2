using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("company_roles")]
[Index("CompanyId", "RoleName", Name = "idx_company_role_unique", IsUnique = true)]
public partial class CompanyRole
{
    [Key]
    [Column("company_role_id")]
    public int CompanyRoleId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("role_name")]
    [StringLength(50)]
    public string RoleName { get; set; } = null!;  // CompanyAdmin, Driver, WarehouseStaff

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    // [NotMapped] prevents EF from creating FK constraint while keeping navigation for code compatibility
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    [InverseProperty("CompanyRole")]
    public virtual ICollection<CompanyUser> CompanyUsers { get; set; } = new List<CompanyUser>();
}
