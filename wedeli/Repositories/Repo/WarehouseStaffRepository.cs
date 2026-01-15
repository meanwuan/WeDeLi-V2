using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class WarehouseStaffRepository : IWarehouseStaffRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WarehouseStaffRepository> _logger;

        public WarehouseStaffRepository(AppDbContext context, ILogger<WarehouseStaffRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WarehouseStaff> GetByIdAsync(int staffId)
        {
            try
            {
                return await _context.WarehouseStaffs
                    .Include(w => w.CompanyUser)
                    .FirstOrDefaultAsync(w => w.StaffId == staffId) ?? new WarehouseStaff();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse staff: {StaffId}", staffId);
                throw;
            }
        }

        public async Task<IEnumerable<WarehouseStaff>> GetAllAsync()
        {
            try
            {
                return await _context.WarehouseStaffs
                    .Include(w => w.CompanyUser)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all warehouse staff");
                throw;
            }
        }

        public async Task<WarehouseStaff> AddAsync(WarehouseStaff entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _context.WarehouseStaffs.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding warehouse staff");
                throw;
            }
        }

        public void Update(WarehouseStaff entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                _context.WarehouseStaffs.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse staff");
                throw;
            }
        }

        public void Delete(WarehouseStaff entity)
        {
            try
            {
                _context.WarehouseStaffs.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse staff");
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

        public async Task<WarehouseStaff> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.WarehouseStaffs
                    .Include(w => w.CompanyUser)
                    .FirstOrDefaultAsync(w => w.CompanyUserId == userId) ?? new WarehouseStaff();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse staff by user: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<WarehouseStaff>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.WarehouseStaffs
                    .Where(w => w.CompanyId == companyId)
                    .Include(w => w.CompanyUser)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse staff by company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<WarehouseStaff>> GetByLocationAsync(string location)
        {
            try
            {
                return await _context.WarehouseStaffs
                    .Where(w => w.WarehouseLocation == location)
                    .Include(w => w.CompanyUser)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse staff by location: {Location}", location);
                throw;
            }
        }

        public async Task<bool> ToggleActiveStatusAsync(int staffId, bool isActive)
        {
            try
            {
                var staff = await _context.WarehouseStaffs.FindAsync(staffId);
                if (staff == null)
                    return false;

                staff.IsActive = isActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status: {StaffId}", staffId);
                throw;
            }
        }

        public async Task<WarehouseStaff> UpdateAsync(WarehouseStaff entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse staff");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var staff = await GetByIdAsync(id);
                if (staff == null)
                    return false;

                Delete(staff);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse staff: {StaffId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.WarehouseStaffs.AnyAsync(w => w.StaffId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking warehouse staff existence: {StaffId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.WarehouseStaffs.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting warehouse staff");
                throw;
            }
        }
    }
}
