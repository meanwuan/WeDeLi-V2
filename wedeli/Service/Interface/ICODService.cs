using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.COD;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// COD (Cash On Delivery) service interface
    /// </summary>
    public interface ICODService
    {
        // COD Transaction management
        Task<CodTransactionResponseDto> GetCODTransactionAsync(int transactionId);
        Task<CodTransactionResponseDto> GetCODByOrderAsync(int orderId);
        Task<IEnumerable<CodTransactionResponseDto>> GetDriverCODTransactionsAsync(int driverId, string status = null);
        
        // Driver workflow
        Task<bool> CollectCODAsync(int orderId, int driverId, string collectionProofPhoto = null);
        Task<bool> SubmitCODToCompanyAsync(SubmitCodDto dto);
        Task<decimal> GetDriverPendingCODAsync(int driverId);
        Task<IEnumerable<CodTransactionResponseDto>> GetPendingCollectionsAsync(int driverId);
        
        // Company workflow
        Task<bool> ConfirmCODReceiptAsync(int transactionId, int receivedBy);
        Task<bool> TransferToSenderAsync(TransferToSenderDto dto);
        
        // Admin reconciliation
        Task<CodDashboardDto> GetDriverCODSummaryAsync(int driverId, DateTime date);
        Task<IEnumerable<CodDashboardDto>> GetPendingReconciliationsAsync(int? companyId = null);
        Task<bool> ReconcileDriverCODAsync(int summaryId, int reconciledBy);
        Task<bool> ReconcileAllDriversAsync(DateTime date, int companyId, int reconciledBy);
        
        // Dashboard
        Task<CodDashboardDto> GetCODDashboardAsync(int? companyId = null);
    }
}
