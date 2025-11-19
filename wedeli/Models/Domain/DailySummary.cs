using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("daily_summary")]
[Index("SummaryDate", Name = "idx_summary_date", IsUnique = true)]
public partial class DailySummary
{
    [Key]
    [Column("summary_id")]
    public int SummaryId { get; set; }

    [Column("summary_date")]
    public DateOnly SummaryDate { get; set; }

    [Column("total_orders_created")]
    public int? TotalOrdersCreated { get; set; }

    [Column("total_orders_delivered")]
    public int? TotalOrdersDelivered { get; set; }

    [Column("total_orders_cancelled")]
    public int? TotalOrdersCancelled { get; set; }

    [Column("total_orders_transferred")]
    public int? TotalOrdersTransferred { get; set; }

    [Column("total_vehicles_active")]
    public int? TotalVehiclesActive { get; set; }

    [Column("total_vehicles_overloaded")]
    public int? TotalVehiclesOverloaded { get; set; }

    [Column("total_trips_completed")]
    public int? TotalTripsCompleted { get; set; }

    [Column("total_drivers_active")]
    public int? TotalDriversActive { get; set; }

    [Column("avg_deliveries_per_driver")]
    [Precision(8, 2)]
    public decimal? AvgDeliveriesPerDriver { get; set; }

    [Column("total_revenue")]
    [Precision(15, 2)]
    public decimal? TotalRevenue { get; set; }

    [Column("total_cod_collected")]
    [Precision(15, 2)]
    public decimal? TotalCodCollected { get; set; }

    [Column("total_cod_submitted")]
    [Precision(15, 2)]
    public decimal? TotalCodSubmitted { get; set; }

    [Column("pending_cod_amount")]
    [Precision(15, 2)]
    public decimal? PendingCodAmount { get; set; }

    [Column("total_complaints")]
    public int? TotalComplaints { get; set; }

    [Column("complaints_resolved")]
    public int? ComplaintsResolved { get; set; }

    [Column("last_updated_at", TypeName = "timestamp")]
    public DateTime? LastUpdatedAt { get; set; }

    [Column("generated_by")]
    [StringLength(50)]
    public string? GeneratedBy { get; set; }
}
