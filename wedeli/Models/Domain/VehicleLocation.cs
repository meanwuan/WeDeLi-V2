using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

/// <summary>
/// Lưu trữ vị trí GPS realtime của xe
/// </summary>
[Table("vehicle_locations")]
[Index("VehicleId", Name = "idx_vehicle_locations_vehicle_id")]
[Index("RecordedAt", Name = "idx_vehicle_locations_recorded_at")]
public class VehicleLocation
{
    [Key]
    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("vehicle_id")]
    public int VehicleId { get; set; }

    /// <summary>
    /// Vĩ độ GPS
    /// </summary>
    [Column("latitude")]
    [Precision(10, 8)]
    public decimal Latitude { get; set; }

    /// <summary>
    /// Kinh độ GPS
    /// </summary>
    [Column("longitude")]
    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    /// <summary>
    /// Tốc độ (km/h)
    /// </summary>
    [Column("speed")]
    [Precision(5, 2)]
    public decimal? Speed { get; set; }

    /// <summary>
    /// Hướng di chuyển (0-360 độ)
    /// </summary>
    [Column("heading")]
    [Precision(5, 2)]
    public decimal? Heading { get; set; }

    /// <summary>
    /// Độ chính xác GPS (meters)
    /// </summary>
    [Column("accuracy")]
    [Precision(6, 2)]
    public decimal? Accuracy { get; set; }

    /// <summary>
    /// Trạng thái: Moving, Stopped, Idle
    /// </summary>
    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "Moving";

    /// <summary>
    /// Thời điểm ghi nhận vị trí
    /// </summary>
    [Column("recorded_at", TypeName = "timestamp")]
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Thời điểm tạo record
    /// </summary>
    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    // Navigation property
    [ForeignKey("VehicleId")]
    [InverseProperty("VehicleLocations")]
    public virtual Vehicle Vehicle { get; set; } = null!;
}
