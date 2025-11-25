using wedeli.Models.DTO;
using wedeli.Repositories.Interface;

// File: service/Interface/IOrderService.cs

namespace wedeli.service.Interface;

public interface IOrderService
{
    // === ORDER CRUD ===
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, int customerId);
    Task<OrderResponse> GetOrderByIdAsync(int orderId);
    Task<OrderResponse> GetOrderByTrackingCodeAsync(string trackingCode);
    Task<OrderResponse> UpdateOrderAsync(int orderId, UpdateOrderRequest request, int userId);
    Task<bool> CancelOrderAsync(int orderId, int userId, string? reason = null);

    // === ORDER QUERIES (by role) ===
    Task<PagedOrderResponse> GetCustomerOrdersAsync(int customerId, OrderQueryParams queryParams);
    Task<PagedOrderResponse> GetDriverOrdersAsync(int driverId, OrderQueryParams queryParams);
    Task<PagedOrderResponse> GetCompanyOrdersAsync(int companyId, OrderQueryParams queryParams);
    Task<PagedOrderResponse> GetAllOrdersAsync(OrderQueryParams queryParams);

    // === ORDER STATUS ===
    Task<OrderResponse> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request, int userId);
    Task<List<TrackingEvent>> GetOrderHistoryAsync(int orderId);

    // === ORDER ASSIGNMENT ===
    Task<OrderResponse> AssignOrderAsync(int orderId, AssignOrderRequest request, int assignedBy);
    Task<List<OrderResponse>> BulkAssignOrdersAsync(BulkAssignRequest request, int assignedBy);
    Task<OrderResponse> UnassignOrderAsync(int orderId, int unassignedBy);

    // === TRACKING (PUBLIC) ===
    Task<OrderTrackingResponse> TrackOrderAsync(string trackingCode);

    // === ORDER PHOTOS ===
    Task<OrderPhotoResponse> AddOrderPhotoAsync(int orderId, UploadOrderPhotoRequest request, int uploadedBy);
    Task<List<OrderPhotoResponse>> GetOrderPhotosAsync(int orderId);
    Task<bool> DeleteOrderPhotoAsync(int orderId, int photoId, int userId);

    // === PRICING ===
    Task<ShippingFeeResponse> CalculateShippingFeeAsync(CalculateShippingFeeRequest request);

    // === STATISTICS ===
    Task<OrderStatistics> GetStatisticsAsync(int? companyId = null);

    // === VALIDATION HELPERS ===
    Task<bool> CanUpdateOrderAsync(int orderId);
    Task<bool> CanCancelOrderAsync(int orderId);
    Task<bool> IsOrderAccessibleByUserAsync(int orderId, int userId, string role);
}