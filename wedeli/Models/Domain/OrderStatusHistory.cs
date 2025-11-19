using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("order_status_history")]
[Index("OrderId", Name = "order_id")]
[Index("UpdatedBy", Name = "updated_by")]
public partial class OrderStatusHistory
{
    [Key]
    [Column("history_id")]
    public int HistoryId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("old_status")]
    [StringLength(50)]
    public string? OldStatus { get; set; }

    [Column("new_status")]
    [StringLength(50)]
    public string NewStatus { get; set; } = null!;

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    [Column("location")]
    [StringLength(200)]
    public string? Location { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderStatusHistories")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("OrderStatusHistories")]
    public virtual User? UpdatedByNavigation { get; set; }
}
