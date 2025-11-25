using System;

namespace wedeli.Models.DTO
{
    /// <summary>
    /// DTO for COD Transaction
    /// </summary>
    public class CodTransactionDto
    {
        public int CodTransactionId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; } = null!;
        public decimal CodAmount { get; set; }
        public int? CollectedByDriver { get; set; }
        public string? DriverName { get; set; }
        public DateTime? CollectedAt { get; set; }
        public string? CollectionStatus { get; set; }
        public string? CollectionProofPhoto { get; set; }

        public bool? SubmittedToCompany { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public decimal? SubmittedAmount { get; set; }

        public int? CompanyReceivedBy { get; set; }
        public bool? TransferredToSender { get; set; }
        public DateTime? TransferredAt { get; set; }
        public string? TransferMethod { get; set; }
        public string? TransferReference { get; set; }

        public decimal? CompanyFee { get; set; }
        public decimal? AdjustmentAmount { get; set; }
        public string? AdjustmentReason { get; set; }
        public string? OverallStatus { get; set; }
        public string? Notes { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating COD Transaction
    /// </summary>
    public class CreateCodTransactionDto
    {
        public int OrderId { get; set; }
        public decimal CodAmount { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating COD Transaction
    /// </summary>
    public class UpdateCodTransactionDto
    {
        public decimal? AdjustmentAmount { get; set; }
        public string? AdjustmentReason { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for recording COD collection
    /// </summary>
    public class RecordCodCollectionDto
    {
        public int CodTransactionId { get; set; }
        public string? ProofPhotoUrl { get; set; }
    }

    /// <summary>
    /// DTO for submitting COD to company
    /// </summary>
    public class SubmitCodToCompanyDto
    {
        public int DriverId { get; set; }
        public List<int> CodTransactionIds { get; set; } = new();
    }

    /// <summary>
    /// DTO for transferring COD to sender
    /// </summary>
    public class TransferCodToSenderDto
    {
        public int CodTransactionId { get; set; }
        public string TransferMethod { get; set; } = null!; // cash, bank_transfer, e_wallet
        public string? TransferReference { get; set; }
        public string? TransferProofUrl { get; set; }
    }

    /// <summary>
    /// Filter parameters for COD queries
    /// </summary>
    public class CodTransactionFilterDto
    {
        public int? CompanyId { get; set; }
        public int? DriverId { get; set; }
        public string? CollectionStatus { get; set; } // pending, collected, failed
        public string? OverallStatus { get; set; } // pending_collection, collected, submitted_to_company, completed, failed
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for COD Daily Summary
    /// </summary>
    public class CodDailySummaryDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = null!;
        public DateTime Date { get; set; }
        public int TotalCodCollected { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? TotalSubmitted { get; set; }
        public List<CodTransactionDto> Transactions { get; set; } = new();
    }

    /// <summary>
    /// DTO for driver COD response
    /// </summary>
    public class DriverCodResponseDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = null!;
        public decimal TotalPendingCod { get; set; }
        public int PendingCodCount { get; set; }
        public List<CodTransactionDto> PendingTransactions { get; set; } = new();
    }

    /// <summary>
    /// DTO for company COD reconciliation
    /// </summary>
    public class CompanyCodReconciliationDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalSubmittedByDrivers { get; set; }
        public decimal Variance { get; set; }
        public int TransactionCount { get; set; }
        public List<CodDailySummaryDto> DriverSummaries { get; set; } = new();
    }
}
