using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(AppDbContext context, ILogger<PaymentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Payment> GetByIdAsync(int paymentId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId) ?? new Payment();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payments");
                throw;
            }
        }

        public async Task<Payment> AddAsync(Payment entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.PaymentStatus = "pending";
                await _context.Payments.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment");
                throw;
            }
        }

        public void Update(Payment entity)
        {
            try
            {
                entity.PaidAt = DateTime.UtcNow;
                _context.Payments.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment");
                throw;
            }
        }

        public void Delete(Payment entity)
        {
            try
            {
                _context.Payments.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment");
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

        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.Payments
                    .Where(p => p.OrderId == orderId)
                    .Include(p => p.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _context.Payments
                    .Where(p => p.CustomerId == customerId)
                    .Include(p => p.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                var query = _context.Payments
                    .Where(p => p.PaymentStatus == status)
                    .Include(p => p.Order);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int? companyId = null)
        {
            try
            {
                var query = _context.Payments
                    .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                    .Include(p => p.Order);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by date range");
                throw;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment == null)
                    return false;

                payment.PaymentStatus = status;
                payment.PaidAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<Payment> UpdateAsync(Payment entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var payment = await GetByIdAsync(id);
                if (payment == null)
                    return false;

                Delete(payment);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment: {PaymentId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Payments.AnyAsync(p => p.PaymentId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment existence: {PaymentId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Payments.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting payments");
                throw;
            }
        }
    }
}
