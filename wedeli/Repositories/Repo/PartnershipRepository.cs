using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    public class PartnershipRepository : IPartnershipRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PartnershipRepository> _logger;

        public PartnershipRepository(AppDbContext context, ILogger<PartnershipRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== Partnership CRUD =====

        public async Task<CompanyPartnership?> GetPartnershipByIdAsync(int partnershipId)
        {
            try
            {
                return await _context.CompanyPartnerships
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .Include(p => p.CreatedByNavigation)
                    .FirstOrDefaultAsync(p => p.PartnershipId == partnershipId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting partnership by ID: {partnershipId}");
                throw;
            }
        }

        public async Task<CompanyPartnership?> GetPartnershipAsync(int companyId, int partnerCompanyId)
        {
            try
            {
                return await _context.CompanyPartnerships
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .FirstOrDefaultAsync(p =>
                        p.CompanyId == companyId &&
                        p.PartnerCompanyId == partnerCompanyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting partnership: {companyId} -> {partnerCompanyId}");
                throw;
            }
        }

        public async Task<(List<CompanyPartnership> Partnerships, int TotalCount)> GetPartnershipsAsync(PartnershipFilterDto filter)
        {
            try
            {
                var query = _context.CompanyPartnerships
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .Include(p => p.CreatedByNavigation)
                    .AsQueryable();

                if (filter.CompanyId.HasValue)
                    query = query.Where(p => p.CompanyId == filter.CompanyId.Value);

                if (filter.PartnerCompanyId.HasValue)
                    query = query.Where(p => p.PartnerCompanyId == filter.PartnerCompanyId.Value);

                if (!string.IsNullOrEmpty(filter.PartnershipLevel))
                    query = query.Where(p => p.PartnershipLevel == filter.PartnershipLevel);

                if (filter.IsActive.HasValue)
                    query = query.Where(p => p.IsActive == filter.IsActive.Value);

                var totalCount = await query.CountAsync();

                var partnerships = await query
                    .OrderBy(p => p.PriorityOrder)
                    .ThenByDescending(p => p.TotalTransferredOrders)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (partnerships, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partnerships with filters");
                throw;
            }
        }

        public async Task<List<CompanyPartnership>> GetCompanyPartnersAsync(int companyId)
        {
            try
            {
                return await _context.CompanyPartnerships
                    .Include(p => p.PartnerCompany)
                    .Where(p => p.CompanyId == companyId && p.IsActive == true)
                    .OrderBy(p => p.PriorityOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting partners for company: {companyId}");
                throw;
            }
        }

        public async Task<List<CompanyPartnership>> GetPreferredPartnersAsync(int companyId)
        {
            try
            {
                return await _context.CompanyPartnerships
                    .Include(p => p.PartnerCompany)
                    .Where(p => p.CompanyId == companyId &&
                           p.IsActive == true &&
                           p.PartnershipLevel == "preferred")
                    .OrderBy(p => p.PriorityOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting preferred partners for company: {companyId}");
                throw;
            }
        }

        public async Task<CompanyPartnership> CreatePartnershipAsync(CompanyPartnership partnership)
        {
            try
            {
                partnership.CreatedAt = DateTime.Now;
                partnership.UpdatedAt = DateTime.Now;
                partnership.TotalTransferredOrders = 0;
                partnership.IsActive = true;

                await _context.CompanyPartnerships.AddAsync(partnership);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Partnership created: {partnership.CompanyId} -> {partnership.PartnerCompanyId}");
                return partnership;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partnership");
                throw;
            }
        }

        public async Task<CompanyPartnership> UpdatePartnershipAsync(CompanyPartnership partnership)
        {
            try
            {
                partnership.UpdatedAt = DateTime.Now;
                _context.CompanyPartnerships.Update(partnership);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Partnership updated: {partnership.PartnershipId}");
                return partnership;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating partnership: {partnership.PartnershipId}");
                throw;
            }
        }

        public async Task<bool> DeletePartnershipAsync(int partnershipId)
        {
            try
            {
                var partnership = await _context.CompanyPartnerships.FindAsync(partnershipId);
                if (partnership == null)
                    return false;

                partnership.IsActive = false;
                partnership.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Partnership deleted (soft): {partnershipId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting partnership: {partnershipId}");
                throw;
            }
        }

        // ===== Partnership Statistics =====

        public async Task<PartnershipStatisticsDto> GetPartnershipStatisticsAsync(int partnershipId)
        {
            try
            {
                var partnership = await _context.CompanyPartnerships
                    .Include(p => p.Company)
                    .Include(p => p.PartnerCompany)
                    .FirstOrDefaultAsync(p => p.PartnershipId == partnershipId);
                if (partnership == null)
                    throw new KeyNotFoundException($"Partnership not found: {partnershipId}");

                // Lấy các giao dịch chuyển đơn hàng liên quan một cách thủ công
                // vì không có navigation property trực tiếp.
                var transfers = await _context.OrderTransfers
                    .Where(t => t.FromCompanyId == partnership.CompanyId &&
                                t.ToCompanyId == partnership.PartnerCompanyId)
                    .ToListAsync();

                var lastTransferDate = transfers.Any()
                    ? transfers.Max(t => t.TransferredAt)
                    : (DateTime?)null;

                var daysSinceLastTransfer = lastTransferDate.HasValue
                    ? (DateTime.Now - lastTransferDate.Value).Days
                    : 0;

                return new PartnershipStatisticsDto
                {
                    PartnershipId = partnershipId,
                    CompanyName = partnership.Company?.CompanyName ?? "",
                    PartnerCompanyName = partnership.PartnerCompany?.CompanyName ?? "",
                    TotalOrdersTransferred = transfers.Count,
                    OrdersCompleted = transfers.Count(t => t.TransferStatus == "completed"),
                    OrdersCancelled = transfers.Count(t => t.TransferStatus == "rejected"),
                    TotalCommissionPaid = transfers.Sum(t => t.CommissionPaid ?? 0),
                    AverageCommission = transfers.Any()
                        ? transfers.Average(t => t.CommissionPaid ?? 0)
                        : 0,
                    LastTransferDate = lastTransferDate,
                    DaysSinceLastTransfer = daysSinceLastTransfer
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting partnership statistics: {partnershipId}");
                throw;
            }
        }

        public async Task UpdatePartnershipStatisticsAsync(int partnershipId)
        {
            try
            {
                var partnership = await _context.CompanyPartnerships.FindAsync(partnershipId);
                if (partnership == null)
                    return;

                var totalTransfers = await _context.OrderTransfers
                    .CountAsync(t => t.FromCompanyId == partnership.CompanyId &&
                                t.ToCompanyId == partnership.PartnerCompanyId);

                partnership.TotalTransferredOrders = totalTransfers;
                partnership.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Partnership statistics updated: {partnershipId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating partnership statistics: {partnershipId}");
                throw;
            }
        }

        // ===== Helper Methods =====

        public async Task<bool> PartnershipExistsAsync(int companyId, int partnerCompanyId)
        {
            return await _context.CompanyPartnerships
                .AnyAsync(p => p.CompanyId == companyId &&
                          p.PartnerCompanyId == partnerCompanyId);
        }

        public async Task<CompanyPartnership?> GetBestPartnerForOrderAsync(int companyId, decimal orderWeight)
        {
            try
            {
                // Get all active partners ordered by priority
                var partners = await _context.CompanyPartnerships
                    .Include(p => p.PartnerCompany)
                    .Where(p => p.CompanyId == companyId && p.IsActive == true)
                    .OrderBy(p => p.PriorityOrder)
                    .ThenBy(p => p.PartnershipLevel == "preferred" ? 0 :
                            p.PartnershipLevel == "regular" ? 1 : 2)
                    .ToListAsync();

                // Check each partner's available vehicles
                foreach (var partner in partners)
                {
                    var hasAvailableVehicle = await _context.Vehicles
                        .AnyAsync(v => v.CompanyId == partner.PartnerCompanyId &&
                                 v.CurrentStatus == "available" &&
                                 v.MaxWeightKg >= orderWeight);

                    if (hasAvailableVehicle)
                        return partner;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding best partner for company: {companyId}");
                throw;
            }
        }
    }
}