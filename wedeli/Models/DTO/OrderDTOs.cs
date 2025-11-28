using wedeli.Enums;

// File: Models/DTO/OrderDTOs.cs

namespace wedeli.Models.DTO;

#region REQUEST DTOs

/// <summary>
/// Tạo đơn hàng mới
/// </summary>
public class CreateOrderRequest
{
    // Sender Info
    public string SenderName { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;

    // Receiver Info
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
    public string? ReceiverProvince { get; set; }
    public string? ReceiverDistrict { get; set; }

    // Package Info
    public ParcelType ParcelType { get; set; } = ParcelType.Other;
    public decimal? WeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? SpecialInstructions { get; set; }

    // Pricing
    public decimal? CodAmount { get; set; } = 0;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    // Route
    public int? RouteId { get; set; }

    // Scheduled pickup
    public DateTime? PickupScheduledAt { get; set; }
}

/// <summary>
/// Cập nhật thông tin đơn hàng (chỉ khi chưa pickup)
/// </summary>
public class UpdateOrderRequest
{
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ReceiverAddress { get; set; }
    public string? ReceiverProvince { get; set; }
    public string? ReceiverDistrict { get; set; }

    public ParcelType? ParcelType { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? CodAmount { get; set; }
    public string? SpecialInstructions { get; set; }
}

/// <summary>
/// Cập nhật trạng thái đơn hàng
/// </summary>
public class UpdateOrderStatusRequest
{
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Assign driver/vehicle cho order
/// </summary>
public class AssignOrderRequest
{
    public int? DriverId { get; set; }
    public int? VehicleId { get; set; }
    public int? TripId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Bulk assign nhiều orders cho driver
/// </summary>
public class BulkAssignRequest
{
    public List<int> OrderIds { get; set; } = new();
    public int DriverId { get; set; }
    public int? VehicleId { get; set; }
    public int? TripId { get; set; }
}

/// <summary>
/// Upload ảnh cho order
/// </summary>
public class UploadOrderPhotoRequest
{
    public string PhotoUrl { get; set; } = string.Empty;
    public PhotoType PhotoType { get; set; }
    public string? FileName { get; set; }
    public int? FileSizeKb { get; set; }
}

/// <summary>
/// Tính phí shipping
/// </summary>
public class CalculateShippingFeeRequest
{
    public int RouteId { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? CodAmount { get; set; }
}

#endregion

#region RESPONSE DTOs

/// <summary>
/// Response chi tiết order
/// </summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public string TrackingCode { get; set; } = string.Empty;

    // Sender
    public string SenderName { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;

    // Receiver
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
    public string? ReceiverProvince { get; set; }
    public string? ReceiverDistrict { get; set; }

    // Package
    public string ParcelType { get; set; } = string.Empty;
    public decimal? WeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? SpecialInstructions { get; set; }

    // Pricing
    public decimal ShippingFee { get; set; }
    public decimal CodAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }

    // Status
    public string OrderStatus { get; set; } = string.Empty;

    // References
    public CustomerSummary? Customer { get; set; }
    public DriverSummary? Driver { get; set; }
    public RouteSummary? Route { get; set; }
    public VehicleSummary? Vehicle { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PickupScheduledAt { get; set; }
    public DateTime? PickupConfirmedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

/// <summary>
/// Response list orders (nhẹ hơn)
/// </summary>
public class OrderListItem
{
    public int OrderId { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
    public string? ReceiverProvince { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal CodAmount { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? DriverName { get; set; }
    public string? RouteName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response tracking order (public)
/// </summary>
public class OrderTrackingResponse
{
    public string TrackingCode { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;

    public string ReceiverProvince { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public List<TrackingEvent> TrackingHistory { get; set; } = new();
}

public class TrackingEvent
{
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Kết quả tính phí
/// </summary>
public class ShippingFeeResponse
{
    public decimal BaseFee { get; set; }
    public decimal WeightFee { get; set; }
    public decimal CodFee { get; set; }
    public decimal TotalFee { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Summary cho nested objects
/// </summary>
public class CustomerSummary
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class DriverSummary
{
    public int DriverId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal? Rating { get; set; }
}

public class RouteSummary
{
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string? OriginProvince { get; set; }
    public string? DestinationProvince { get; set; }
}

public class VehicleSummary
{
    public int VehicleId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
}

/// <summary>
/// Paged response wrapper
/// </summary>
public class PagedOrderResponse
{
    public List<OrderListItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Order statistics
/// </summary>
public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public int PendingPickupOrders { get; set; }
    public int InTransitOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int TodayOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
}

/// <summary>
/// Order Photo response
/// </summary>
public class OrderPhotoResponse
{
    public int PhotoId { get; set; }
    public int OrderId { get; set; }
    public string PhotoType { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public int? FileSizeKb { get; set; }
    public DateTime UploadedAt { get; set; }
    public int? UploadedBy { get; set; }
}

#endregion