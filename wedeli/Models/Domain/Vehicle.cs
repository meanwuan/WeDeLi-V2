using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("vehicles")]
[Index("CompanyId", Name = "company_id")]
[Index("LicensePlate", Name = "license_plate", IsUnique = true)]
public partial class Vehicle
{
    [Key]
    [Column("vehicle_id")]
    public int VehicleId { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("license_plate")]
    [StringLength(20)]
    public string LicensePlate { get; set; } = null!;

    [Column("vehicle_type", TypeName = "enum('truck','van','motorbike')")]
    public string VehicleType { get; set; } = null!;

    [Column("max_weight_kg")]
    [Precision(8, 2)]
    public decimal? MaxWeightKg { get; set; }

    [Column("max_volume_m3")]
    [Precision(6, 2)]
    public decimal? MaxVolumeM3 { get; set; }

    /// <summary>
    /// Trọng tải hiện tại
    /// </summary>
    [Column("current_weight_kg")]
    [Precision(8, 2)]
    public decimal? CurrentWeightKg { get; set; }

    /// <summary>
    /// % tải trọng
    /// </summary>
    [Column("capacity_percentage")]
    [Precision(5, 2)]
    public decimal? CapacityPercentage { get; set; }

    /// <summary>
    /// Ngưỡng cảnh báo đầy (%)
    /// </summary>
    [Column("overload_threshold")]
    [Precision(5, 2)]
    public decimal? OverloadThreshold { get; set; }

    /// <summary>
    /// Admin cho phép thêm hàng nhẹ
    /// </summary>
    [Column("allow_overload")]
    public bool? AllowOverload { get; set; }

    [Column("current_status", TypeName = "enum('available','in_transit','maintenance','inactive','overloaded')")]
    public string? CurrentStatus { get; set; }

    [Column("gps_enabled")]
    public bool? GpsEnabled { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    [InverseProperty("NewVehicle")]
    public virtual ICollection<OrderTransfer> OrderTransferNewVehicles { get; set; } = new List<OrderTransfer>();

    [InverseProperty("OriginalVehicle")]
    public virtual ICollection<OrderTransfer> OrderTransferOriginalVehicles { get; set; } = new List<OrderTransfer>();

    [InverseProperty("Vehicle")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("Vehicle")]
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    [InverseProperty("Vehicle")]
    public virtual ICollection<VehicleLocation> VehicleLocations { get; set; } = new List<VehicleLocation>();
}
