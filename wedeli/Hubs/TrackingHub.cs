using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace wedeli.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time order tracking and driver location updates
    /// Supports: Order status updates, Driver location broadcast, Delivery notifications
    /// </summary>
    public class TrackingHub : Hub
    {
        private readonly ILogger<TrackingHub> _logger;

        // Store active connections: ConnectionId -> UserId
        private static readonly ConcurrentDictionary<string, int> _activeConnections = new();

        // Store tracking subscriptions: OrderId -> List<ConnectionIds>
        private static readonly ConcurrentDictionary<int, HashSet<string>> _orderSubscriptions = new();

        public TrackingHub(ILogger<TrackingHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when client connects to hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation($"Client connected: {connectionId}");

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when client disconnects from hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // Remove from active connections
            _activeConnections.TryRemove(connectionId, out _);

            // Remove from all order subscriptions
            foreach (var subscription in _orderSubscriptions)
            {
                subscription.Value.Remove(connectionId);
                if (!subscription.Value.Any())
                {
                    _orderSubscriptions.TryRemove(subscription.Key, out _);
                }
            }

            _logger.LogInformation($"Client disconnected: {connectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client subscribes to track a specific order by tracking code
        /// </summary>
        /// <param name="orderId">Order ID to track</param>
        public async Task SubscribeToOrder(int orderId)
        {
            var connectionId = Context.ConnectionId;

            // Add to subscription list
            if (!_orderSubscriptions.ContainsKey(orderId))
            {
                _orderSubscriptions[orderId] = new HashSet<string>();
            }
            _orderSubscriptions[orderId].Add(connectionId);

            // Add connection to a group for this order
            await Groups.AddToGroupAsync(connectionId, $"Order_{orderId}");

            _logger.LogInformation($"Client {connectionId} subscribed to Order {orderId}");

            // Send confirmation to client
            await Clients.Caller.SendAsync("SubscriptionConfirmed", new
            {
                orderId,
                message = $"Successfully subscribed to order {orderId}"
            });
        }

        /// <summary>
        /// Client unsubscribes from tracking an order
        /// </summary>
        public async Task UnsubscribeFromOrder(int orderId)
        {
            var connectionId = Context.ConnectionId;

            if (_orderSubscriptions.ContainsKey(orderId))
            {
                _orderSubscriptions[orderId].Remove(connectionId);
                if (!_orderSubscriptions[orderId].Any())
                {
                    _orderSubscriptions.TryRemove(orderId, out _);
                }
            }

            await Groups.RemoveFromGroupAsync(connectionId, $"Order_{orderId}");

            _logger.LogInformation($"Client {connectionId} unsubscribed from Order {orderId}");
        }

        /// <summary>
        /// Broadcast order status change to all subscribers
        /// Called from backend service when order status changes
        /// </summary>
        public async Task BroadcastOrderStatusUpdate(int orderId, string oldStatus, string newStatus, string? location, DateTime timestamp)
        {
            var updateData = new
            {
                orderId,
                oldStatus,
                newStatus,
                location,
                timestamp,
                message = GetStatusMessage(newStatus)
            };

            // Send to all clients tracking this order
            await Clients.Group($"Order_{orderId}").SendAsync("OrderStatusUpdated", updateData);

            _logger.LogInformation($"Order {orderId} status updated: {oldStatus} -> {newStatus}");
        }

        /// <summary>
        /// Broadcast driver location update (for live tracking)
        /// </summary>
        public async Task BroadcastDriverLocation(int orderId, int driverId, double latitude, double longitude, DateTime timestamp)
        {
            var locationData = new
            {
                orderId,
                driverId,
                latitude,
                longitude,
                timestamp
            };

            // Send to all clients tracking this order
            await Clients.Group($"Order_{orderId}").SendAsync("DriverLocationUpdated", locationData);

            _logger.LogDebug($"Driver {driverId} location updated for Order {orderId}: ({latitude}, {longitude})");
        }

        /// <summary>
        /// Notify when delivery is completed with photo
        /// </summary>
        public async Task NotifyDeliveryCompleted(int orderId, string photoUrl, string deliveryNotes)
        {
            var deliveryData = new
            {
                orderId,
                photoUrl,
                deliveryNotes,
                completedAt = DateTime.Now,
                message = "Đơn hàng đã được giao thành công!"
            };

            await Clients.Group($"Order_{orderId}").SendAsync("DeliveryCompleted", deliveryData);

            _logger.LogInformation($"Delivery completed notification sent for Order {orderId}");
        }

        /// <summary>
        /// Notify when photo is uploaded (driver upload before/after delivery)
        /// </summary>
        public async Task NotifyPhotoUploaded(int orderId, string photoType, string photoUrl, int uploadedBy)
        {
            var photoData = new
            {
                orderId,
                photoType,
                photoUrl,
                uploadedBy,
                uploadedAt = DateTime.Now
            };

            await Clients.Group($"Order_{orderId}").SendAsync("PhotoUploaded", photoData);

            _logger.LogInformation($"Photo uploaded notification sent for Order {orderId}, Type: {photoType}");
        }

        /// <summary>
        /// Get active subscriber count for an order
        /// </summary>
        public int GetSubscriberCount(int orderId)
        {
            return _orderSubscriptions.ContainsKey(orderId)
                ? _orderSubscriptions[orderId].Count
                : 0;
        }

        /// <summary>
        /// Get user-friendly status message
        /// </summary>
        private string GetStatusMessage(string status)
        {
            return status switch
            {
                "pending_pickup" => "Đang chờ lấy hàng",
                "picked_up" => "Đã lấy hàng",
                "in_transit" => "Đang vận chuyển",
                "out_for_delivery" => "Đang giao hàng",
                "delivered" => "Đã giao hàng thành công",
                "returned" => "Đã hoàn trả",
                "cancelled" => "Đã hủy đơn",
                _ => "Trạng thái không xác định"
            };
        }
    }

    /// <summary>
    /// Extension methods to easily call SignalR hub methods from services
    /// </summary>
    public static class TrackingHubExtensions
    {
        public static async Task NotifyOrderStatusChange(
            this IHubContext<TrackingHub> hubContext,
            int orderId,
            string oldStatus,
            string newStatus,
            string? location = null)
        {
            await hubContext.Clients.Group($"Order_{orderId}").SendAsync(
                "OrderStatusUpdated",
                new
                {
                    orderId,
                    oldStatus,
                    newStatus,
                    location,
                    timestamp = DateTime.Now
                }
            );
        }

        public static async Task NotifyDriverLocationUpdate(
            this IHubContext<TrackingHub> hubContext,
            int orderId,
            int driverId,
            double latitude,
            double longitude)
        {
            await hubContext.Clients.Group($"Order_{orderId}").SendAsync(
                "DriverLocationUpdated",
                new
                {
                    orderId,
                    driverId,
                    latitude,
                    longitude,
                    timestamp = DateTime.Now
                }
            );
        }

        public static async Task NotifyPhotoUpload(
            this IHubContext<TrackingHub> hubContext,
            int orderId,
            string photoType,
            string photoUrl,
            int uploadedBy)
        {
            await hubContext.Clients.Group($"Order_{orderId}").SendAsync(
                "PhotoUploaded",
                new
                {
                    orderId,
                    photoType,
                    photoUrl,
                    uploadedBy,
                    uploadedAt = DateTime.Now
                }
            );
        }
    }
}