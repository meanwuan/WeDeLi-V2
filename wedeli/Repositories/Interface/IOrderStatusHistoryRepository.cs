using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IOrderStatusHistoryRepository : IBaseRepository<OrderStatusHistory>
    {
        Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId);
        Task<OrderStatusHistory> AddStatusChangeAsync(int orderId, string oldStatus, string newStatus, int? updatedBy = null, string location = null, string notes = null);
        Task<IEnumerable<OrderStatusHistory>> GetRecentChangesAsync(int topN = 50);
    }
}
