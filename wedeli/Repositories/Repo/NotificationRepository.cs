using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Notification repository for managing notification data access
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(AppDbContext context, ILogger<NotificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // IBaseRepository Implementation

        /// <summary>
        /// Get notification by ID
        /// </summary>
        public async Task<Notification> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Notification ID must be greater than 0");

                var notification = await _context.Notifications
                    // NOTE: User is in Platform DB, cannot be included in same query
                    .Include(n => n.Order)
                    .FirstOrDefaultAsync(n => n.NotificationId == id);

                if (notification == null)
                    throw new KeyNotFoundException($"Notification not found with ID: {id}");

                _logger.LogInformation("Retrieved notification: {NotificationId}", id);
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification: {NotificationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all notifications
        /// </summary>
        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            try
            {
                var notifications = await _context.Notifications
                    // NOTE: User is in Platform DB, cannot be included in same query
                    .Include(n => n.Order)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} notifications", notifications.Count);
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all notifications");
                throw;
            }
        }

        /// <summary>
        /// Add new notification
        /// </summary>
        public async Task<Notification> AddAsync(Notification entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                entity.CreatedAt = DateTime.UtcNow;
                _context.Notifications.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created notification: {NotificationId}", entity.NotificationId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        /// <summary>
        /// Update notification
        /// </summary>
        public async Task<Notification> UpdateAsync(Notification entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.NotificationId <= 0)
                    throw new ArgumentException("Notification ID must be greater than 0");

                var existing = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == entity.NotificationId);
                if (existing == null)
                    throw new KeyNotFoundException($"Notification not found with ID: {entity.NotificationId}");

                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated notification: {NotificationId}", entity.NotificationId);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification: {NotificationId}", entity?.NotificationId);
                throw;
            }
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Notification ID must be greater than 0");

                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);
                if (notification == null)
                    throw new KeyNotFoundException($"Notification not found with ID: {id}");

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted notification: {NotificationId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification: {NotificationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if notification exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Notifications.AnyAsync(n => n.NotificationId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking notification existence: {NotificationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count all notifications
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Notifications.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting notifications");
                throw;
            }
        }

        // INotificationRepository Implementation

        /// <summary>
        /// Get notifications by user
        /// </summary>
        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, bool? isRead = null)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0");

                var query = _context.Notifications
                    .Where(n => n.UserId == userId)
                    // NOTE: User is in Platform DB, cannot be included in same query
                    .Include(n => n.Order);

                if (isRead.HasValue)
                {
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Notification, Order?>)query.Where(n => n.IsRead == isRead);
                }

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} notifications for user: {UserId}", notifications.Count, userId);
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications by user: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get notifications by order
        /// </summary>
        public async Task<IEnumerable<Notification>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0");

                var notifications = await _context.Notifications
                    .Where(n => n.OrderId == orderId)
                    // NOTE: User is in Platform DB, cannot be included in same query
                    .Include(n => n.Order)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} notifications for order: {OrderId}", notifications.Count, orderId);
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications by order: {OrderId}", orderId);
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
                if (notificationId <= 0)
                    throw new ArgumentException("Notification ID must be greater than 0");

                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
                if (notification == null)
                    throw new KeyNotFoundException($"Notification not found with ID: {notificationId}");

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marked notification as read: {NotificationId}", notificationId);
                return true;
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
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0");

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsRead == false)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Marked {Count} notifications as read for user: {UserId}", notifications.Count, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read: {UserId}", userId);
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
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0");

                var count = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsRead == false)
                    .CountAsync();

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
        public async Task<Notification> CreateNotificationAsync(int userId, int? orderId, string type, string title, string message, string sentVia = "push")
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0");

                if (string.IsNullOrEmpty(type))
                    throw new ArgumentException("Type cannot be empty");

                var notification = new Notification
                {
                    UserId = userId,
                    OrderId = orderId,
                    NotificationType = type,
                    Title = title,
                    Message = message,
                    SentVia = sentVia,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created notification for user: {UserId}", userId);
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }
    }
}
