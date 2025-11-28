using wedeli.Models.Domain;
using wedeli.Enums;

// File: Repositories/Interface/IOrderRepository.cs

namespace wedeli.Repositories.Interface;

public interface IOrderRepository
{
    // === BASIC CRUD ===
    Task<Order?> GetByIdAsync(int orderId);
    Task<Order?> GetByTrackingCodeAsync(string trackingCode);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<bool> DeleteAsync(int orderId);

    // === QUERY BY ROLE ===
    Task<(List<Order> Orders, int TotalCount)> GetByCustomerAsync(
        int customerId, OrderQueryParams queryParams);

    Task<(List<Order> Orders, int TotalCount)> GetByDriverAsync(
        int driverId, OrderQueryParams queryParams);

    Task<(List<Order> Orders, int TotalCount)> GetByCompanyAsync(
        int companyId, OrderQueryParams queryParams);

    Task<(List<Order> Orders, int TotalCount)> GetAllAsync(OrderQueryParams queryParams);

    // === QUERY BY STATUS ===
    Task<List<Order>> GetByStatusAsync(OrderStatus status, int? companyId = null);
    Task<List<Order>> GetPendingPickupAsync(int? companyId = null);
    Task<List<Order>> GetInTransitAsync(int? driverId = null);

    // === ORDER HISTORY ===
    Task<List<OrderStatusHistory>> GetStatusHistoryAsync(int orderId);
    Task<OrderStatusHistory> AddStatusHistoryAsync(OrderStatusHistory history);

    // === ORDER PHOTOS ===
    Task<List<OrderPhoto>> GetPhotosAsync(int orderId);
    Task<OrderPhoto> AddPhotoAsync(OrderPhoto photo);
    Task<bool> DeletePhotoAsync(int photoId);

    // === STATISTICS ===
    Task<int> CountByStatusAsync(OrderStatus status, int? companyId = null);
    Task<int> CountTodayOrdersAsync(int? companyId = null);
    Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate, int? companyId = null);

    // === HELPERS ===
    Task<bool> ExistsAsync(int orderId);
    Task<bool> TrackingCodeExistsAsync(string trackingCode);
    Task<string> GenerateTrackingCodeAsync();
}

/// <summary>
/// Query parameters cho filtering, sorting, paging
/// </summary>
public class OrderQueryParams
{
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // Filters
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchKeyword { get; set; }
    public int? RouteId { get; set; }
    public int? VehicleId { get; set; }

    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}