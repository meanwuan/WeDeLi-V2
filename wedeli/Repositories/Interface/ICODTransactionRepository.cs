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
        Task<bool> UpdateStatusAsync(int codId, string newStatus);
        Task<IEnumerable<CodTransaction>> GetPendingCollectionsAsync(int driverId);
        Task<IEnumerable<CodTransaction>> GetPendingSubmissionsAsync(int? driverId = null);
        Task<decimal> GetDriverPendingAmountAsync(int driverId);
    }
}
