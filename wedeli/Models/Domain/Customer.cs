using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("customers")]
[Index("Phone", Name = "idx_customers_phone")]
[Index("UserId", Name = "user_id")]
public partial class Customer
{
    [Key]
    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("full_name")]
    [StringLength(200)]
    public string FullName { get; set; } = null!;

    [Column("phone")]
    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Khách hàng quen thuộc
    /// </summary>
    [Column("is_regular")]
    public bool? IsRegular { get; set; }

    [Column("total_orders")]
    public int? TotalOrders { get; set; }

    [Column("total_revenue")]
    [Precision(15, 2)]
    public decimal? TotalRevenue { get; set; }

    [Column("payment_privilege", TypeName = "enum('prepay','postpay','periodic')")]
    public string? PaymentPrivilege { get; set; }

    /// <summary>
    /// Hạn mức công nợ cho khách quen
    /// </summary>
    [Column("credit_limit")]
    [Precision(15, 2)]
    public decimal? CreditLimit { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    [InverseProperty("Customer")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("Customer")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Customer")]
    public virtual ICollection<PeriodicInvoice> PeriodicInvoices { get; set; } = new List<PeriodicInvoice>();

    [InverseProperty("Customer")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [ForeignKey("UserId")]
    [InverseProperty("Customers")]
    public virtual User? User { get; set; }
}
