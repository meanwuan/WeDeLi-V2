using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("company_users")]
[Index("CompanyId", Name = "idx_company_users_company")]
[Index("Username", Name = "idx_company_users_username", IsUnique = true)]
[Index("CompanyRoleId", Name = "idx_company_users_role")]
public partial class CompanyUser
{
    [Key]
    [Column("company_user_id")]
    public int CompanyUserId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("company_role_id")]
    public int CompanyRoleId { get; set; }

    [Column("username")]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("full_name")]
    [StringLength(200)]
    public string FullName { get; set; } = null!;

    [Column("phone")]
    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    [ForeignKey("CompanyRoleId")]
    [InverseProperty("CompanyUsers")]
    public virtual CompanyRole CompanyRole { get; set; } = null!;

    [InverseProperty("CompanyUser")]
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    [InverseProperty("CompanyUser")]
    public virtual ICollection<WarehouseStaff> WarehouseStaffs { get; set; } = new List<WarehouseStaff>();
}
