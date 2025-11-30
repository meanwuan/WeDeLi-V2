using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IOrderTransferRepository : IBaseRepository<OrderTransfer>
    {
        Task<IEnumerable<OrderTransfer>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderTransfer>> GetByFromCompanyAsync(int companyId);
        Task<IEnumerable<OrderTransfer>> GetByToCompanyAsync(int companyId);
        Task<IEnumerable<OrderTransfer>> GetByStatusAsync(string status, int? companyId = null);
        Task<bool> UpdateStatusAsync(int transferId, string status);
        Task<bool> AcceptTransferAsync(int transferId, int? newVehicleId = null);
        Task<OrderTransfer> CreateTransferAsync(int orderId, int fromCompanyId, int toCompanyId, string reason, int transferredBy);
    }
}
