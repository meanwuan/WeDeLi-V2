using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.COD
{
    // ============================================
    // COD TRANSACTION DTOs
    // ============================================

    public class CodTransactionResponseDto
    {
        public int CodTransactionId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public decimal CodAmount { get; set; }
        public int? CollectedByDriver { get; set; }
        public string DriverName { get; set; }
        public DateTime? CollectedAt { get; set; }
        public string CollectionStatus { get; set; }
        public string CollectionProofPhoto { get; set; }
        public bool SubmittedToCompany { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public decimal? SubmittedAmount { get; set; }
        public bool TransferredToSender { get; set; }
        public DateTime? TransferredAt { get; set; }
        public string TransferMethod { get; set; }
        public decimal CompanyFee { get; set; }
        public string OverallStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CollectCodDto
    {
        [Required] public int OrderId { get; set; }
        [Required][Range(0, 999999999)] public decimal CodAmount { get; set; }
        public string CollectionProofPhotoUrl { get; set; }
        public string Notes { get; set; }
    }

    public class SubmitCodDto
    {
        [Required] public int DriverId { get; set; }
        [Required] public List<int> TransactionIds { get; set; }
        [Required][Range(0, 999999999)] public decimal TotalAmount { get; set; }
        public string Notes { get; set; }
    }

    public class TransferToSenderDto
    {
        [Required] public int TransactionId { get; set; }
        [Required] public string TransferMethod { get; set; }
        public string TransferReference { get; set; }
        public string TransferProofUrl { get; set; }
        [Range(0, 999999999)] public decimal? CompanyFee { get; set; }
    }

    public class ReconcileCodDto
    {
        [Required] public int DriverId { get; set; }
        [Required] public DateTime ReconciliationDate { get; set; }
        public string Notes { get; set; }
    }

    public class CodDashboardDto
    {
        public decimal TotalPendingCollection { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalSubmitted { get; set; }
        public decimal TotalTransferred { get; set; }
        public int PendingTransactionCount { get; set; }
        public int CompletedTransactionCount { get; set; }
        public List<DriverCodSummaryDto> DriverSummaries { get; set; }
    }

    public class DriverCodSummaryDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalSubmitted { get; set; }
        public decimal PendingAmount { get; set; }
        public int PendingTransactionCount { get; set; }
        public DateTime? LastSubmissionDate { get; set; }
    }
}

namespace wedeli.Models.DTO.Payment
{
    // ============================================
    // PAYMENT DTOs
    // ============================================

    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentType { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionRef { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required] public int OrderId { get; set; }
        [Required] public int CustomerId { get; set; }
        [Required][Range(0, 999999999)] public decimal Amount { get; set; }
        [Required] public string PaymentMethod { get; set; }
        [Required] public string PaymentType { get; set; }
        public string TransactionRef { get; set; }
        public string Notes { get; set; }
    }

    public class ConfirmPaymentDto
    {
        [Required] public int PaymentId { get; set; }
        [Required] public string TransactionRef { get; set; }
        public string Notes { get; set; }
    }

    public class PeriodicInvoiceResponseDto
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string BillingCycle { get; set; }
        public string InvoicePeriod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string InvoiceStatus { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public string PaymentTerms { get; set; }
        public List<InvoiceOrderDto> Orders { get; set; }
    }

    public class InvoiceOrderDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public decimal ShippingFee { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class GenerateInvoiceDto
    {
        [Required] public int CustomerId { get; set; }
        [Required] public int CompanyId { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentTerms { get; set; }
    }

    public class PayInvoiceDto
    {
        [Required] public int InvoiceId { get; set; }
        [Required][Range(0, 999999999)] public decimal Amount { get; set; }
        [Required] public string PaymentMethod { get; set; }
        public string TransactionRef { get; set; }
        public string Notes { get; set; }
    }
}