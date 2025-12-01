using System;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Payment
{
    // ============================================
    // CREATE PAYMENT
    // ============================================

    public class CreatePaymentPaymentDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string? PaymentMethod { get; set; }

        public string? Reference { get; set; }

        public string? Notes { get; set; }
    }

    // ============================================
    // PAYMENT RESPONSE
    // ============================================

    public class PaymentPaymentResponseDto
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? PaymentStatus { get; set; }

        public string? TransactionRef { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? Notes { get; set; }
    }

    // ============================================
    // UPDATE PAYMENT
    // ============================================

    public class UpdatePaymentStatusDto
    {
        [Required]
        public string? PaymentStatus { get; set; }

        public string? Notes { get; set; }
    }

    // ============================================
    // PAYMENT STATISTICS
    // ============================================

    public class PaymentStatisticsDto
    {
        public decimal TotalAmount { get; set; }

        public int TotalCount { get; set; }

        public decimal PendingAmount { get; set; }

        public int PendingCount { get; set; }

        public decimal CompletedAmount { get; set; }

        public int CompletedCount { get; set; }

        public decimal FailedAmount { get; set; }

        public int FailedCount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
