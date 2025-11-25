using wedeli.Models.DTO;

namespace wedeli.service.Interface
{
    public interface ICodTransactionService
    {
        // ===== CRUD Operations =====
        Task<CodTransactionDto?> GetCodTransactionByIdAsync(int codTransactionId);
        Task<CodTransactionDto?> GetCodTransactionByOrderIdAsync(int orderId);
        Task<(List<CodTransactionDto> Transactions, int TotalCount)> GetCodTransactionsAsync(CodTransactionFilterDto filter);
        Task<CodTransactionDto> CreateCodTransactionAsync(CreateCodTransactionDto dto);

        // ===== Driver Operations =====
        Task<DriverCodResponseDto> GetDriverPendingCodAsync(int driverId);
        Task<CodTransactionDto> RecordCodCollectionAsync(int codTransactionId, string? proofPhotoUrl = null);
        Task<CodTransactionDto> SubmitCodToCompanyAsync(int driverId, List<int> codTransactionIds);

        // ===== Company Operations =====
        Task<List<CodDailySummaryDto>> GetCompanyPendingCodAsync(int companyId);
        Task<CompanyCodReconciliationDto> GetCompanyCodReconciliationAsync(int companyId, DateTime date);
        Task<CodTransactionDto> ReceiveCodFromDriverAsync(int codTransactionId, int receivedByUserId);
        Task<CodTransactionDto> TransferCodToSenderAsync(int codTransactionId, string transferMethod, string? transferReference = null, string? transferProofUrl = null);
    }
}
