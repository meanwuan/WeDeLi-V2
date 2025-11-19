using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("cod_transactions")]
[Index("CollectedByDriver", Name = "collected_by_driver")]
[Index("CompanyReceivedBy", Name = "company_received_by")]
[Index("OrderId", Name = "order_id")]
public partial class CodTransaction
{
    [Key]
    [Column("cod_transaction_id")]
    public int CodTransactionId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    /// <summary>
    /// Số tiền thu hộ
    /// </summary>
    [Column("cod_amount")]
    [Precision(15, 2)]
    public decimal CodAmount { get; set; }

    /// <summary>
    /// Driver ID
    /// </summary>
    [Column("collected_by_driver")]
    public int? CollectedByDriver { get; set; }

    [Column("collected_at", TypeName = "timestamp")]
    public DateTime? CollectedAt { get; set; }

    [Column("collection_status", TypeName = "enum('pending','collected','failed')")]
    public string? CollectionStatus { get; set; }

    /// <summary>
    /// Ảnh xác nhận thu tiền
    /// </summary>
    [Column("collection_proof_photo")]
    [StringLength(500)]
    public string? CollectionProofPhoto { get; set; }

    [Column("submitted_to_company")]
    public bool? SubmittedToCompany { get; set; }

    [Column("submitted_at", TypeName = "timestamp")]
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Số tiền thực nộp (có thể khác nếu có phí)
    /// </summary>
    [Column("submitted_amount")]
    [Precision(15, 2)]
    public decimal? SubmittedAmount { get; set; }

    /// <summary>
    /// Nhân viên nhà xe xác nhận nhận tiền
    /// </summary>
    [Column("company_received_by")]
    public int? CompanyReceivedBy { get; set; }

    [Column("transferred_to_sender")]
    public bool? TransferredToSender { get; set; }

    [Column("transferred_at", TypeName = "timestamp")]
    public DateTime? TransferredAt { get; set; }

    [Column("transfer_method", TypeName = "enum('cash','bank_transfer','e_wallet')")]
    public string? TransferMethod { get; set; }

    [Column("transfer_reference")]
    [StringLength(100)]
    public string? TransferReference { get; set; }

    /// <summary>
    /// Ảnh/file chứng từ chuyển tiền
    /// </summary>
    [Column("transfer_proof")]
    [StringLength(500)]
    public string? TransferProof { get; set; }

    /// <summary>
    /// Phí nhà xe trừ (nếu có)
    /// </summary>
    [Column("company_fee")]
    [Precision(15, 2)]
    public decimal? CompanyFee { get; set; }

    /// <summary>
    /// Điều chỉnh (nếu có)
    /// </summary>
    [Column("adjustment_amount")]
    [Precision(15, 2)]
    public decimal? AdjustmentAmount { get; set; }

    [Column("adjustment_reason", TypeName = "text")]
    public string? AdjustmentReason { get; set; }

    [Column("overall_status", TypeName = "enum('pending_collection','collected','submitted_to_company','completed','failed')")]
    public string? OverallStatus { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CollectedByDriver")]
    [InverseProperty("CodTransactions")]
    public virtual Driver? CollectedByDriverNavigation { get; set; }

    [ForeignKey("CompanyReceivedBy")]
    [InverseProperty("CodTransactions")]
    public virtual User? CompanyReceivedByNavigation { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("CodTransactions")]
    public virtual Order Order { get; set; } = null!;
}
