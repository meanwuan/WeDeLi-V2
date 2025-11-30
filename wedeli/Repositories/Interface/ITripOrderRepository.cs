using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ITripOrderRepository : IBaseRepository<TripOrder>
    {
        Task<IEnumerable<TripOrder>> GetByTripIdAsync(int tripId);
        Task<TripOrder> GetByOrderIdAsync(int orderId);
        Task<bool> AssignOrderToTripAsync(int tripId, int orderId, int? sequenceNumber = null);
        Task<bool> ConfirmPickupAsync(int tripOrderId);
        Task<bool> ConfirmDeliveryAsync(int tripOrderId);
        Task<bool> RemoveOrderFromTripAsync(int tripOrderId);
    }
}
