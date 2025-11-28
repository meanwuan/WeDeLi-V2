using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    public class DriverRepository : IDriverRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DriverRepository> _logger;

        public DriverRepository(AppDbContext context, ILogger<DriverRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== CRUD Operations =====

        public async Task<Driver?> GetDriverByIdAsync(int driverId)
        {
            try
            {
                return await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .FirstOrDefaultAsync(d => d.DriverId == driverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver by ID: {driverId}");
                throw;
            }
        }

        public async Task<Driver?> GetDriverByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .FirstOrDefaultAsync(d => d.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver by user ID: {userId}");
                throw;
            }
        }

        public async Task<(List<Driver> Drivers, int TotalCount)> GetDriversAsync(DriverFilterDto filter)
        {
            try
            {
                var query = _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .AsQueryable();

                if (filter.CompanyId.HasValue)
                    query = query.Where(d => d.CompanyId == filter.CompanyId.Value);

                if (filter.IsActive.HasValue)
                    query = query.Where(d => d.IsActive == filter.IsActive.Value);

                if (filter.MinRating.HasValue)
                    query = query.Where(d => d.Rating >= filter.MinRating.Value);

                if (filter.LicenseExpiringSoon == true)
                {
                    var thresholdDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
                    query = query.Where(d => d.LicenseExpiry.HasValue && d.LicenseExpiry.Value <= thresholdDate);
                }

                var totalCount = await query.CountAsync();

                var drivers = await query
                    .OrderBy(d => d.User.FullName)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (drivers, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drivers with filters");
                throw;
            }
        }

        public async Task<List<Driver>> GetDriversByCompanyAsync(int companyId)
        {
            try
            {
                return await _context.Drivers
                    .Include(d => d.User)
                    .Where(d => d.CompanyId == companyId && d.IsActive == true)
                    .OrderBy(d => d.User.FullName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting drivers for company: {companyId}");
                throw;
            }
        }

        public async Task<Driver> CreateDriverAsync(Driver driver)
        {
            try
            {
                driver.CreatedAt = DateTime.Now;
                driver.TotalTrips = 0;
                driver.SuccessRate = 100.00M;
                driver.Rating = 5.00M;
                driver.IsActive = true;

                await _context.Drivers.AddAsync(driver);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Driver created: {driver.DriverId}");
                return driver;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                throw;
            }
        }

        public async Task<Driver> UpdateDriverAsync(Driver driver)
        {
            try
            {
                _context.Drivers.Update(driver);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Driver updated: {driver.DriverId}");
                return driver;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating driver: {driver.DriverId}");
                throw;
            }
        }

        public async Task<bool> DeleteDriverAsync(int driverId)
        {
            try
            {
                var driver = await _context.Drivers.FindAsync(driverId);
                if (driver == null)
                    return false;

                driver.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Driver deleted (soft): {driverId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting driver: {driverId}");
                throw;
            }
        }

        // ===== Performance & Statistics =====

        public async Task<DriverPerformanceDto> GetDriverPerformanceAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Orders)
                    .Include(d => d.Trips)
                    .FirstOrDefaultAsync(d => d.DriverId == driverId);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver not found: {driverId}");

                var ordersQuery = _context.Orders.Where(o => o.DriverId == driverId);
                var tripsQuery = _context.Trips.Where(t => t.DriverId == driverId);

                if (startDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value);
                    tripsQuery = tripsQuery.Where(t => t.TripDate >= DateOnly.FromDateTime(startDate.Value));
                }

                if (endDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate.Value);
                    tripsQuery = tripsQuery.Where(t => t.TripDate <= DateOnly.FromDateTime(endDate.Value));
                }

                var ordersList = await ordersQuery.ToListAsync();
                var tripsList = await tripsQuery.ToListAsync();

                var codTransactions = await _context.CodTransactions
                    .Where(c => c.CollectedByDriver == driverId && c.CreatedAt.HasValue)
                    .ToListAsync();

                if (startDate.HasValue)
                    codTransactions = codTransactions.Where(c => c.CreatedAt >= startDate.Value).ToList();
                if (endDate.HasValue)
                    codTransactions = codTransactions.Where(c => c.CreatedAt <= endDate.Value).ToList();

                var ratings = await _context.Ratings
                    .Where(r => r.DriverId == driverId)
                    .ToListAsync();

                return new DriverPerformanceDto
                {
                    DriverId = driverId,
                    FullName = driver.User.FullName,
                    TotalTrips = tripsList.Count,
                    CompletedTrips = tripsList.Count(t => t.TripStatus == "completed"),
                    CancelledTrips = tripsList.Count(t => t.TripStatus == "cancelled"),
                    TotalOrdersAssigned = ordersList.Count,
                    OrdersDelivered = ordersList.Count(o => o.OrderStatus == "delivered"),
                    OrdersReturned = ordersList.Count(o => o.OrderStatus == "returned"),
                    OrdersCancelled = ordersList.Count(o => o.OrderStatus == "cancelled"),
                    SuccessRate = (decimal)driver.SuccessRate,
                    OnTimeDeliveryRate = CalculateOnTimeRate(ordersList),
                    AverageRating = ratings.Any() ? (decimal)ratings.Average(r => r.RatingScore ?? 0) : 5.00M,
                    TotalRatings = ratings.Count,
                    TotalCodCollected = codTransactions.Where(c => c.CollectionStatus == "collected").Sum(c => c.CodAmount),
                    TotalCodSubmitted = codTransactions.Sum(c => c.SubmittedAmount ?? 0),
                    PendingCodAmount = codTransactions.Where(c => c.CollectionStatus == "collected" && c.SubmittedToCompany != true).Sum(c => c.CodAmount),
                    LastDeliveryDate = ordersList.Where(o => o.DeliveredAt.HasValue).Max(o => o.DeliveredAt),
                    DaysActive = driver.CreatedAt.HasValue ? (DateTime.Now - driver.CreatedAt.Value).Days : 0,
                    AverageDeliveriesPerDay = CalculateAvgDeliveriesPerDay(ordersList, driver.CreatedAt)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver performance: {driverId}");
                throw;
            }
        }

        public async Task<List<Order>> GetDriverOrdersAsync(int driverId, string? status = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Customer)
                    .Where(o => o.DriverId == driverId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.OrderStatus == status);

                return await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver orders: {driverId}");
                throw;
            }
        }

        public async Task<List<Order>> GetDriverOrdersByDateAsync(int driverId, DateTime date)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Where(o => o.DriverId == driverId && o.CreatedAt.HasValue && o.CreatedAt.Value.Date == date.Date)
                    .OrderBy(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver orders by date: {driverId}, {date}");
                throw;
            }
        }

        public async Task<DriverDailySummaryDto> GetDriverDailySummaryAsync(int driverId, DateTime date)
        {
            try
            {
                var driver = await GetDriverByIdAsync(driverId);
                if (driver == null)
                    throw new KeyNotFoundException($"Driver not found: {driverId}");

                var orders = await GetDriverOrdersByDateAsync(driverId, date);

                var codTransactions = await _context.CodTransactions
                    .Where(c => c.CollectedByDriver == driverId && c.CollectedAt.HasValue && c.CollectedAt.Value.Date == date.Date)
                    .ToListAsync();

                var dateOnly = DateOnly.FromDateTime(date);
                var trips = await _context.Trips
                    .Where(t => t.DriverId == driverId && t.TripDate == dateOnly)
                    .ToListAsync();

                return new DriverDailySummaryDto
                {
                    DriverId = driverId,
                    FullName = driver.User.FullName,
                    Date = date,
                    OrdersAssigned = orders.Count,
                    OrdersDelivered = orders.Count(o => o.OrderStatus == "delivered"),
                    OrdersPending = orders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                    TotalCodCollected = codTransactions.Where(c => c.CollectionStatus == "collected").Sum(c => c.CodAmount),
                    TotalDistanceKm = 0,
                    TripCount = trips.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver daily summary: {driverId}, {date}");
                throw;
            }
        }

        public async Task UpdateDriverStatisticsAsync(int driverId, bool isSuccess)
        {
            try
            {
                var driver = await _context.Drivers.FindAsync(driverId);
                if (driver == null)
                    return;

                driver.TotalTrips++;

                var totalOrders = await _context.Orders.CountAsync(o => o.DriverId == driverId);
                var successfulOrders = await _context.Orders.CountAsync(o => o.DriverId == driverId && o.OrderStatus == "delivered");

                if (totalOrders > 0)
                {
                    driver.SuccessRate = Math.Round((decimal)successfulOrders / totalOrders * 100, 2);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Driver statistics updated: {driverId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating driver statistics: {driverId}");
                throw;
            }
        }

        // ===== Availability & Status =====

        public async Task<List<Driver>> GetAvailableDriversAsync(int? companyId = null)
        {
            try
            {
                var query = _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .Where(d => d.IsActive == true);

                if (companyId.HasValue)
                    query = query.Where(d => d.CompanyId == companyId.Value);

                var driversOnTrip = await _context.Trips
                    .Where(t => t.TripStatus == "in_progress")
                    .Select(t => t.DriverId)
                    .ToListAsync();

                query = query.Where(d => !driversOnTrip.Contains(d.DriverId));

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available drivers");
                throw;
            }
        }

        public async Task<bool> IsDriverAvailableAsync(int driverId)
        {
            try
            {
                var driver = await _context.Drivers.FindAsync(driverId);
                if (driver == null || driver.IsActive != true)
                    return false;

                var onActiveTrip = await _context.Trips
                    .AnyAsync(t => t.DriverId == driverId && t.TripStatus == "in_progress");

                return !onActiveTrip;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking driver availability: {driverId}");
                throw;
            }
        }

        public async Task<List<Driver>> GetDriversWithExpiringLicenseAsync(int daysThreshold = 30)
        {
            try
            {
                var thresholdDate = DateOnly.FromDateTime(DateTime.Now.AddDays(daysThreshold));
                return await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .Where(d => d.IsActive == true && d.LicenseExpiry.HasValue && d.LicenseExpiry.Value <= thresholdDate)
                    .OrderBy(d => d.LicenseExpiry)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drivers with expiring license");
                throw;
            }
        }

        // ===== Ratings =====

        public async Task UpdateDriverRatingAsync(int driverId, int newRatingScore)
        {
            try
            {
                var driver = await _context.Drivers.FindAsync(driverId);
                if (driver == null)
                    return;

                var averageRating = await GetDriverAverageRatingAsync(driverId);
                driver.Rating = averageRating;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Driver rating updated: {driverId} -> {averageRating}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating driver rating: {driverId}");
                throw;
            }
        }

        public async Task<decimal> GetDriverAverageRatingAsync(int driverId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Where(r => r.DriverId == driverId && r.RatingScore.HasValue)
                    .ToListAsync();

                return ratings.Any() ? (decimal)ratings.Average(r => r.RatingScore!.Value) : 5.00M;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver average rating: {driverId}");
                throw;
            }
        }

        // ===== COD Management =====

        public async Task<decimal> GetDriverPendingCodAsync(int driverId)
        {
            try
            {
                return await _context.CodTransactions
                    .Where(c => c.CollectedByDriver == driverId
                           && c.CollectionStatus == "collected"
                           && c.SubmittedToCompany != true)
                    .SumAsync(c => c.CodAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver pending COD: {driverId}");
                throw;
            }
        }

        public async Task<List<CodTransaction>> GetDriverCodTransactionsAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.CodTransactions
                    .Include(c => c.Order)
                    .Where(c => c.CollectedByDriver == driverId);

                if (startDate.HasValue)
                    query = query.Where(c => c.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(c => c.CreatedAt <= endDate.Value);

                return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver COD transactions: {driverId}");
                throw;
            }
        }

        // ===== Helper Methods =====

        public async Task<bool> DriverExistsAsync(int driverId)
        {
            return await _context.Drivers.AnyAsync(d => d.DriverId == driverId);
        }

        public async Task<bool> UserIsDriverAsync(int userId)
        {
            return await _context.Drivers.AnyAsync(d => d.UserId == userId);
        }

        public async Task<int> GetDriverCountByCompanyAsync(int companyId)
        {
            return await _context.Drivers.CountAsync(d => d.CompanyId == companyId && d.IsActive == true);
        }

        // ===== Private Helper Methods =====

        private decimal CalculateOnTimeRate(List<Order> orders)
        {
            var deliveredOrders = orders.Where(o => o.DeliveredAt.HasValue).ToList();
            if (!deliveredOrders.Any())
                return 100M;

            var onTimeCount = deliveredOrders.Count;
            return Math.Round((decimal)onTimeCount / deliveredOrders.Count * 100, 2);
        }

        private decimal CalculateAvgDeliveriesPerDay(List<Order> orders, DateTime? startDate)
        {
            var deliveredOrders = orders.Where(o => o.OrderStatus == "delivered").Count();
            var daysActive = Math.Max(1, (DateTime.Now - (startDate ?? DateTime.Now)).Days);
            return Math.Round((decimal)deliveredOrders / daysActive, 2);
        }
    }
}