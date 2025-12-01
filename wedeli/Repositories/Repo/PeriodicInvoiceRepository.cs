using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class PeriodicInvoiceRepository : IPeriodicInvoiceRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PeriodicInvoiceRepository> _logger;

        public PeriodicInvoiceRepository(AppDbContext context, ILogger<PeriodicInvoiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PeriodicInvoice> GetByIdAsync(int invoiceId)
        {
            try
            {
                return await _context.PeriodicInvoices
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId) ?? new PeriodicInvoice();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting periodic invoice: {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<IEnumerable<PeriodicInvoice>> GetAllAsync()
        {
            try
            {
                return await _context.PeriodicInvoices.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all periodic invoices");
                throw;
            }
        }

        public async Task<PeriodicInvoice> AddAsync(PeriodicInvoice entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _context.PeriodicInvoices.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding periodic invoice");
                throw;
            }
        }

        public void Update(PeriodicInvoice entity)
        {
            try
            {
                _context.PeriodicInvoices.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating periodic invoice");
                throw;
            }
        }

        public void Delete(PeriodicInvoice entity)
        {
            try
            {
                _context.PeriodicInvoices.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting periodic invoice");
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

        public async Task<IEnumerable<PeriodicInvoice>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _context.PeriodicInvoices
                    .Where(i => i.CustomerId == customerId)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<PeriodicInvoice>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.PeriodicInvoices
                    .Where(i => i.CompanyId == companyId)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<PeriodicInvoice>> GetByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                var query = _context.PeriodicInvoices
                    .Where(i => i.InvoiceStatus == status);

                if (companyId.HasValue)
                    query = query.Where(i => i.CompanyId == companyId.Value);

                return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<PeriodicInvoice>> GetOverdueInvoicesAsync(int? companyId = null)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var query = _context.PeriodicInvoices
                    .Where(i => i.InvoiceStatus != "paid" && i.DueDate < today);

                if (companyId.HasValue)
                    query = query.Where(i => i.CompanyId == companyId.Value);

                return await query.OrderBy(i => i.DueDate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue invoices");
                throw;
            }
        }

        public async Task<bool> UpdatePaymentAsync(int invoiceId, decimal paidAmount)
        {
            try
            {
                var invoice = await _context.PeriodicInvoices.FindAsync(invoiceId);
                if (invoice == null)
                    return false;

                invoice.PaidAmount = (invoice.PaidAmount ?? 0) + paidAmount;

                if (invoice.PaidAmount >= invoice.TotalAmount)
                    invoice.InvoiceStatus = "paid";

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment: {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(int invoiceId, string status)
        {
            try
            {
                var invoice = await _context.PeriodicInvoices.FindAsync(invoiceId);
                if (invoice == null)
                    return false;

                invoice.InvoiceStatus = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice status: {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<PeriodicInvoice> GenerateInvoiceAsync(int customerId, int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var invoice = new PeriodicInvoice
                {
                    CustomerId = customerId,
                    CompanyId = companyId,
                    StartDate = DateOnly.FromDateTime(startDate),
                    EndDate = DateOnly.FromDateTime(endDate),
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                    InvoiceStatus = "pending",
                    TotalAmount = 0,
                    PaidAmount = 0,
                    BillingCycle = "custom"
                };

                await _context.PeriodicInvoices.AddAsync(invoice);
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<PeriodicInvoice> UpdateAsync(PeriodicInvoice entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating periodic invoice");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var invoice = await GetByIdAsync(id);
                if (invoice == null)
                    return false;

                Delete(invoice);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting periodic invoice: {InvoiceId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.PeriodicInvoices.AnyAsync(i => i.InvoiceId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking periodic invoice existence: {InvoiceId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.PeriodicInvoices.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting periodic invoices");
                throw;
            }
        }
    }
}
