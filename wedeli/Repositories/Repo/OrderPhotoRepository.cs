using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Infrastructure;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Order photo repository for managing order photos
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
        /// Get photos by order ID
        /// </summary>
        public async Task<IEnumerable<OrderPhoto>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

                var photos = await _context.OrderPhotos
                    .Where(p => p.OrderId == orderId)
                    .OrderByDescending(p => p.UploadedAt)
                    .ToListAsync();

                return photos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photos for order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Get photos by type (e.g., "pickup", "delivery")
        /// </summary>
        public async Task<IEnumerable<OrderPhoto>> GetByTypeAsync(int orderId, string photoType)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

                if (string.IsNullOrEmpty(photoType))
                    throw new ArgumentNullException(nameof(photoType));

                var photos = await _context.OrderPhotos
                    .Where(p => p.OrderId == orderId && p.PhotoType == photoType)
                    .OrderByDescending(p => p.UploadedAt)
                    .ToListAsync();

                return photos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photos by type - OrderId: {OrderId}, Type: {PhotoType}", orderId, photoType);
                throw;
            }
        }

        /// <summary>
        /// Upload photo for order
        /// </summary>
        public async Task<OrderPhoto> UploadPhotoAsync(int orderId, string photoType, string photoUrl, string fileName, int? uploadedBy = null)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

                if (string.IsNullOrEmpty(photoType))
                    throw new ArgumentNullException(nameof(photoType));

                if (string.IsNullOrEmpty(photoUrl))
                    throw new ArgumentNullException(nameof(photoUrl));

                var orderPhoto = new OrderPhoto
                {
                    OrderId = orderId,
                    PhotoType = photoType,
                    PhotoUrl = photoUrl,
                    FileName = fileName,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow
                };

                _context.OrderPhotos.Add(orderPhoto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Uploaded photo for order - OrderId: {OrderId}, Type: {PhotoType}, FileName: {FileName}", 
                    orderId, photoType, fileName);
                
                return orderPhoto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Delete photo by ID
        /// </summary>
        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            try
            {
                if (photoId <= 0)
                    throw new ArgumentException("Photo ID must be greater than 0", nameof(photoId));

                var photo = await _context.OrderPhotos.FirstOrDefaultAsync(p => p.PhotoId == photoId);
                
                if (photo == null)
                    throw new KeyNotFoundException($"Photo {photoId} not found");

                _context.OrderPhotos.Remove(photo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted photo: {PhotoId}", photoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// Get photo by ID
        /// </summary>
        public async Task<OrderPhoto> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Photo ID must be greater than 0", nameof(id));

                var photo = await _context.OrderPhotos.FirstOrDefaultAsync(p => p.PhotoId == id);
                
                if (photo == null)
                    throw new KeyNotFoundException($"Photo {id} not found");

                return photo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photo: {PhotoId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all photos
        /// </summary>
        public async Task<IEnumerable<OrderPhoto>> GetAllAsync()
        {
            try
            {
                var photos = await _context.OrderPhotos
                    .OrderByDescending(p => p.UploadedAt)
                    .ToListAsync();

                return photos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all photos");
                throw;
            }
        }

        /// <summary>
        /// Add new photo
        /// </summary>
        public async Task<OrderPhoto> AddAsync(OrderPhoto entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _context.OrderPhotos.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new photo: {PhotoId}", entity.PhotoId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding photo");
                throw;
            }
        }

        /// <summary>
        /// Update photo
        /// </summary>
        public async Task<OrderPhoto> UpdateAsync(OrderPhoto entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingPhoto = await _context.OrderPhotos
                    .FirstOrDefaultAsync(p => p.PhotoId == entity.PhotoId);

                if (existingPhoto == null)
                    throw new KeyNotFoundException($"Photo {entity.PhotoId} not found");

                existingPhoto.PhotoType = entity.PhotoType;
                existingPhoto.PhotoUrl = entity.PhotoUrl;
                existingPhoto.FileName = entity.FileName;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated photo: {PhotoId}", entity.PhotoId);
                return existingPhoto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating photo: {PhotoId}", entity?.PhotoId);
                throw;
            }
        }

        /// <summary>
        /// Delete photo by ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Photo ID must be greater than 0", nameof(id));

                var photo = await _context.OrderPhotos.FirstOrDefaultAsync(p => p.PhotoId == id);
                
                if (photo == null)
                    throw new KeyNotFoundException($"Photo {id} not found");

                _context.OrderPhotos.Remove(photo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted photo: {PhotoId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo: {PhotoId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if photo exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.OrderPhotos.AnyAsync(p => p.PhotoId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if photo exists: {PhotoId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count total photos
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.OrderPhotos.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting photos");
                throw;
            }
        }
    }
}
