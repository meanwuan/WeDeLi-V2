using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    public class TransportCompanyRepository : GenericRepository<TransportCompany>, ITransportCompanyRepository
    {
        public TransportCompanyRepository(AppDbContext context, ILogger<TransportCompanyRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<TransportCompany?> GetCompanyByNameAsync(string companyName)
        {
            try
            {
                return await _dbSet
                    .FirstOrDefaultAsync(c => c.CompanyName == companyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting company by name: {companyName}");
                throw;
            }
        }

        public async Task<TransportCompany?> GetCompanyWithDetailsAsync(int companyId)
        {
            try
            {
                return await _dbSet
                    .Include(c => c.Vehicles)
                    .Include(c => c.Drivers)
                    .Include(c => c.Routes)
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting company with details: {companyId}");
                throw;
            }
        }

        public async Task<List<TransportCompany>> GetActiveCompaniesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active companies");
                throw;
            }
        }

        public async Task<List<TransportCompany>> GetCompaniesByLocationAsync(string province)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.Address == province && c.IsActive == true)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting companies by location: {province}");
                throw;
            }
        }

        public async Task<List<TransportCompany>> SearchCompaniesAsync(string keyword)
        {
            try
            {
                var lowerKeyword = keyword.ToLower();

                return await _dbSet
                    .Where(c =>
                        c.CompanyName!.ToLower().Contains(lowerKeyword) ||
                        c.Phone!.Contains(keyword))
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching companies with keyword: {keyword}");
                throw;
            }
        }

        public async Task<CompanyStatistics> GetCompanyStatisticsAsync(int companyId)
        {
            try
            {
                var company = await _dbSet
                    .Include(c => c.Vehicles)
                    .Include(c => c.Drivers)
                    .Include(c => c.Routes)
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                if (company == null)
                {
                    throw new KeyNotFoundException($"Company not found: {companyId}");
                }

                // Truy vấn các đơn hàng liên quan một cách riêng biệt thông qua Route
                var orders = await _context.Orders
                    .Where(o => o.Route != null && o.Route.CompanyId == companyId)
                    .ToListAsync();

                var statistics = new CompanyStatistics
                {
                    TotalVehicles = company.Vehicles?.Count ?? 0,
                    ActiveVehicles = company.Vehicles?.Count(v => v.CurrentStatus == "available") ?? 0,
                    TotalDrivers = company.Drivers?.Count ?? 0,
                    ActiveDrivers = company.Drivers?.Count(d => d.IsActive == true) ?? 0,
                    TotalOrders = orders.Count,
                    PendingOrders = orders.Count(o => o.OrderStatus == "pending_pickup"),
                    CompletedOrders = orders.Count(o => o.OrderStatus == "delivered"),
                    TotalRevenue = orders.Where(o => o.OrderStatus == "delivered").Sum(o => o.ShippingFee),
                    TotalRoutes = company.Routes?.Count ?? 0
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting statistics for company: {companyId}");
                throw;
            }
        }

        public async Task<int> GetVehiclesCountAsync(int companyId)
        {
            try
            {
                return await _context.Vehicles
                    .Where(v => v.CompanyId == companyId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicles count for company: {companyId}");
                throw;
            }
        }

        public async Task<int> GetDriversCountAsync(int companyId)
        {
            try
            {
                return await _context.Drivers
                    .Where(d => d.CompanyId == companyId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting drivers count for company: {companyId}");
                throw;
            }
        }

        public async Task<int> GetOrdersCountAsync(int companyId)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.Route != null && o.Route.CompanyId == companyId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting orders count for company: {companyId}");
                throw;
            }
        }

        //public async Task<bool> CompanyCodeExistsAsync(string companyCode)
        //{
        //    try
        //    {
        //        return await _dbSet
        //            .AnyAsync(c => c.CompanyCode == companyCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error checking if company code exists: {companyCode}");
        //        throw;
        //    }
        //}
    }
}