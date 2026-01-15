using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("daily_activity_logs")]
[Index("ChangedBy", Name = "changed_by")]
[Index("EntityType", "EntityId", Name = "idx_entity")]
[Index("LogDate", Name = "idx_log_date")]
[Index("LogType", Name = "idx_log_type")]
public partial class DailyActivityLog
{
    [Key]
    [Column("log_id")]
    public long LogId { get; set; }

    [Column("log_date")]
    public DateOnly LogDate { get; set; }

    [Column("log_type", TypeName = "enum('order','payment','transfer','vehicle','complaint','system')")]
    public string LogType { get; set; } = null!;

    /// <summary>
    /// orders, vehicles, drivers, etc
    /// </summary>
    [Column("entity_type")]
    [StringLength(50)]
    public string? EntityType { get; set; }

    /// <summary>
    /// ID của entity
    /// </summary>
    [Column("entity_id")]
    public int? EntityId { get; set; }

    /// <summary>
    /// created, updated, deleted, transferred, etc
    /// </summary>
    [Column("action")]
    [StringLength(100)]
    public string Action { get; set; } = null!;

    /// <summary>
    /// Giá trị cũ (JSON)
    /// </summary>
    [Column("old_value", TypeName = "text")]
    public string? OldValue { get; set; }

    /// <summary>
    /// Giá trị mới (JSON)
    /// </summary>
    [Column("new_value", TypeName = "text")]
    public string? NewValue { get; set; }

    /// <summary>
    /// User thực hiện thay đổi
    /// </summary>
    [Column("changed_by")]
    public int? ChangedBy { get; set; }

    [Column("ip_address")]
    [StringLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent", TypeName = "text")]
    public string? UserAgent { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [NotMapped]
    public virtual User? ChangedByNavigation { get; set; }
}
