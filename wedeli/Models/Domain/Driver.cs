using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("drivers")]
[Index("CompanyId", Name = "company_id")]
[Index("CompanyUserId", Name = "company_user_id")]
public partial class Driver
{
    [Key]
    [Column("driver_id")]
    public int DriverId { get; set; }

    /// <summary>
    /// Link to CompanyUser instead of platform User
    /// </summary>
    [Column("company_user_id")]
    public int CompanyUserId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("driver_license")]
    [StringLength(50)]
    public string? DriverLicense { get; set; }

    [Column("license_expiry")]
    public DateOnly? LicenseExpiry { get; set; }

    [Column("total_trips")]
    public int? TotalTrips { get; set; }

    [Column("success_rate")]
    [Precision(5, 2)]
    public decimal? SuccessRate { get; set; }

    [Column("rating")]
    [Precision(3, 2)]
    public decimal? Rating { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("CollectedByDriverNavigation")]
    public virtual ICollection<CodTransaction> CodTransactions { get; set; } = new List<CodTransaction>();

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    [InverseProperty("Driver")]
    public virtual ICollection<DriverCodSummary> DriverCodSummaries { get; set; } = new List<DriverCodSummary>();

    [InverseProperty("Driver")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("Driver")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [InverseProperty("Driver")]
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    [ForeignKey("CompanyUserId")]
    [InverseProperty("Drivers")]
    public virtual CompanyUser CompanyUser { get; set; } = null!;
}

