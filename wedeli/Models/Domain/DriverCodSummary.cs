using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("driver_cod_summary")]
[Index("ReconciledBy", Name = "reconciled_by")]
[Index("DriverId", "SummaryDate", Name = "unique_driver_date", IsUnique = true)]
public partial class DriverCodSummary
{
    [Key]
    [Column("summary_id")]
    public int SummaryId { get; set; }

    [Column("driver_id")]
    public int DriverId { get; set; }

    [Column("summary_date")]
    public DateOnly SummaryDate { get; set; }

    [Column("total_cod_collected")]
    [Precision(15, 2)]
    public decimal? TotalCodCollected { get; set; }

    [Column("total_cod_submitted")]
    [Precision(15, 2)]
    public decimal? TotalCodSubmitted { get; set; }

    /// <summary>
    /// Số tiền chưa nộp
    /// </summary>
    [Column("pending_amount")]
    [Precision(15, 2)]
    public decimal? PendingAmount { get; set; }

    [Column("reconciliation_status", TypeName = "enum('pending','reconciled','discrepancy')")]
    public string? ReconciliationStatus { get; set; }

    /// <summary>
    /// Admin xác nhận đối soát
    /// </summary>
    [Column("reconciled_by")]
    public int? ReconciledBy { get; set; }

    [Column("reconciled_at", TypeName = "timestamp")]
    public DateTime? ReconciledAt { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("DriverCodSummaries")]
    public virtual Driver Driver { get; set; } = null!;

    [ForeignKey("ReconciledBy")]
    [InverseProperty("DriverCodSummaries")]
    public virtual User? ReconciledByNavigation { get; set; }
}
