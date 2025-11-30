using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ICODTransactionRepository : IBaseRepository<CodTransaction>
    {
        Task<CodTransaction> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<CodTransaction>> GetByDriverIdAsync(int driverId, string status = null);
        Task<bool> ConfirmCollectionAsync(int codTransactionId, int driverId, string collectionProofPhoto = null);
        Task<bool> SubmitToCompanyAsync(int codTransactionId, decimal submittedAmount, int receivedBy);
        Task<bool> TransferToSenderAsync(int codTransactionId, string transferMethod, string transferReference = null);
        Task<bool> UpdateOverallStatusAsync(int codTransactionId, string status);
        Task<IEnumerable<CodTransaction>> GetPendingCollectionsAsync(int? driverId = null);
        Task<IEnumerable<CodTransaction>> GetPendingSubmissionsAsync(int? driverId = null);
        Task<decimal> GetDriverPendingAmountAsync(int driverId);
    }
}
