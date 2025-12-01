using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderStatusHistoryRepository> _logger;

        public OrderStatusHistoryRepository(AppDbContext context, ILogger<OrderStatusHistoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderStatusHistory> GetByIdAsync(int historyId)
        {
            try
            {
                return await _context.OrderStatusHistories
                    .Include(h => h.Order)
                    .FirstOrDefaultAsync(h => h.HistoryId == historyId) ?? new OrderStatusHistory();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status history: {HistoryId}", historyId);
                throw;
            }
        }

        public async Task<IEnumerable<OrderStatusHistory>> GetAllAsync()
        {
            try
            {
                return await _context.OrderStatusHistories
                    .Include(h => h.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all order status histories");
                throw;
            }
        }

        public async Task<OrderStatusHistory> AddAsync(OrderStatusHistory entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _context.OrderStatusHistories.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order status history");
                throw;
            }
        }

        public void Update(OrderStatusHistory entity)
        {
            try
            {
                _context.OrderStatusHistories.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status history");
                throw;
            }
        }

        public void Delete(OrderStatusHistory entity)
        {
            try
            {
                _context.OrderStatusHistories.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order status history");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes");
                throw;
            }
        }

        public async Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.OrderStatusHistories
                    .Where(h => h.OrderId == orderId)
                    .Include(h => h.Order)
                    .OrderByDescending(h => h.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status history: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<OrderStatusHistory> AddStatusChangeAsync(int orderId, string oldStatus, string newStatus, int? updatedBy = null, string? location = null, string? notes = null)
        {
            try
            {
                var history = new OrderStatusHistory
                {
                    OrderId = orderId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = updatedBy,
                    Location = location,
                    Notes = notes
                };

                await _context.OrderStatusHistories.AddAsync(history);
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding status change: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<OrderStatusHistory>> GetRecentChangesAsync(int topN = 50)
        {
            try
            {
                return await _context.OrderStatusHistories
                    .Include(h => h.Order)
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(topN)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent status changes");
                throw;
            }
        }

        public async Task<OrderStatusHistory> UpdateAsync(OrderStatusHistory entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status history");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var history = await GetByIdAsync(id);
                if (history == null)
                    return false;

                Delete(history);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order status history: {HistoryId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.OrderStatusHistories.AnyAsync(h => h.HistoryId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking order status history existence: {HistoryId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.OrderStatusHistories.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting order status histories");
                throw;
            }
        }
    }
}
