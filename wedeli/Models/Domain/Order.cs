using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("orders")]
[Index("DriverId", Name = "driver_id")]
[Index("CreatedAt", Name = "idx_orders_created_at")]
[Index("CustomerId", Name = "idx_orders_customer")]
[Index("OrderStatus", Name = "idx_orders_status")]
[Index("TrackingCode", Name = "idx_orders_tracking", IsUnique = true)]
[Index("RouteId", Name = "route_id")]
[Index("VehicleId", Name = "vehicle_id")]
public partial class Order
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("tracking_code")]
    [StringLength(50)]
    public string TrackingCode { get; set; } = null!;

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("sender_name")]
    [StringLength(200)]
    public string SenderName { get; set; } = null!;

    [Column("sender_phone")]
    [StringLength(20)]
    public string SenderPhone { get; set; } = null!;

    [Column("sender_address", TypeName = "text")]
    public string SenderAddress { get; set; } = null!;

    [Column("receiver_name")]
    [StringLength(200)]
    public string ReceiverName { get; set; } = null!;

    [Column("receiver_phone")]
    [StringLength(20)]
    public string ReceiverPhone { get; set; } = null!;

    [Column("receiver_address", TypeName = "text")]
    public string ReceiverAddress { get; set; } = null!;

    [Column("receiver_province")]
    [StringLength(100)]
    public string? ReceiverProvince { get; set; }

    [Column("receiver_district")]
    [StringLength(100)]
    public string? ReceiverDistrict { get; set; }

    [Column("parcel_type", TypeName = "enum('fragile','electronics','food','cold','document','other')")]
    public string? ParcelType { get; set; }

    [Column("weight_kg")]
    [Precision(8, 2)]
    public decimal? WeightKg { get; set; }

    [Column("declared_value")]
    [Precision(15, 2)]
    public decimal? DeclaredValue { get; set; }

    [Column("special_instructions", TypeName = "text")]
    public string? SpecialInstructions { get; set; }

    [Column("route_id")]
    public int? RouteId { get; set; }

    [Column("vehicle_id")]
    public int? VehicleId { get; set; }

    [Column("driver_id")]
    public int? DriverId { get; set; }

    [Column("shipping_fee")]
    [Precision(12, 2)]
    public decimal ShippingFee { get; set; }

    /// <summary>
    /// Tiền thu hộ
    /// </summary>
    [Column("cod_amount")]
    [Precision(15, 2)]
    public decimal? CodAmount { get; set; }

    [Column("payment_method", TypeName = "enum('cash','bank_transfer','e_wallet','periodic')")]
    public string PaymentMethod { get; set; } = null!;

    [Column("payment_status", TypeName = "enum('unpaid','paid','pending')")]
    public string? PaymentStatus { get; set; }

    [Column("paid_at", TypeName = "timestamp")]
    public DateTime? PaidAt { get; set; }

    [Column("order_status", TypeName = "enum('pending_pickup','picked_up','in_transit','out_for_delivery','delivered','returned','cancelled')")]
    public string? OrderStatus { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column("pickup_scheduled_at", TypeName = "timestamp")]
    public DateTime? PickupScheduledAt { get; set; }

    [Column("pickup_confirmed_at", TypeName = "timestamp")]
    public DateTime? PickupConfirmedAt { get; set; }

    [Column("delivered_at", TypeName = "timestamp")]
    public DateTime? DeliveredAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<CodTransaction> CodTransactions { get; set; } = new List<CodTransaction>();

    [InverseProperty("Order")]
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    [NotMapped]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("DriverId")]
    [InverseProperty("Orders")]
    public virtual Driver? Driver { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderPhoto> OrderPhotos { get; set; } = new List<OrderPhoto>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderTransfer> OrderTransfers { get; set; } = new List<OrderTransfer>();

    [InverseProperty("Order")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Order")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [ForeignKey("RouteId")]
    [InverseProperty("Orders")]
    public virtual Route? Route { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<TripOrder> TripOrders { get; set; } = new List<TripOrder>();

    [ForeignKey("VehicleId")]
    [InverseProperty("Orders")]
    public virtual Vehicle? Vehicle { get; set; }
}
