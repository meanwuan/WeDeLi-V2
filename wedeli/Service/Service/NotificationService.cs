using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Notification service for managing notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get user notifications
        /// </summary>
        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool? isRead = null)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId, isRead);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

                _logger.LogInformation("Retrieved {Count} notifications for user: {UserId}", notifications.Count(), userId);
                return notificationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user notifications: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        public async Task<NotificationDto> GetNotificationByIdAsync(int notificationId)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                var notificationDto = _mapper.Map<NotificationDto>(notification);

                _logger.LogInformation("Retrieved notification: {NotificationId}", notificationId);
                return notificationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification: {NotificationId}", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Get unread count for user
        /// </summary>
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);

                _logger.LogInformation("Retrieved unread count for user: {UserId} - Count: {Count}", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Create notification
        /// </summary>
        public async Task<NotificationDto> CreateNotificationAsync(NotificationDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var notification = new Notification
                {
                    UserId = dto.UserId,
                    OrderId = dto.OrderId,
                    NotificationType = dto.NotificationType,
                    Title = dto.Title,
                    Message = dto.Message,
                    SentVia = dto.SentVia ?? "push",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                var createdNotification = await _notificationRepository.AddAsync(notification);
                var notificationDto = _mapper.Map<NotificationDto>(createdNotification);

                _logger.LogInformation("Created notification: {NotificationId}", createdNotification.NotificationId);
                return notificationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            try
            {
                var result = await _notificationRepository.MarkAsReadAsync(notificationId);

                _logger.LogInformation("Marked notification as read: {NotificationId}", notificationId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Mark all notifications as read for user
        /// </summary>
        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var result = await _notificationRepository.MarkAllAsReadAsync(userId);

                _logger.LogInformation("Marked all notifications as read for user: {UserId}", userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                var result = await _notificationRepository.DeleteAsync(notificationId);

                _logger.LogInformation("Deleted notification: {NotificationId}", notificationId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification: {NotificationId}", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Send order status notification
        /// </summary>
        public async Task SendOrderStatusNotificationAsync(int orderId, string newStatus)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order not found with ID: {orderId}");

                var title = "Cập nhật trạng thái đơn hàng";
                var message = $"Đơn hàng {order.TrackingCode} có trạng thái mới: {newStatus}";

                // Notify customer
                if (order.CustomerId == null)
                {
                    await _notificationRepository.CreateNotificationAsync(
                        order.CustomerId,
                        orderId,
                        "order_status",
                        title,
                        message,
                        "all"
                    );
                }

                // Notify driver if assigned
                if (order.DriverId.HasValue)
                {
                    await _notificationRepository.CreateNotificationAsync(
                        order.Driver?.CompanyUserId ?? 0,
                        orderId,
                        "order_status",
                        title,
                        message,
                        "all"
                    );
                }

                _logger.LogInformation("Sent order status notification for order: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order status notification: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Send payment notification
        /// </summary>
        public async Task SendPaymentNotificationAsync(int orderId, string paymentStatus)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order not found with ID: {orderId}");

                var title = "Thông báo thanh toán";
                var message = $"Thanh toán cho đơn hàng {order.TrackingCode}: {paymentStatus}";

                if (order.CustomerId == null)
                {
                    await _notificationRepository.CreateNotificationAsync(
                        order.CustomerId,
                        orderId,
                        "payment",
                        title,
                        message,
                        "all"
                    );
                }

                _logger.LogInformation("Sent payment notification for order: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment notification: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Send complaint notification
        /// </summary>
        public async Task SendComplaintNotificationAsync(int complaintId, string status)
        {
            try
            {
                var title = "Cập nhật khiếu nại";
                var message = $"Khiếu nại #{complaintId} - Trạng thái: {status}";

                // This would typically notify the complaint creator
                _logger.LogInformation("Sent complaint notification for complaint: {ComplaintId}", complaintId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending complaint notification: {ComplaintId}", complaintId);
                throw;
            }
        }
    }
}
