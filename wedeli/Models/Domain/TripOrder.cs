using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("trip_orders")]
[Index("OrderId", Name = "order_id")]
[Index("TripId", Name = "trip_id")]
public partial class TripOrder
{
    [Key]
    [Column("trip_order_id")]
    public int TripOrderId { get; set; }

    [Column("trip_id")]
    public int TripId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    /// <summary>
    /// Thứ tự giao hàng
    /// </summary>
    [Column("sequence_number")]
    public int? SequenceNumber { get; set; }

    [Column("pickup_confirmed")]
    public bool? PickupConfirmed { get; set; }

    [Column("delivery_confirmed")]
    public bool? DeliveryConfirmed { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("TripOrders")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("TripId")]
    [InverseProperty("TripOrders")]
    public virtual Trip Trip { get; set; } = null!;
}
