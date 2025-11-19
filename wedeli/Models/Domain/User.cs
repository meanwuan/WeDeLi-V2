using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("users")]
[Index("RoleId", Name = "role_id")]
[Index("Username", Name = "username", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

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

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("CompanyReceivedByNavigation")]
    public virtual ICollection<CodTransaction> CodTransactions { get; set; } = new List<CodTransaction>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<CompanyPartnership> CompanyPartnerships { get; set; } = new List<CompanyPartnership>();

    [InverseProperty("ResolvedByNavigation")]
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    [InverseProperty("User")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<DailyActivityLog> DailyActivityLogs { get; set; } = new List<DailyActivityLog>();

    [InverseProperty("ReconciledByNavigation")]
    public virtual ICollection<DriverCodSummary> DriverCodSummaries { get; set; } = new List<DriverCodSummary>();

    [InverseProperty("User")]
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<OrderPhoto> OrderPhotos { get; set; } = new List<OrderPhoto>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    [InverseProperty("TransferredByNavigation")]
    public virtual ICollection<OrderTransfer> OrderTransfers { get; set; } = new List<OrderTransfer>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<WarehouseStaff> WarehouseStaffs { get; set; } = new List<WarehouseStaff>();
}
