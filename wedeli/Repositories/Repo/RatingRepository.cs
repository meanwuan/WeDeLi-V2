using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class RatingRepository : IRatingRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RatingRepository> _logger;

        public RatingRepository(AppDbContext context, ILogger<RatingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Rating> GetByIdAsync(int ratingId)
        {
            try
            {
                return await _context.Ratings
                    .Include(r => r.Order)
                    .Include(r => r.Driver)
                    .FirstOrDefaultAsync(r => r.RatingId == ratingId) ?? new Rating();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rating: {RatingId}", ratingId);
                throw;
            }
        }

        public async Task<IEnumerable<Rating>> GetAllAsync()
        {
            try
            {
                return await _context.Ratings
                    .Include(r => r.Order)
                    .Include(r => r.Driver)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all ratings");
                throw;
            }
        }

        public async Task<Rating> AddAsync(Rating entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _context.Ratings.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rating");
                throw;
            }
        }

        public void Update(Rating entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                _context.Ratings.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating");
                throw;
            }
        }

        public void Delete(Rating entity)
        {
            try
            {
                _context.Ratings.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating");
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

        public async Task<IEnumerable<Rating>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.Ratings
                    .Where(r => r.OrderId == orderId)
                    .Include(r => r.Order)
                    .Include(r => r.Driver)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings by order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<Rating>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _context.Ratings
                    .Where(r => r.CustomerId == customerId)
                    .Include(r => r.Order)
                    .Include(r => r.Driver)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings by customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<Rating>> GetByDriverIdAsync(int driverId)
        {
            try
            {
                return await _context.Ratings
                    .Where(r => r.DriverId == driverId)
                    .Include(r => r.Order)
                    .Include(r => r.Driver)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings by driver: {DriverId}", driverId);
                throw;
            }
        }

        public async Task<decimal> GetAverageRatingAsync(int driverId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Where(r => r.DriverId == driverId)
                    .ToListAsync();

                if (!ratings.Any())
                    return 0m;

                return (decimal)ratings.Average(r => r.RatingScore ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average rating for driver: {DriverId}", driverId);
                throw;
            }
        }

        public async Task<Rating> UpdateAsync(Rating entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var rating = await GetByIdAsync(id);
                if (rating == null)
                    return false;

                Delete(rating);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating: {RatingId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Ratings.AnyAsync(r => r.RatingId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rating existence: {RatingId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Ratings.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting ratings");
                throw;
            }
        }
    }
}
