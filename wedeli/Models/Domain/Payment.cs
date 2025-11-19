using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("payments")]
[Index("CustomerId", Name = "customer_id")]
[Index("PaymentStatus", Name = "idx_payments_status")]
[Index("OrderId", Name = "order_id")]
public partial class Payment
{
    [Key]
    [Column("payment_id")]
    public int PaymentId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    [Column("payment_method", TypeName = "enum('cash','bank_transfer','e_wallet')")]
    public string PaymentMethod { get; set; } = null!;

    [Column("payment_type", TypeName = "enum('shipping_fee','cod_collection','refund')")]
    public string PaymentType { get; set; } = null!;

    [Column("payment_status", TypeName = "enum('pending','completed','failed')")]
    public string? PaymentStatus { get; set; }

    [Column("transaction_ref")]
    [StringLength(100)]
    public string? TransactionRef { get; set; }

    [Column("paid_at", TypeName = "timestamp")]
    public DateTime? PaidAt { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Payments")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("OrderId")]
    [InverseProperty("Payments")]
    public virtual Order Order { get; set; } = null!;
}
