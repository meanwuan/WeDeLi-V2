using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Infrastructure;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Order repository for order data access operations
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get order by tracking code
        /// </summary>
        public async Task<Order> GetByTrackingCodeAsync(string trackingCode)
        {
            try
            {
                if (string.IsNullOrEmpty(trackingCode))
                    throw new ArgumentNullException(nameof(trackingCode));

                var order = await _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Include(o => o.Route)
                    .Include(o => o.OrderStatusHistories)
                    .FirstOrDefaultAsync(o => o.TrackingCode == trackingCode);

                if (order == null)
                    throw new KeyNotFoundException($"Order with tracking code {trackingCode} not found.");
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by tracking code: {TrackingCode}", trackingCode);
                throw;
            }
        }

        /// <summary>
        /// Get orders by customer ID with pagination
        /// </summary>
        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (customerId <= 0)
                    throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

                var orders = await _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for customer: {CustomerId}", customerId);
                throw;
            }
        }

        /// <summary>
        /// Get orders by driver ID with optional status filter
        /// </summary>
        public async Task<IEnumerable<Order>> GetByDriverIdAsync(int driverId, string? status = null)
        {
            try
            {
                if (driverId <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(driverId));

                var query = _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Where(o => o.DriverId == driverId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.OrderStatus == status);

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get orders by vehicle ID
        /// </summary>
        public async Task<IEnumerable<Order>> GetByVehicleIdAsync(int vehicleId)
        {
            try
            {
                if (vehicleId <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0", nameof(vehicleId));

                var orders = await _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Where(o => o.VehicleId == vehicleId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for vehicle: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Get orders by status with optional company filter
        /// </summary>
        public async Task<IEnumerable<Order>> GetByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    throw new ArgumentNullException(nameof(status));

                var query = _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Include(o => o.Route)
                    .Where(o => o.OrderStatus == status);

                if (companyId.HasValue && companyId > 0)
                    query = query.Where(o => (o.Route != null && o.Route.CompanyId == companyId) || (o.Vehicle != null && o.Vehicle.CompanyId == companyId));

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get orders within date range
        /// </summary>
        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int? companyId = null)
        {
            try
            {
                if (startDate > endDate)
                    throw new ArgumentException("Start date must be before end date");

                var query = _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

                if (companyId.HasValue && companyId > 0)
                    query = query.Where(o => (o.Route != null && o.Route.CompanyId == companyId) || (o.Vehicle != null && o.Vehicle.CompanyId == companyId));

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by date range");
                throw;
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int orderId, string newStatus, int? updatedBy = null)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

                if (string.IsNullOrEmpty(newStatus))
                    throw new ArgumentNullException(nameof(newStatus));

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                order.OrderStatus = newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated order status - OrderId: {OrderId}, NewStatus: {NewStatus}", orderId, newStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Assign driver and vehicle to order
        /// </summary>
        public async Task<bool> AssignDriverAndVehicleAsync(int orderId, int driverId, int vehicleId)
        {
            try
            {
                if (orderId <= 0 || driverId <= 0 || vehicleId <= 0)
                    throw new ArgumentException("Invalid order, driver, or vehicle ID");

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                order.DriverId = driverId;
                order.VehicleId = vehicleId;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Assigned driver and vehicle to order - OrderId: {OrderId}, DriverId: {DriverId}, VehicleId: {VehicleId}", 
                    orderId, driverId, vehicleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver and vehicle: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Update order payment status
        /// </summary>
        public async Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

                if (string.IsNullOrEmpty(paymentStatus))
                    throw new ArgumentNullException(nameof(paymentStatus));

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                order.PaymentStatus = paymentStatus;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated payment status - OrderId: {OrderId}, PaymentStatus: {PaymentStatus}", orderId, paymentStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Search orders by search term
        /// </summary>
        public async Task<IEnumerable<Order>> SearchOrdersAsync(string searchTerm, int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    throw new ArgumentNullException(nameof(searchTerm));

                searchTerm = searchTerm.ToLower();

                var query = _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    // NOTE: Cannot search by Customer.FullName/Phone here since Customer is in Platform DB
                    // Customer search must be done separately via PlatformDbContext
                    .Where(o => o.TrackingCode.ToLower().Contains(searchTerm) ||
                                o.SenderName.ToLower().Contains(searchTerm) ||
                                o.SenderPhone.Contains(searchTerm) ||
                                o.ReceiverName.ToLower().Contains(searchTerm) ||
                                o.ReceiverPhone.Contains(searchTerm) ||
                                o.SenderAddress.ToLower().Contains(searchTerm) ||
                                o.ReceiverAddress.ToLower().Contains(searchTerm));

                if (companyId.HasValue && companyId > 0)
                    query = query.Where(o => (o.Route != null && o.Route.CompanyId == companyId) || (o.Vehicle != null && o.Vehicle.CompanyId == companyId));

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders: {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Get pending orders
        /// </summary>
        public async Task<IEnumerable<Order>> GetPendingOrdersAsync(int? companyId = null)
        {
            try
            {
                var pendingStatuses = new[] { "pending_pickup", "picked_up", "in_transit" };

                var query = _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Where(o => pendingStatuses.Contains(o.OrderStatus));

                if (companyId.HasValue && companyId > 0)
                    query = query.Where(o => (o.Route != null && o.Route.CompanyId == companyId) || (o.Vehicle != null && o.Vehicle.CompanyId == companyId));

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending orders");
                throw;
            }
        }

        /// <summary>
        /// Get orders by company ID with pagination
        /// </summary>
        public async Task<IEnumerable<Order>> GetByCompanyIdAsync(int companyId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var orders = await _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Include(o => o.Route)
                    .Where(o => (o.Route != null && o.Route.CompanyId == companyId) || 
                               (o.Vehicle != null && o.Vehicle.CompanyId == companyId))
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        public async Task<Order> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(id));

                var order = await _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Include(o => o.OrderStatusHistories)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                    throw new KeyNotFoundException($"Order {id} not found");

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order: {OrderId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                var orders = await _context.Orders
                    // NOTE: Customer is in Platform DB, cannot be included in same query
                    // Customer data should be loaded separately using PlatformDbContext if needed
                    .Include(o => o.Driver)
                    .Include(o => o.Vehicle)
                    .Include(o => o.Route)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                throw;
            }
        }

        /// <summary>
        /// Add new order
        /// </summary>
        public async Task<Order> AddAsync(Order entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _context.Orders.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new order: {OrderId} ({TrackingCode})", entity.OrderId, entity.TrackingCode);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order");
                throw;
            }
        }

        /// <summary>
        /// Update order
        /// </summary>
        public async Task<Order> UpdateAsync(Order entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == entity.OrderId);

                if (existingOrder == null)
                    throw new KeyNotFoundException($"Order {entity.OrderId} not found");

                existingOrder.SenderAddress = entity.SenderAddress;
                existingOrder.ReceiverAddress = entity.ReceiverAddress;
                existingOrder.SpecialInstructions = entity.SpecialInstructions;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated order: {OrderId}", entity.OrderId);
                return existingOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order: {OrderId}", entity?.OrderId);
                throw;
            }
        }

        /// <summary>
        /// Delete order by ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(id));

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                    throw new KeyNotFoundException($"Order {id} not found");

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted order: {OrderId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order: {OrderId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if order exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Orders.AnyAsync(o => o.OrderId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if order exists: {OrderId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count total orders
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Orders.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting orders");
                throw;
            }
        }
    }
}
