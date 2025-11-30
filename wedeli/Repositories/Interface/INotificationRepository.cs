using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, bool? isRead = null);
        Task<IEnumerable<Notification>> GetByOrderIdAsync(int orderId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notification> CreateNotificationAsync(int userId, int? orderId, string type, string title, string message, string sentVia = "push");
    }
}
