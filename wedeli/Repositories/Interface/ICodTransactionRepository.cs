using wedeli.Models.Domain;
using wedeli.Models.DTO;

namespace wedeli.Repositories.Interface
{
    public interface ICodTransactionRepository
    {
        // ===== CRUD Operations =====
        Task<CodTransaction?> GetCodTransactionByIdAsync(int codTransactionId);
        Task<CodTransaction?> GetCodTransactionByOrderIdAsync(int orderId);
        Task<(List<CodTransaction> Transactions, int TotalCount)> GetCodTransactionsAsync(CodTransactionFilterDto filter);

        // ===== Driver Operations =====
        Task<List<CodTransaction>> GetDriverPendingCodAsync(int driverId);
        Task<decimal> GetDriverPendingCodAmountAsync(int driverId);
        Task<CodTransaction> RecordCodCollectionAsync(int codTransactionId, string? proofPhotoUrl = null);
        Task<CodTransaction> SubmitCodToCompanyAsync(int driverId, List<int> codTransactionIds);

        // ===== Company Operations =====
        Task<List<CodTransaction>> GetCompanyPendingCodAsync(int companyId);
        Task<(List<CodTransaction> Transactions, decimal TotalAmount)> GetCompanyCodSummaryByDateAsync(int companyId, DateTime date);
        Task<CodTransaction> ReceiveCodFromDriverAsync(int codTransactionId, int receivedByUserId);
        Task<CodTransaction> TransferCodToSenderAsync(int codTransactionId, string transferMethod, string? transferReference = null, string? transferProofUrl = null);

        // ===== Reconciliation =====
        Task<List<CodTransaction>> ReconcileDriverCodAsync(int driverId);
        Task<Dictionary<int, decimal>> GetDailyCodSummaryByDriverAsync(int companyId, DateTime date);
        Task<decimal> CalculateCompanyFeeAsync(decimal codAmount, int companyId);

        // ===== Query & Reports =====
        Task<List<CodTransaction>> GetCodTransactionsByStatusAsync(string status, int? companyId = null);
        Task<CodTransaction> Create(CodTransaction codTransaction);
        Task<CodTransaction> Update(CodTransaction codTransaction);
        Task<bool> DeleteAsync(int codTransactionId);
    }
}
