using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Order service interface for order management
    /// </summary>
    public interface IOrderService
    {
        // Basic CRUD
        Task<OrderDetailDto> GetOrderByIdAsync(int orderId);
        Task<OrderResponseDto> GetOrderByTrackingCodeAsync(string trackingCode);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(int customerId, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByDriverAsync(int driverId, string status = null);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(string status, int? companyId = null);
        Task<IEnumerable<OrderResponseDto>> GetPendingOrdersAsync(int? companyId = null);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 20);
        
        // Order creation & management
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderResponseDto> UpdateOrderAsync(int orderId, UpdateOrderDto dto);
        Task<bool> CancelOrderAsync(int orderId, string reason, int? cancelledBy = null);
        Task<bool> AssignDriverAndVehicleAsync(int orderId, int driverId, int vehicleId, int? assignedBy = null);
        
        // Order status workflow
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, int? updatedBy = null, string notes = null);
        Task<bool> ConfirmPickupAsync(int orderId, int? confirmedBy = null);
        Task<bool> MarkInTransitAsync(int orderId);
        Task<bool> MarkOutForDeliveryAsync(int orderId);
        Task<bool> CompleteDeliveryAsync(int orderId, int? deliveredBy = null, string signature = null);
        Task<bool> MarkAsReturnedAsync(int orderId, string reason);
        
        // Order tracking
        Task<OrderTrackingDto> TrackOrderAsync(string trackingCode);
        Task<IEnumerable<OrderStatusDto>> GetOrderHistoryAsync(int orderId);
        
        // Statistics
        Task<IEnumerable<OrderResponseDto>> SearchOrdersAsync(string searchTerm, int? companyId = null);
    }
}
