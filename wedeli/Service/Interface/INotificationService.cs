using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Notification service interface
    /// </summary>
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool? isRead = null);
        Task<NotificationDto> GetNotificationByIdAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
        
        Task<NotificationDto> CreateNotificationAsync(NotificationDto dto);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
        
        // Specific notification types
        Task SendOrderStatusNotificationAsync(int orderId, string newStatus);
        Task SendPaymentNotificationAsync(int orderId, string paymentStatus);
        Task SendComplaintNotificationAsync(int complaintId, string status);
    }
}