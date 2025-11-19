using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("routes")]
[Index("CompanyId", Name = "company_id")]
public partial class Route
{
    [Key]
    [Column("route_id")]
    public int RouteId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("route_name")]
    [StringLength(200)]
    public string RouteName { get; set; } = null!;

    [Column("origin_province")]
    [StringLength(100)]
    public string? OriginProvince { get; set; }

    [Column("origin_district")]
    [StringLength(100)]
    public string? OriginDistrict { get; set; }

    [Column("destination_province")]
    [StringLength(100)]
    public string? DestinationProvince { get; set; }

    [Column("destination_district")]
    [StringLength(100)]
    public string? DestinationDistrict { get; set; }

    [Column("distance_km")]
    [Precision(8, 2)]
    public decimal? DistanceKm { get; set; }

    [Column("estimated_duration_hours")]
    [Precision(5, 2)]
    public decimal? EstimatedDurationHours { get; set; }

    [Column("base_price")]
    [Precision(12, 2)]
    public decimal? BasePrice { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Routes")]
    public virtual TransportCompany Company { get; set; } = null!;

    [InverseProperty("Route")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("Route")]
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
