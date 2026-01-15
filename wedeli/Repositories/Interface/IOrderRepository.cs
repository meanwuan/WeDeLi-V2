using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order> GetByTrackingCodeAsync(string trackingCode);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<Order>> GetByDriverIdAsync(int driverId, string status = null);
        Task<IEnumerable<Order>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Order>> GetByStatusAsync(string status, int? companyId = null);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int? companyId = null);
        Task<bool> UpdateStatusAsync(int orderId, string newStatus, int? updatedBy = null);
        Task<bool> AssignDriverAndVehicleAsync(int orderId, int driverId, int vehicleId);
        Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus);
        Task<IEnumerable<Order>> SearchOrdersAsync(string searchTerm, int? companyId = null);
        Task<IEnumerable<Order>> GetPendingOrdersAsync(int? companyId = null);
        Task<IEnumerable<Order>> GetByCompanyIdAsync(int companyId, int pageNumber = 1, int pageSize = 20);
    }
}
