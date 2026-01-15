using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("periodic_invoices")]
[Index("CompanyId", Name = "company_id")]
[Index("CustomerId", Name = "customer_id")]
public partial class PeriodicInvoice
{
    [Key]
    [Column("invoice_id")]
    public int InvoiceId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    /// <summary>
    /// Nhà xe phát hành hóa đơn
    /// </summary>
    [Column("company_id")]
    public int CompanyId { get; set; }

    /// <summary>
    /// Chu kỳ do nhà xe quy định
    /// </summary>
    [Column("billing_cycle", TypeName = "enum('weekly','biweekly','monthly','custom')")]
    public string BillingCycle { get; set; } = null!;

    /// <summary>
    /// YYYY-MM hoặc YYYY-Wxx hoặc custom range
    /// </summary>
    [Column("invoice_period")]
    [StringLength(20)]
    public string? InvoicePeriod { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("total_amount")]
    [Precision(15, 2)]
    public decimal TotalAmount { get; set; }

    [Column("paid_amount")]
    [Precision(15, 2)]
    public decimal? PaidAmount { get; set; }

    [Column("invoice_status", TypeName = "enum('pending','paid','overdue','cancelled')")]
    public string? InvoiceStatus { get; set; }

    [Column("due_date")]
    public DateOnly? DueDate { get; set; }

    [Column("paid_at", TypeName = "timestamp")]
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Điều khoản thanh toán riêng của nhà xe
    /// </summary>
    [Column("payment_terms", TypeName = "text")]
    public string? PaymentTerms { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    // Cross-DB reference: CompanyId references TransportCompany in Platform DB
    [NotMapped]
    public virtual TransportCompany? Company { get; set; }

    // Cross-DB reference: CustomerId references Customer in Platform DB
    [NotMapped]
    public virtual Customer? Customer { get; set; }
}
