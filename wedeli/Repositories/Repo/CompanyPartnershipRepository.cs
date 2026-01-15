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
    /// Company partnership repository for managing partnership data access
    /// </summary>
    public class CompanyPartnershipRepository : ICompanyPartnershipRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompanyPartnershipRepository> _logger;

        public CompanyPartnershipRepository(AppDbContext context, ILogger<CompanyPartnershipRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // IBaseRepository Implementation

        /// <summary>
        /// Get partnership by ID
        /// </summary>
        public async Task<CompanyPartnership> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Partnership ID must be greater than 0");

                var partnership = await _context.CompanyPartnerships
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .Include(p => p.CreatedByNavigation)
                    .FirstOrDefaultAsync(p => p.PartnershipId == id);

                if (partnership == null)
                    throw new KeyNotFoundException($"Partnership not found with ID: {id}");

                _logger.LogInformation("Retrieved partnership: {PartnershipId}", id);
                return partnership;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partnership: {PartnershipId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all partnerships
        /// </summary>
        public async Task<IEnumerable<CompanyPartnership>> GetAllAsync()
        {
            try
            {
                var partnerships = await _context.CompanyPartnerships
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .Include(p => p.CreatedByNavigation)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} partnerships", partnerships.Count);
                return partnerships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all partnerships");
                throw;
            }
        }

        /// <summary>
        /// Add new partnership
        /// </summary>
        public async Task<CompanyPartnership> AddAsync(CompanyPartnership entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Check for duplicate partnership
                var existing = await _context.CompanyPartnerships
                    .FirstOrDefaultAsync(p => p.CompanyId == entity.CompanyId && p.PartnerCompanyId == entity.PartnerCompanyId);

                if (existing != null)
                    throw new InvalidOperationException($"Partnership already exists between these companies");

                entity.CreatedAt = DateTime.UtcNow;
                entity.IsActive = true;

                _context.CompanyPartnerships.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created partnership: {PartnershipId}", entity.PartnershipId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partnership");
                throw;
            }
        }

        /// <summary>
        /// Update partnership
        /// </summary>
        public async Task<CompanyPartnership> UpdateAsync(CompanyPartnership entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.PartnershipId <= 0)
                    throw new ArgumentException("Partnership ID must be greater than 0");

                var existingPartnership = await _context.CompanyPartnerships
                    .FirstOrDefaultAsync(p => p.PartnershipId == entity.PartnershipId);

                if (existingPartnership == null)
                    throw new KeyNotFoundException($"Partnership not found with ID: {entity.PartnershipId}");

                entity.UpdatedAt = DateTime.UtcNow;
                _context.Entry(existingPartnership).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated partnership: {PartnershipId}", entity.PartnershipId);
                return existingPartnership;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partnership: {PartnershipId}", entity?.PartnershipId);
                throw;
            }
        }

        /// <summary>
        /// Delete partnership
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Partnership ID must be greater than 0");

                var partnership = await _context.CompanyPartnerships
                    .FirstOrDefaultAsync(p => p.PartnershipId == id);

                if (partnership == null)
                    throw new KeyNotFoundException($"Partnership not found with ID: {id}");

                _context.CompanyPartnerships.Remove(partnership);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted partnership: {PartnershipId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partnership: {PartnershipId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if partnership exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.CompanyPartnerships.AnyAsync(p => p.PartnershipId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking partnership existence: {PartnershipId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count all partnerships
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.CompanyPartnerships.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting partnerships");
                throw;
            }
        }

        // ICompanyPartnershipRepository Implementation

        /// <summary>
        /// Get partnerships by company
        /// </summary>
        public async Task<IEnumerable<CompanyPartnership>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0");

                var partnerships = await _context.CompanyPartnerships
                    .Where(p => p.CompanyId == companyId)
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .OrderBy(p => p.PriorityOrder)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} partnerships for company: {CompanyId}", partnerships.Count, companyId);
                return partnerships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partnerships by company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get partnerships by level
        /// </summary>
        public async Task<IEnumerable<CompanyPartnership>> GetPartnersByLevelAsync(int companyId, string level)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0");

                if (string.IsNullOrEmpty(level))
                    throw new ArgumentException("Level cannot be empty");

                var partnerships = await _context.CompanyPartnerships
                    .Where(p => p.CompanyId == companyId && p.PartnershipLevel == level)
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .OrderBy(p => p.PriorityOrder)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} {Level} partnerships for company: {CompanyId}", 
                    partnerships.Count, level, companyId);
                return partnerships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partnerships by level: {CompanyId}, {Level}", companyId, level);
                throw;
            }
        }

        /// <summary>
        /// Get specific partnership between two companies
        /// </summary>
        public async Task<CompanyPartnership> GetPartnershipAsync(int companyId, int partnerCompanyId)
        {
            try
            {
                if (companyId <= 0 || partnerCompanyId <= 0)
                    throw new ArgumentException("Company IDs must be greater than 0");

                var partnership = await _context.CompanyPartnerships
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.PartnerCompanyId == partnerCompanyId);

                _logger.LogInformation("Retrieved partnership: {CompanyId} -> {PartnerCompanyId}", companyId, partnerCompanyId);
                return partnership;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partnership: {CompanyId}, {PartnerCompanyId}", companyId, partnerCompanyId);
                throw;
            }
        }

        /// <summary>
        /// Update partnership priority
        /// </summary>
        public async Task<bool> UpdatePriorityAsync(int partnershipId, int priority)
        {
            try
            {
                if (partnershipId <= 0)
                    throw new ArgumentException("Partnership ID must be greater than 0");

                var partnership = await _context.CompanyPartnerships.FirstOrDefaultAsync(p => p.PartnershipId == partnershipId);
                if (partnership == null)
                    throw new KeyNotFoundException($"Partnership not found with ID: {partnershipId}");

                partnership.PriorityOrder = priority;
                partnership.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated partnership {PartnershipId} priority to: {Priority}", partnershipId, priority);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partnership priority: {PartnershipId}", partnershipId);
                throw;
            }
        }

        /// <summary>
        /// Update partnership commission rate
        /// </summary>
        public async Task<bool> UpdateCommissionRateAsync(int partnershipId, decimal rate)
        {
            try
            {
                if (partnershipId <= 0)
                    throw new ArgumentException("Partnership ID must be greater than 0");

                if (rate < 0 || rate > 100)
                    throw new ArgumentException("Commission rate must be between 0 and 100");

                var partnership = await _context.CompanyPartnerships.FirstOrDefaultAsync(p => p.PartnershipId == partnershipId);
                if (partnership == null)
                    throw new KeyNotFoundException($"Partnership not found with ID: {partnershipId}");

                partnership.CommissionRate = rate;
                partnership.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated partnership {PartnershipId} commission rate to: {Rate}%", partnershipId, rate);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating commission rate: {PartnershipId}", partnershipId);
                throw;
            }
        }

        /// <summary>
        /// Increment transferred orders count
        /// </summary>
        public async Task<bool> IncrementTransferredOrdersAsync(int partnershipId)
        {
            try
            {
                if (partnershipId <= 0)
                    throw new ArgumentException("Partnership ID must be greater than 0");

                var partnership = await _context.CompanyPartnerships.FirstOrDefaultAsync(p => p.PartnershipId == partnershipId);
                if (partnership == null)
                    throw new KeyNotFoundException($"Partnership not found with ID: {partnershipId}");

                partnership.TotalTransferredOrders = (partnership.TotalTransferredOrders ?? 0) + 1;
                partnership.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incremented transferred orders for partnership: {PartnershipId}", partnershipId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing transferred orders: {PartnershipId}", partnershipId);
                throw;
            }
        }

        /// <summary>
        /// Get preferred partners
        /// </summary>
        public async Task<IEnumerable<CompanyPartnership>> GetPreferredPartnersAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0");

                var partnerships = await _context.CompanyPartnerships
                    .Where(p => p.CompanyId == companyId && p.PartnershipLevel == "preferred" && p.IsActive == true)
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .OrderBy(p => p.PriorityOrder)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} preferred partners for company: {CompanyId}", partnerships.Count, companyId);
                return partnerships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferred partners: {CompanyId}", companyId);
                throw;
            }
        }
    }
}
