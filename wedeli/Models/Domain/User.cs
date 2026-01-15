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

    [NotMapped]
    public virtual ICollection<CodTransaction> CodTransactions { get; set; } = new List<CodTransaction>();

    [NotMapped]
    public virtual ICollection<CompanyPartnership> CompanyPartnerships { get; set; } = new List<CompanyPartnership>();

    [NotMapped]
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    [InverseProperty("User")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    [NotMapped]
    public virtual ICollection<DailyActivityLog> DailyActivityLogs { get; set; } = new List<DailyActivityLog>();

    [NotMapped]
    public virtual ICollection<DriverCodSummary> DriverCodSummaries { get; set; } = new List<DriverCodSummary>();


    [NotMapped]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [NotMapped]
    public virtual ICollection<OrderPhoto> OrderPhotos { get; set; } = new List<OrderPhoto>();

    [NotMapped]
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    [NotMapped]
    public virtual ICollection<OrderTransfer> OrderTransfers { get; set; } = new List<OrderTransfer>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("User")]
    public virtual TransportCompany? TransportCompany { get; set; }
}
