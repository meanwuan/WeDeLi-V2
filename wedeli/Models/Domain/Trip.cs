using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("trips")]
[Index("DriverId", Name = "driver_id")]
[Index("TripDate", "TripStatus", Name = "idx_trips_date")]
[Index("RouteId", Name = "route_id")]
[Index("VehicleId", Name = "vehicle_id")]
public partial class Trip
{
    [Key]
    [Column("trip_id")]
    public int TripId { get; set; }

    [Column("route_id")]
    public int RouteId { get; set; }

    [Column("vehicle_id")]
    public int VehicleId { get; set; }

    [Column("driver_id")]
    public int DriverId { get; set; }

    [Column("trip_date")]
    public DateOnly TripDate { get; set; }

    [Column("departure_time", TypeName = "timestamp")]
    public DateTime? DepartureTime { get; set; }

    [Column("arrival_time", TypeName = "timestamp")]
    public DateTime? ArrivalTime { get; set; }

    [Column("trip_status", TypeName = "enum('scheduled','in_progress','completed','cancelled')")]
    public string? TripStatus { get; set; }

    [Column("total_orders")]
    public int? TotalOrders { get; set; }

    [Column("total_weight_kg")]
    [Precision(8, 2)]
    public decimal? TotalWeightKg { get; set; }

    /// <summary>
    /// Chuyến về (E2)
    /// </summary>
    [Column("is_return_trip")]
    public bool? IsReturnTrip { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("Trips")]
    public virtual Driver Driver { get; set; } = null!;

    [ForeignKey("RouteId")]
    [InverseProperty("Trips")]
    public virtual Route Route { get; set; } = null!;

    [InverseProperty("Trip")]
    public virtual ICollection<TripOrder> TripOrders { get; set; } = new List<TripOrder>();

    [ForeignKey("VehicleId")]
    [InverseProperty("Trips")]
    public virtual Vehicle Vehicle { get; set; } = null!;
}
