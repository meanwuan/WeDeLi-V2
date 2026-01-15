using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ComplaintRepository> _logger;

        public ComplaintRepository(AppDbContext context, ILogger<ComplaintRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Complaint> GetByIdAsync(int complaintId)
        {
            try
            {
                return await _context.Complaints
                    .Include(c => c.Order)
                    .FirstOrDefaultAsync(c => c.ComplaintId == complaintId) ?? new Complaint();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaint: {ComplaintId}", complaintId);
                throw;
            }
        }

        public async Task<IEnumerable<Complaint>> GetAllAsync()
        {
            try
            {
                return await _context.Complaints
                    .Include(c => c.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all complaints");
                throw;
            }
        }

        public async Task<Complaint> AddAsync(Complaint entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.ComplaintStatus = "pending";
                await _context.Complaints.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding complaint");
                throw;
            }
        }

        public void Update(Complaint entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                _context.Complaints.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating complaint");
                throw;
            }
        }

        public void Delete(Complaint entity)
        {
            try
            {
                _context.Complaints.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting complaint");
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

        public async Task<IEnumerable<Complaint>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.Complaints
                    .Where(c => c.OrderId == orderId)
                    .Include(c => c.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaints by order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<Complaint>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _context.Complaints
                    .Where(c => c.CustomerId == customerId)
                    .Include(c => c.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaints by customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<Complaint>> GetByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                var query = _context.Complaints
                    .Where(c => c.ComplaintStatus == status)
                    .Include(c => c.Order);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaints by status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<Complaint>> GetByTypeAsync(string complaintType, int? companyId = null)
        {
            try
            {
                var query = _context.Complaints
                    .Where(c => c.ComplaintType == complaintType)
                    .Include(c => c.Order);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaints by type: {Type}", complaintType);
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(int complaintId, string status, string? resolutionNotes = null, int? resolvedBy = null)
        {
            try
            {
                var complaint = await _context.Complaints.FindAsync(complaintId);
                if (complaint == null)
                    return false;

                complaint.ComplaintStatus = status;
                
                if (!string.IsNullOrEmpty(resolutionNotes))
                    complaint.ResolutionNotes = resolutionNotes;
                
                if (resolvedBy.HasValue)
                    complaint.ResolvedBy = resolvedBy.Value;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating complaint status: {ComplaintId}", complaintId);
                throw;
            }
        }

        public async Task<IEnumerable<Complaint>> GetPendingComplaintsAsync(int? companyId = null)
        {
            try
            {
                var query = _context.Complaints
                    .Where(c => c.ComplaintStatus == "pending")
                    .Include(c => c.Order);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending complaints");
                throw;
            }
        }

        public async Task<Complaint> UpdateAsync(Complaint entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating complaint");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var complaint = await GetByIdAsync(id);
                if (complaint == null)
                    return false;

                Delete(complaint);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting complaint: {ComplaintId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Complaints.AnyAsync(c => c.ComplaintId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking complaint existence: {ComplaintId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Complaints.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting complaints");
                throw;
            }
        }
    }
}
