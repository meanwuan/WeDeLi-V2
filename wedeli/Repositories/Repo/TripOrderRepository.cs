using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Implementation
{
    public class TripOrderRepository : ITripOrderRepository
    {
        private readonly AppDbContext _context;

        public TripOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Base Repository Methods

        public async Task<TripOrder> GetByIdAsync(int id)
        {
            return await _context.TripOrders
                .Include(to => to.Trip)
                    .ThenInclude(t => t.Driver)
                .Include(to => to.Trip)
                    .ThenInclude(t => t.Route)
                .Include(to => to.Trip)
                    .ThenInclude(t => t.Vehicle)
                .Include(to => to.Order)
                .FirstOrDefaultAsync(to => to.TripOrderId == id);
        }

        public async Task<IEnumerable<TripOrder>> GetAllAsync()
        {
            return await _context.TripOrders
                .Include(to => to.Trip)
                .Include(to => to.Order)
                .ToListAsync();
        }

        public async Task<TripOrder> AddAsync(TripOrder entity)
        {
            await _context.TripOrders.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TripOrder> UpdateAsync(TripOrder entity)
        {
            _context.TripOrders.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tripOrder = await _context.TripOrders.FindAsync(id);
            if (tripOrder == null)
                return false;

            _context.TripOrders.Remove(tripOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TripOrders.AnyAsync(to => to.TripOrderId == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.TripOrders.CountAsync();
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Lấy tất cả orders trong một trip, sắp xếp theo sequence_number
        /// </summary>
        public async Task<IEnumerable<TripOrder>> GetByTripIdAsync(int tripId)
        {
            return await _context.TripOrders
                .Include(to => to.Order)
                    .ThenInclude(o => o.Customer)
                .Include(to => to.Order)
                    .ThenInclude(o => o.Driver)
                .Include(to => to.Order)
                    .ThenInclude(o => o.Route)
                .Include(to => to.Order)
                    .ThenInclude(o => o.Vehicle)
                .Where(to => to.TripId == tripId)
                .OrderBy(to => to.SequenceNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy trip order theo order ID
        /// </summary>
        public async Task<TripOrder> GetByOrderIdAsync(int orderId)
        {
            return await _context.TripOrders
                .Include(to => to.Trip)
                    .ThenInclude(t => t.Driver)
                    .ThenInclude(d => d.CompanyUser)
                .Include(to => to.Trip)
                    .ThenInclude(t => t.Vehicle)
                .Include(to => to.Trip)
                    .ThenInclude(t => t.Route)
                .Include(to => to.Order)
                .FirstOrDefaultAsync(to => to.OrderId == orderId);
        }

        /// <summary>
        /// Gán order vào trip với sequence number
        /// </summary>
        public async Task<bool> AssignOrderToTripAsync(int tripId, int orderId, int? sequenceNumber = null)
        {
            // Kiểm tra trip và order có tồn tại không
            var trip = await _context.Trips.FindAsync(tripId);
            var order = await _context.Orders.FindAsync(orderId);

            if (trip == null || order == null)
                return false;

            // Kiểm tra order đã được gán vào trip nào chưa
            var existingAssignment = await _context.TripOrders
                .AnyAsync(to => to.OrderId == orderId);

            if (existingAssignment)
                return false;

            // Nếu không có sequence number, tự động tính
            if (!sequenceNumber.HasValue)
            {
                var maxSequence = await _context.TripOrders
                    .Where(to => to.TripId == tripId)
                    .MaxAsync(to => (int?)to.SequenceNumber) ?? 0;

                sequenceNumber = maxSequence + 1;
            }

            var tripOrder = new TripOrder
            {
                TripId = tripId,
                OrderId = orderId,
                SequenceNumber = sequenceNumber,
                PickupConfirmed = false,
                DeliveryConfirmed = false
            };

            await _context.TripOrders.AddAsync(tripOrder);

            // Cập nhật total_orders trong trip
            trip.TotalOrders = await _context.TripOrders.CountAsync(to => to.TripId == tripId) + 1;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xác nhận đã pickup order (pickup_confirmed = true)
        /// </summary>
        public async Task<bool> ConfirmPickupAsync(int tripOrderId)
        {
            var tripOrder = await _context.TripOrders.FindAsync(tripOrderId);

            if (tripOrder == null)
                return false;

            tripOrder.PickupConfirmed = true;

            _context.TripOrders.Update(tripOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xác nhận đã delivery order (delivery_confirmed = true)
        /// </summary>
        public async Task<bool> ConfirmDeliveryAsync(int tripOrderId)
        {
            var tripOrder = await _context.TripOrders.FindAsync(tripOrderId);

            if (tripOrder == null)
                return false;

            tripOrder.DeliveryConfirmed = true;

            _context.TripOrders.Update(tripOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xóa order khỏi trip
        /// </summary>
        public async Task<bool> RemoveOrderFromTripAsync(int tripOrderId)
        {
            var tripOrder = await _context.TripOrders.FindAsync(tripOrderId);

            if (tripOrder == null)
                return false;

            var tripId = tripOrder.TripId;

            _context.TripOrders.Remove(tripOrder);

            // Cập nhật lại total_orders trong trip
            var trip = await _context.Trips.FindAsync(tripId);
            if (trip != null)
            {
                trip.TotalOrders = await _context.TripOrders.CountAsync(to => to.TripId == tripId) - 1;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}