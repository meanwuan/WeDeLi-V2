using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Enums;
using wedeli.Repositories.Interface;

// File: Repositories/Repo/OrderRepository.cs

namespace wedeli.Repositories.Repo;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    #region BASIC CRUD

    public async Task<Order?> GetByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
            .Include(o => o.Vehicle)
            .Include(o => o.Route)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<Order?> GetByTrackingCodeAsync(string trackingCode)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
            .Include(o => o.Route)
            .FirstOrDefaultAsync(o => o.TrackingCode == trackingCode);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        order.CreatedAt = DateTime.UtcNow;
        order.TrackingCode ??= await GenerateTrackingCodeAsync();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return false;

        order.OrderStatus = "cancelled";
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region QUERY BY ROLE

    public async Task<(List<Order> Orders, int TotalCount)> GetByCustomerAsync(
        int customerId, OrderQueryParams p)
    {
        var query = _context.Orders
            .Include(o => o.Route)
            .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
            .Where(o => o.CustomerId == customerId);

        return await ApplyFiltersAndPaginate(query, p);
    }

    public async Task<(List<Order> Orders, int TotalCount)> GetByDriverAsync(
        int driverId, OrderQueryParams p)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Route)
            .Where(o => o.DriverId == driverId);

        return await ApplyFiltersAndPaginate(query, p);
    }

    public async Task<(List<Order> Orders, int TotalCount)> GetByCompanyAsync(
        int companyId, OrderQueryParams p)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
            .Include(o => o.Route)
            .Where(o => o.Route != null && o.Route.CompanyId == companyId);

        return await ApplyFiltersAndPaginate(query, p);
    }

    public async Task<(List<Order> Orders, int TotalCount)> GetAllAsync(OrderQueryParams p)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
            .Include(o => o.Route)
            .AsQueryable();

        return await ApplyFiltersAndPaginate(query, p);
    }

    #endregion

    #region QUERY BY STATUS

    public async Task<List<Order>> GetByStatusAsync(OrderStatus status, int? companyId = null)
    {
        var statusStr = ConvertStatusToDb(status);
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Driver)
            .Where(o => o.OrderStatus == statusStr);

        if (companyId.HasValue)
            query = query.Where(o => o.Route != null && o.Route.CompanyId == companyId);

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<List<Order>> GetPendingPickupAsync(int? companyId = null)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Route)
            .Where(o => o.OrderStatus == "pending_pickup");

        if (companyId.HasValue)
            query = query.Where(o => o.Route != null && o.Route.CompanyId == companyId);

        return await query.OrderBy(o => o.CreatedAt).ToListAsync();
    }

    public async Task<List<Order>> GetInTransitAsync(int? driverId = null)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Route)
            .Where(o => o.OrderStatus == "in_transit" || o.OrderStatus == "out_for_delivery");

        if (driverId.HasValue)
            query = query.Where(o => o.DriverId == driverId);

        return await query.OrderBy(o => o.CreatedAt).ToListAsync();
    }

    #endregion

    #region ORDER HISTORY

    public async Task<List<OrderStatusHistory>> GetStatusHistoryAsync(int orderId)
    {
        return await _context.OrderStatusHistories
            .Include(h => h.UpdatedByNavigation)
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task<OrderStatusHistory> AddStatusHistoryAsync(OrderStatusHistory history)
    {
        if (history.CreatedAt == null || history.CreatedAt == DateTime.MinValue)
            history.CreatedAt = DateTime.UtcNow;

        _context.OrderStatusHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    #endregion

    #region ORDER PHOTOS

    public async Task<List<OrderPhoto>> GetPhotosAsync(int orderId)
    {
        return await _context.OrderPhotos
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.UploadedAt)
            .ToListAsync();
    }

    public async Task<OrderPhoto> AddPhotoAsync(OrderPhoto photo)
    {
        if (photo.UploadedAt == null || photo.UploadedAt == DateTime.MinValue)
            photo.UploadedAt = DateTime.UtcNow;

        _context.OrderPhotos.Add(photo);
        await _context.SaveChangesAsync();
        return photo;
    }

    public async Task<bool> DeletePhotoAsync(int photoId)
    {
        var photo = await _context.OrderPhotos.FindAsync(photoId);
        if (photo == null) return false;

        _context.OrderPhotos.Remove(photo);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region STATISTICS

    public async Task<int> CountByStatusAsync(OrderStatus status, int? companyId = null)
    {
        var statusStr = ConvertStatusToDb(status);
        var query = _context.Orders.Where(o => o.OrderStatus == statusStr);

        if (companyId.HasValue)
            query = query.Where(o => o.Route != null && o.Route.CompanyId == companyId);

        return await query.CountAsync();
    }

    public async Task<int> CountTodayOrdersAsync(int? companyId = null)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var query = _context.Orders.Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow);

        if (companyId.HasValue)
            query = query.Where(o => o.Route != null && o.Route.CompanyId == companyId);

        return await query.CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(
        DateTime fromDate, DateTime toDate, int? companyId = null)
    {
        var query = _context.Orders
            .Where(o => o.OrderStatus == "delivered" &&
                       o.CreatedAt >= fromDate && o.CreatedAt < toDate);

        if (companyId.HasValue)
            query = query.Where(o => o.Route != null && o.Route.CompanyId == companyId);

        return await query.SumAsync(o => o.ShippingFee);
    }

    #endregion

    #region HELPERS

    public async Task<bool> ExistsAsync(int orderId)
    {
        return await _context.Orders.AnyAsync(o => o.OrderId == orderId);
    }

    public async Task<bool> TrackingCodeExistsAsync(string trackingCode)
    {
        return await _context.Orders.AnyAsync(o => o.TrackingCode == trackingCode);
    }

    public async Task<string> GenerateTrackingCodeAsync()
    {
        string code;
        do
        {
            var datePart = DateTime.UtcNow.ToString("yyMMdd");
            var randomPart = GenerateRandomString(6);
            code = $"WD{datePart}{randomPart}";
        }
        while (await TrackingCodeExistsAsync(code));

        return code;
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private async Task<(List<Order> Orders, int TotalCount)> ApplyFiltersAndPaginate(
        IQueryable<Order> query, OrderQueryParams p)
    {
        // Apply filters
        if (p.Status.HasValue)
        {
            var statusStr = ConvertStatusToDb(p.Status.Value);
            query = query.Where(o => o.OrderStatus == statusStr);
        }

        if (p.PaymentStatus.HasValue)
        {
            var paymentStatusStr = ConvertPaymentStatusToDb(p.PaymentStatus.Value);
            query = query.Where(o => o.PaymentStatus == paymentStatusStr);
        }

        if (p.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= p.FromDate.Value);

        if (p.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= p.ToDate.Value);

        if (p.RouteId.HasValue)
            query = query.Where(o => o.RouteId == p.RouteId.Value);

        if (p.VehicleId.HasValue)
            query = query.Where(o => o.VehicleId == p.VehicleId.Value);

        if (!string.IsNullOrWhiteSpace(p.SearchKeyword))
        {
            var kw = p.SearchKeyword.ToLower();
            query = query.Where(o =>
                o.TrackingCode.ToLower().Contains(kw) ||
                o.SenderName.ToLower().Contains(kw) ||
                o.SenderPhone.Contains(kw) ||
                o.ReceiverName.ToLower().Contains(kw) ||
                o.ReceiverPhone.Contains(kw));
        }

        var totalCount = await query.CountAsync();

        query = p.SortBy?.ToLower() switch
        {
            "createdat" => p.SortDescending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt),
            "shippingfee" => p.SortDescending
                ? query.OrderByDescending(o => o.ShippingFee)
                : query.OrderBy(o => o.ShippingFee),
            "status" => p.SortDescending
                ? query.OrderByDescending(o => o.OrderStatus)
                : query.OrderBy(o => o.OrderStatus),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var orders = await query
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    // Helper: Convert C# Enum to DB string
    private static string ConvertStatusToDb(OrderStatus status) => status switch
    {
        OrderStatus.PendingPickup => "pending_pickup",
        OrderStatus.PickedUp => "picked_up",
        OrderStatus.InTransit => "in_transit",
        OrderStatus.OutForDelivery => "out_for_delivery",
        OrderStatus.Delivered => "delivered",
        OrderStatus.Returned => "returned",
        OrderStatus.Cancelled => "cancelled",
        _ => "pending_pickup"
    };

    private static string ConvertPaymentStatusToDb(PaymentStatus status) => status switch
    {
        PaymentStatus.Unpaid => "unpaid",
        PaymentStatus.Paid => "paid",
        PaymentStatus.Pending => "pending",
        _ => "unpaid"
    };

    #endregion
}