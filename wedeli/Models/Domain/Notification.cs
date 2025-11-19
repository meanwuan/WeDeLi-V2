using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("notifications")]
[Index("OrderId", Name = "order_id")]
[Index("UserId", Name = "user_id")]
public partial class Notification
{
    [Key]
    [Column("notification_id")]
    public int NotificationId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("notification_type", TypeName = "enum('order_status','payment','promotion','system')")]
    public string NotificationType { get; set; } = null!;

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("message", TypeName = "text")]
    public string Message { get; set; } = null!;

    [Column("is_read")]
    public bool? IsRead { get; set; }

    [Column("sent_via", TypeName = "enum('sms','email','push','all')")]
    public string? SentVia { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Notifications")]
    public virtual Order? Order { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User? User { get; set; }
}
