using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class TransportCompanyRepository : ITransportCompanyRepository
    {
        private readonly PlatformDbContext _context;
        private readonly ILogger<TransportCompanyRepository> _logger;

        public TransportCompanyRepository(PlatformDbContext context, ILogger<TransportCompanyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TransportCompany> GetByIdAsync(int companyId)
        {
            try
            {
                return await _context.TransportCompanies
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId) ?? new TransportCompany();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transport company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<TransportCompany>> GetAllAsync()
        {
            try
            {
                return await _context.TransportCompanies.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all transport companies");
                throw;
            }
        }

        public async Task<TransportCompany> AddAsync(TransportCompany entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _context.TransportCompanies.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding transport company");
                throw;
            }
        }

        public void Update(TransportCompany entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                _context.TransportCompanies.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transport company");
                throw;
            }
        }

        public void Delete(TransportCompany entity)
        {
            try
            {
                _context.TransportCompanies.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transport company");
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

        public async Task<IEnumerable<TransportCompany>> GetActiveCompaniesAsync()
        {
            try
            {
                return await _context.TransportCompanies
                    .Where(c => c.IsActive == true)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active companies");
                throw;
            }
        }

        public async Task<bool> UpdateRatingAsync(int companyId, decimal rating)
        {
            try
            {
                var company = await _context.TransportCompanies.FindAsync(companyId);
                if (company == null)
                    return false;

                company.Rating = rating;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company rating: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<TransportCompany> GetByNameAsync(string companyName)
        {
            try
            {
                return await _context.TransportCompanies
                    .FirstOrDefaultAsync(c => c.CompanyName == companyName) ?? new TransportCompany();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company by name: {CompanyName}", companyName);
                throw;
            }
        }

        public async Task<TransportCompany?> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.TransportCompanies
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company by userId: {UserId}", userId);
                throw;
            }
        }

        public async Task<TransportCompany> UpdateAsync(TransportCompany entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transport company");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var company = await GetByIdAsync(id);
                if (company == null)
                    return false;

                Delete(company);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transport company: {CompanyId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.TransportCompanies.AnyAsync(c => c.CompanyId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking transport company existence: {CompanyId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.TransportCompanies.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting transport companies");
                throw;
            }
        }
    }
}
