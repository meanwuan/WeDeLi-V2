using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("complaints")]
[Index("CustomerId", Name = "customer_id")]
[Index("OrderId", Name = "order_id")]
[Index("ResolvedBy", Name = "resolved_by")]
public partial class Complaint
{
    [Key]
    [Column("complaint_id")]
    public int ComplaintId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("complaint_type", TypeName = "enum('lost','damaged','late','wrong_address','other')")]
    public string ComplaintType { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// JSON array of photo URLs
    /// </summary>
    [Column("evidence_photos", TypeName = "text")]
    public string? EvidencePhotos { get; set; }

    [Column("complaint_status", TypeName = "enum('pending','investigating','resolved','rejected')")]
    public string? ComplaintStatus { get; set; }

    [Column("resolution_notes", TypeName = "text")]
    public string? ResolutionNotes { get; set; }

    [Column("resolved_by")]
    public int? ResolvedBy { get; set; }

    [Column("resolved_at", TypeName = "timestamp")]
    public DateTime? ResolvedAt { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [NotMapped]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("OrderId")]
    [InverseProperty("Complaints")]
    public virtual Order Order { get; set; } = null!;

    [NotMapped]
    public virtual User? ResolvedByNavigation { get; set; }
}
