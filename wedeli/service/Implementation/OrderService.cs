using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Enums;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

// File: service/Implementation/OrderService.cs

namespace wedeli.service.Implementation;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepo,
        ILogger<OrderService> logger)
    {
        _orderRepo = orderRepo;
        _logger = logger;
    }

    #region ORDER CRUD

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest req, int customerId)
    {
        var feeResult = await CalculateShippingFeeAsync(new CalculateShippingFeeRequest
        {
            RouteId = req.RouteId ?? 0,
            WeightKg = req.WeightKg,
            CodAmount = req.CodAmount
        });

        var order = new Order
        {
            CustomerId = customerId,
            TrackingCode = await _orderRepo.GenerateTrackingCodeAsync(),

            SenderName = req.SenderName,
            SenderPhone = req.SenderPhone,
            SenderAddress = req.SenderAddress,

            ReceiverName = req.ReceiverName,
            ReceiverPhone = req.ReceiverPhone,
            ReceiverAddress = req.ReceiverAddress,
            ReceiverProvince = req.ReceiverProvince,
            ReceiverDistrict = req.ReceiverDistrict,

            ParcelType = ConvertParcelTypeToDb(req.ParcelType),
            WeightKg = req.WeightKg,
            DeclaredValue = req.DeclaredValue,
            SpecialInstructions = req.SpecialInstructions,

            ShippingFee = feeResult.TotalFee,
            CodAmount = req.CodAmount ?? 0,
            PaymentMethod = ConvertPaymentMethodToDb(req.PaymentMethod),
            PaymentStatus = "unpaid",

            RouteId = req.RouteId,
            OrderStatus = "pending_pickup",
            PickupScheduledAt = req.PickupScheduledAt,

            CreatedAt = DateTime.UtcNow
        };

        var created = await _orderRepo.CreateAsync(order);

        await _orderRepo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId = created.OrderId,
            OldStatus = null,
            NewStatus = "pending_pickup",
            Notes = "Đơn hàng được tạo",
            UpdatedBy = customerId,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Order created: {TrackingCode} by Customer {CustomerId}",
            created.TrackingCode, customerId);

        return await GetOrderByIdAsync(created.OrderId);
    }

    public async Task<OrderResponse> GetOrderByIdAsync(int orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");
        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse> GetOrderByTrackingCodeAsync(string trackingCode)
    {
        var order = await _orderRepo.GetByTrackingCodeAsync(trackingCode)
            ?? throw new KeyNotFoundException($"Order {trackingCode} not found");
        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse> UpdateOrderAsync(int orderId, UpdateOrderRequest req, int userId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        if (!await CanUpdateOrderAsync(orderId))
            throw new InvalidOperationException("Order cannot be updated in current status");

        if (!string.IsNullOrEmpty(req.ReceiverName))
            order.ReceiverName = req.ReceiverName;
        if (!string.IsNullOrEmpty(req.ReceiverPhone))
            order.ReceiverPhone = req.ReceiverPhone;
        if (!string.IsNullOrEmpty(req.ReceiverAddress))
            order.ReceiverAddress = req.ReceiverAddress;
        if (req.ReceiverProvince != null)
            order.ReceiverProvince = req.ReceiverProvince;
        if (req.ReceiverDistrict != null)
            order.ReceiverDistrict = req.ReceiverDistrict;
        if (req.ParcelType.HasValue)
            order.ParcelType = ConvertParcelTypeToDb(req.ParcelType.Value);
        if (req.WeightKg.HasValue)
            order.WeightKg = req.WeightKg;
        if (req.DeclaredValue.HasValue)
            order.DeclaredValue = req.DeclaredValue;
        if (req.CodAmount.HasValue)
            order.CodAmount = req.CodAmount.Value;
        if (req.SpecialInstructions != null)
            order.SpecialInstructions = req.SpecialInstructions;

        await _orderRepo.UpdateAsync(order);

        _logger.LogInformation("Order {OrderId} updated by User {UserId}", orderId, userId);
        return await GetOrderByIdAsync(orderId);
    }

    public async Task<bool> CancelOrderAsync(int orderId, int userId, string? reason = null)
    {
        var order = await _orderRepo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        if (!await CanCancelOrderAsync(orderId))
            throw new InvalidOperationException("Order cannot be cancelled in current status");

        var oldStatus = order.OrderStatus;
        order.OrderStatus = "cancelled";
        await _orderRepo.UpdateAsync(order);

        await _orderRepo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = "cancelled",
            Notes = reason ?? "Đơn hàng bị hủy",
            UpdatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Order {OrderId} cancelled by User {UserId}", orderId, userId);
        return true;
    }

    #endregion

    #region ORDER QUERIES

    public async Task<PagedOrderResponse> GetCustomerOrdersAsync(int customerId, OrderQueryParams p)
    {
        var (orders, total) = await _orderRepo.GetByCustomerAsync(customerId, p);
        return BuildPagedResponse(orders, total, p);
    }

    public async Task<PagedOrderResponse> GetDriverOrdersAsync(int driverId, OrderQueryParams p)
    {
        var (orders, total) = await _orderRepo.GetByDriverAsync(driverId, p);
        return BuildPagedResponse(orders, total, p);
    }

    public async Task<PagedOrderResponse> GetCompanyOrdersAsync(int companyId, OrderQueryParams p)
    {
        var (orders, total) = await _orderRepo.GetByCompanyAsync(companyId, p);
        return BuildPagedResponse(orders, total, p);
    }

    public async Task<PagedOrderResponse> GetAllOrdersAsync(OrderQueryParams p)
    {
        var (orders, total) = await _orderRepo.GetAllAsync(p);
        return BuildPagedResponse(orders, total, p);
    }

    #endregion

    #region ORDER STATUS

    public async Task<OrderResponse> UpdateOrderStatusAsync(
        int orderId, UpdateOrderStatusRequest req, int userId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        var newStatusStr = ConvertOrderStatusToDb(req.NewStatus);

        if (!IsValidStatusTransition(order.OrderStatus, newStatusStr))
            throw new InvalidOperationException(
                $"Invalid status transition: {order.OrderStatus} -> {newStatusStr}");

        var oldStatus = order.OrderStatus;
        order.OrderStatus = newStatusStr;

        switch (req.NewStatus)
        {
            case OrderStatus.PickedUp:
                order.PickupConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.DeliveredAt = DateTime.UtcNow;
                order.PaymentStatus = "paid";
                order.PaidAt = DateTime.UtcNow;
                break;
        }

        await _orderRepo.UpdateAsync(order);

        if (!string.IsNullOrEmpty(req.PhotoUrl))
        {
            var photoType = req.NewStatus == OrderStatus.PickedUp
                ? "before_delivery"
                : "after_delivery";
            await _orderRepo.AddPhotoAsync(new OrderPhoto
            {
                OrderId = orderId,
                PhotoType = photoType,
                PhotoUrl = req.PhotoUrl,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            });
        }

        await _orderRepo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatusStr,
            Notes = req.Notes ?? GetStatusChangeNote(req.NewStatus),
            Location = req.Location,
            UpdatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Order {OrderId} status: {Old} -> {New} by User {User}",
            orderId, oldStatus, newStatusStr, userId);

        return await GetOrderByIdAsync(orderId);
    }

    public async Task<List<TrackingEvent>> GetOrderHistoryAsync(int orderId)
    {
        var histories = await _orderRepo.GetStatusHistoryAsync(orderId);
        return histories.Select(h => new TrackingEvent
        {
            Status = h.NewStatus ?? "Unknown",
            Description = h.Notes ?? "No description",
            Location = h.Location,
            Timestamp = (DateTime)h.CreatedAt
        }).ToList();
    }

    #endregion

    #region ORDER ASSIGNMENT

    public async Task<OrderResponse> AssignOrderAsync(int orderId, AssignOrderRequest req, int assignedBy)
    {
        var order = await _orderRepo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        if (order.OrderStatus != "pending_pickup")
            throw new InvalidOperationException("Order can only be assigned in PendingPickup status");

        order.DriverId = req.DriverId;
        order.VehicleId = req.VehicleId;

        await _orderRepo.UpdateAsync(order);

        await _orderRepo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = order.OrderStatus,
            NewStatus = order.OrderStatus,
            Notes = req.Notes ?? $"Đã phân công cho tài xế {req.DriverId}",
            UpdatedBy = assignedBy,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Order {OrderId} assigned to Driver {DriverId}", orderId, req.DriverId);
        return await GetOrderByIdAsync(orderId);
    }

    public async Task<List<OrderResponse>> BulkAssignOrdersAsync(BulkAssignRequest req, int assignedBy)
    {
        var results = new List<OrderResponse>();
        foreach (var orderId in req.OrderIds)
        {
            try
            {
                var result = await AssignOrderAsync(orderId, new AssignOrderRequest
                {
                    DriverId = req.DriverId,
                    VehicleId = req.VehicleId,
                    TripId = req.TripId
                }, assignedBy);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to assign order {OrderId}: {Error}", orderId, ex.Message);
            }
        }
        return results;
    }

    public async Task<OrderResponse> UnassignOrderAsync(int orderId, int unassignedBy)
    {
        var order = await _orderRepo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        if (order.OrderStatus != "pending_pickup")
            throw new InvalidOperationException("Only pending_pickup orders can be unassigned");

        var oldDriverId = order.DriverId;
        order.DriverId = null;
        order.VehicleId = null;

        await _orderRepo.UpdateAsync(order);

        await _orderRepo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = order.OrderStatus,
            NewStatus = order.OrderStatus,
            Notes = $"Hủy phân công tài xế {oldDriverId}",
            UpdatedBy = unassignedBy,
            CreatedAt = DateTime.UtcNow
        });

        return await GetOrderByIdAsync(orderId);
    }

    #endregion

    #region TRACKING (PUBLIC)

    public async Task<OrderTrackingResponse> TrackOrderAsync(string trackingCode)
    {
        var order = await _orderRepo.GetByTrackingCodeAsync(trackingCode)
            ?? throw new KeyNotFoundException($"Tracking code {trackingCode} not found");

        var history = await GetOrderHistoryAsync(order.OrderId);

        return new OrderTrackingResponse
        {
            TrackingCode = order.TrackingCode,
            CurrentStatus = order.OrderStatus ?? "Unknown",
            StatusDescription = GetStatusDescriptionFromDb(order.OrderStatus),
            ReceiverProvince = order.ReceiverProvince ?? "N/A",
            ReceiverName = MaskName(order.ReceiverName),
            CreatedAt = order.CreatedAt ?? DateTime.UtcNow,
            EstimatedDelivery = CalculateEstimatedDelivery(order),
            DeliveredAt = order.DeliveredAt,
            TrackingHistory = history
        };
    }

    #endregion

    #region ORDER PHOTOS

    public async Task<OrderPhotoResponse> AddOrderPhotoAsync(int orderId, UploadOrderPhotoRequest req, int uploadedBy)
    {
        if (!await _orderRepo.ExistsAsync(orderId))
            throw new KeyNotFoundException($"Order {orderId} not found");

        var photo = await _orderRepo.AddPhotoAsync(new OrderPhoto
        {
            OrderId = orderId,
            PhotoType = ConvertPhotoTypeToDb(req.PhotoType),
            PhotoUrl = req.PhotoUrl,
            FileName = req.FileName,
            FileSizeKb = req.FileSizeKb,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        });

        return MapToPhotoResponse(photo);
    }

    public async Task<List<OrderPhotoResponse>> GetOrderPhotosAsync(int orderId)
    {
        var photos = await _orderRepo.GetPhotosAsync(orderId);
        return photos.Select(MapToPhotoResponse).ToList();
    }

    public async Task<bool> DeleteOrderPhotoAsync(int orderId, int photoId, int userId)
    {
        return await _orderRepo.DeletePhotoAsync(photoId);
    }

    #endregion

    #region PRICING

    public Task<ShippingFeeResponse> CalculateShippingFeeAsync(CalculateShippingFeeRequest req)
    {
        decimal baseFee = 30000;
        decimal weightFee = (req.WeightKg ?? 0) * 5000;
        decimal codFee = (req.CodAmount ?? 0) * 0.01m;

        return Task.FromResult(new ShippingFeeResponse
        {
            BaseFee = baseFee,
            WeightFee = weightFee,
            CodFee = codFee,
            TotalFee = baseFee + weightFee + codFee
        });
    }

    #endregion

    #region STATISTICS

    public async Task<OrderStatistics> GetStatisticsAsync(int? companyId = null)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        return new OrderStatistics
        {
            PendingPickupOrders = await _orderRepo.CountByStatusAsync(OrderStatus.PendingPickup, companyId),
            InTransitOrders = await _orderRepo.CountByStatusAsync(OrderStatus.InTransit, companyId),
            DeliveredOrders = await _orderRepo.CountByStatusAsync(OrderStatus.Delivered, companyId),
            CancelledOrders = await _orderRepo.CountByStatusAsync(OrderStatus.Cancelled, companyId),
            TodayOrders = await _orderRepo.CountTodayOrdersAsync(companyId),
            TotalRevenue = await _orderRepo.GetTotalRevenueAsync(monthStart, tomorrow, companyId),
            TodayRevenue = await _orderRepo.GetTotalRevenueAsync(today, tomorrow, companyId)
        };
    }

    #endregion

    #region VALIDATION HELPERS

    public async Task<bool> CanUpdateOrderAsync(int orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null) return false;
        return order.OrderStatus == "pending_pickup";
    }

    public async Task<bool> CanCancelOrderAsync(int orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null) return false;
        return order.OrderStatus == "pending_pickup";
    }

    public async Task<bool> IsOrderAccessibleByUserAsync(int orderId, int userId, string role)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null) return false;

        return role.ToLower() switch
        {
            "admin" => true,
            "customer" => order.Customer?.UserId == userId,
            "driver" => order.Driver?.UserId == userId,
            _ => false
        };
    }

    #endregion

    #region PRIVATE HELPERS

    private static bool IsValidStatusTransition(string? from, string to)
    {
        var validTransitions = new Dictionary<string, string[]>
        {
            ["pending_pickup"] = new[] { "picked_up", "cancelled" },
            ["picked_up"] = new[] { "in_transit", "cancelled" },
            ["in_transit"] = new[] { "out_for_delivery", "returned" },
            ["out_for_delivery"] = new[] { "delivered", "returned" },
            ["delivered"] = Array.Empty<string>(),
            ["returned"] = Array.Empty<string>(),
            ["cancelled"] = Array.Empty<string>()
        };

        if (string.IsNullOrEmpty(from)) return true;
        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    private static string GetStatusChangeNote(OrderStatus status) => status switch
    {
        OrderStatus.PendingPickup => "Đơn hàng đang chờ lấy",
        OrderStatus.PickedUp => "Đã lấy hàng",
        OrderStatus.InTransit => "Đang vận chuyển",
        OrderStatus.OutForDelivery => "Đang giao hàng",
        OrderStatus.Delivered => "Đã giao hàng thành công",
        OrderStatus.Returned => "Đã hoàn trả",
        OrderStatus.Cancelled => "Đơn hàng đã bị hủy",
        _ => "Cập nhật trạng thái"
    };

    private static string GetStatusDescriptionFromDb(string? status) => status switch
    {
        "pending_pickup" => "Đơn hàng đang chờ shipper đến lấy",
        "picked_up" => "Shipper đã lấy hàng",
        "in_transit" => "Đơn hàng đang được vận chuyển",
        "out_for_delivery" => "Đơn hàng đang được giao",
        "delivered" => "Đơn hàng đã được giao thành công",
        "returned" => "Đơn hàng đã được hoàn trả",
        "cancelled" => "Đơn hàng đã bị hủy",
        _ => "Trạng thái không xác định"
    };

    private static string MaskName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length <= 2) return name;
        return name[0] + new string('*', name.Length - 2) + name[^1];
    }

    private static DateTime? CalculateEstimatedDelivery(Order order)
    {
        if (order.DeliveredAt.HasValue) return order.DeliveredAt;
        if (order.OrderStatus == "cancelled") return null;
        return (order.CreatedAt ?? DateTime.UtcNow).AddDays(3);
    }

    // Convert C# Enum to DB string
    private static string ConvertOrderStatusToDb(OrderStatus status) => status switch
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

    private static string ConvertPaymentMethodToDb(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "cash",
        PaymentMethod.BankTransfer => "bank_transfer",
        PaymentMethod.EWallet => "e_wallet",
        PaymentMethod.Periodic => "periodic",
        _ => "cash"
    };

    private static string ConvertParcelTypeToDb(ParcelType type) => type switch
    {
        ParcelType.Fragile => "fragile",
        ParcelType.Electronics => "electronics",
        ParcelType.Food => "food",
        ParcelType.Cold => "cold",
        ParcelType.Document => "document",
        ParcelType.Other => "other",
        _ => "other"
    };

    private static string ConvertPhotoTypeToDb(PhotoType type) => type switch
    {
        PhotoType.BeforeDelivery => "before_delivery",
        PhotoType.AfterDelivery => "after_delivery",
        PhotoType.ParcelCondition => "parcel_condition",
        PhotoType.Signature => "signature",
        PhotoType.DamageProof => "damage_proof",
        _ => "before_delivery"
    };

    private OrderResponse MapToOrderResponse(Order o) => new()
    {
        OrderId = o.OrderId,
        TrackingCode = o.TrackingCode,
        SenderName = o.SenderName,
        SenderPhone = o.SenderPhone,
        SenderAddress = o.SenderAddress,
        ReceiverName = o.ReceiverName,
        ReceiverPhone = o.ReceiverPhone,
        ReceiverAddress = o.ReceiverAddress,
        ReceiverProvince = o.ReceiverProvince,
        ReceiverDistrict = o.ReceiverDistrict,
        ParcelType = o.ParcelType ?? "other",
        WeightKg = o.WeightKg ?? 0,
        DeclaredValue = o.DeclaredValue ?? 0,
        SpecialInstructions = o.SpecialInstructions,
        ShippingFee = o.ShippingFee,
        CodAmount = o.CodAmount ?? 0,
        TotalAmount = o.ShippingFee + (o.CodAmount ?? 0),
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus ?? "unpaid",
        PaidAt = o.PaidAt,
        OrderStatus = o.OrderStatus ?? "pending_pickup",
        Customer = o.Customer == null ? null : new CustomerSummary
        {
            CustomerId = o.Customer.CustomerId,
            FullName = o.Customer.FullName,
            Phone = o.Customer.Phone
        },
        Driver = o.Driver == null ? null : new DriverSummary
        {
            DriverId = o.Driver.DriverId,
            FullName = o.Driver.User?.FullName ?? "N/A",
            Phone = o.Driver.User?.Phone ?? "N/A",
            Rating = o.Driver.Rating ?? 0
        },
        Route = o.Route == null ? null : new RouteSummary
        {
            RouteId = o.Route.RouteId,
            RouteName = o.Route.RouteName,
            OriginProvince = o.Route.OriginProvince,
            DestinationProvince = o.Route.DestinationProvince
        },
        Vehicle = o.Vehicle == null ? null : new VehicleSummary
        {
            VehicleId = o.Vehicle.VehicleId,
            LicensePlate = o.Vehicle.LicensePlate,
            VehicleType = o.Vehicle.VehicleType
        },
        CreatedAt = o.CreatedAt ?? DateTime.UtcNow,
        UpdatedAt = o.UpdatedAt,
        PickupScheduledAt = o.PickupScheduledAt,
        PickupConfirmedAt = o.PickupConfirmedAt,
        DeliveredAt = o.DeliveredAt
    };

    private PagedOrderResponse BuildPagedResponse(List<Order> orders, int total, OrderQueryParams p) => new()
    {
        Items = orders.Select(o => new OrderListItem
        {
            OrderId = o.OrderId,
            TrackingCode = o.TrackingCode,
            SenderName = o.SenderName,
            SenderPhone = o.SenderPhone,
            ReceiverName = o.ReceiverName,
            ReceiverPhone = o.ReceiverPhone,
            ReceiverAddress = o.ReceiverAddress,
            ReceiverProvince = o.ReceiverProvince,
            ShippingFee = o.ShippingFee,
            CodAmount = o.CodAmount ?? 0,
            OrderStatus = o.OrderStatus ?? "pending_pickup",
            PaymentStatus = o.PaymentStatus ?? "unpaid",
            DriverName = o.Driver?.User?.FullName,
            RouteName = o.Route?.RouteName,
            CreatedAt = o.CreatedAt ?? DateTime.UtcNow
        }).ToList(),
        TotalCount = total,
        Page = p.Page,
        PageSize = p.PageSize
    };

    private static OrderPhotoResponse MapToPhotoResponse(OrderPhoto p) => new()
    {
        PhotoId = p.PhotoId,
        OrderId = p.OrderId,
        PhotoType = p.PhotoType ?? "before_delivery",
        PhotoUrl = p.PhotoUrl,
        FileName = p.FileName,
        FileSizeKb = p.FileSizeKb,
        UploadedAt = p.UploadedAt ?? DateTime.UtcNow,
        UploadedBy = p.UploadedBy
    };

    #endregion
}