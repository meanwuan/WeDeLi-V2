using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Repository implementation for order photo operations
    /// </summary>
    public class OrderPhotoRepository : IOrderPhotoRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderPhotoRepository> _logger;

        public OrderPhotoRepository(AppDbContext context, ILogger<OrderPhotoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Add a new photo to database
        /// </summary>
        public async Task<OrderPhoto> AddPhotoAsync(OrderPhoto photo)
        {
            try
            {
                photo.UploadedAt = DateTime.Now;
                await _context.OrderPhotos.AddAsync(photo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Photo added successfully for Order ID: {photo.OrderId}");
                return photo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding photo for Order ID: {photo.OrderId}");
                throw;
            }
        }

        /// <summary>
        /// Get photo by ID
        /// </summary>
        public async Task<OrderPhoto?> GetPhotoByIdAsync(int photoId)
        {
            try
            {
                return await _context.OrderPhotos
                    .Include(p => p.Order)
                    .Include(p => p.UploadedByNavigation)
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting photo by ID: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// Get all photos for a specific order
        /// </summary>
        public async Task<List<OrderPhoto>> GetPhotosByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.OrderPhotos
                    .Where(p => p.OrderId == orderId)
                    .Include(p => p.UploadedByNavigation)
                    .OrderBy(p => p.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting photos for Order ID: {orderId}");
                throw;
            }
        }

        /// <summary>
        /// Get photos by order ID and photo type
        /// </summary>
        public async Task<List<OrderPhoto>> GetPhotosByTypeAsync(int orderId, string photoType)
        {
            try
            {
                return await _context.OrderPhotos
                    .Where(p => p.OrderId == orderId && p.PhotoType == photoType)
                    .Include(p => p.UploadedByNavigation)
                    .OrderBy(p => p.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting photos by type for Order ID: {orderId}, Type: {photoType}");
                throw;
            }
        }

        /// <summary>
        /// Get photos uploaded by a specific user
        /// </summary>
        public async Task<List<OrderPhoto>> GetPhotosByUploaderAsync(int uploadedBy)
        {
            try
            {
                return await _context.OrderPhotos
                    .Where(p => p.UploadedBy == uploadedBy)
                    .Include(p => p.Order)
                    .OrderByDescending(p => p.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting photos by uploader: {uploadedBy}");
                throw;
            }
        }

        /// <summary>
        /// Delete a photo from database
        /// </summary>
        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            try
            {
                var photo = await _context.OrderPhotos.FindAsync(photoId);
                if (photo == null)
                {
                    _logger.LogWarning($"Photo not found for deletion: {photoId}");
                    return false;
                }

                _context.OrderPhotos.Remove(photo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Photo deleted successfully: {photoId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting photo: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// Delete all photos for an order
        /// </summary>
        public async Task<bool> DeletePhotosByOrderIdAsync(int orderId)
        {
            try
            {
                var photos = await _context.OrderPhotos
                    .Where(p => p.OrderId == orderId)
                    .ToListAsync();

                if (!photos.Any())
                {
                    _logger.LogWarning($"No photos found for Order ID: {orderId}");
                    return false;
                }

                _context.OrderPhotos.RemoveRange(photos);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"All photos deleted for Order ID: {orderId}. Count: {photos.Count}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting photos for Order ID: {orderId}");
                throw;
            }
        }

        /// <summary>
        /// Check if photo exists
        /// </summary>
        public async Task<bool> PhotoExistsAsync(int photoId)
        {
            try
            {
                return await _context.OrderPhotos.AnyAsync(p => p.PhotoId == photoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking photo existence: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// Get total file size for an order's photos (in KB)
        /// </summary>
        public async Task<int> GetTotalFileSizeForOrderAsync(int orderId)
        {
            try
            {
                var totalSize = await _context.OrderPhotos
                    .Where(p => p.OrderId == orderId && p.FileSizeKb.HasValue)
                    .SumAsync(p => p.FileSizeKb ?? 0);

                return totalSize;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating total file size for Order ID: {orderId}");
                throw;
            }
        }

        /// <summary>
        /// Get count of photos by type for an order
        /// </summary>
        public async Task<int> GetPhotoCountByTypeAsync(int orderId, string photoType)
        {
            try
            {
                return await _context.OrderPhotos
                    .CountAsync(p => p.OrderId == orderId && p.PhotoType == photoType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting photos for Order ID: {orderId}, Type: {photoType}");
                throw;
            }
        }
    }
}