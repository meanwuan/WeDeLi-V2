using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    /// <summary>
    /// Repository interface for managing order photos
    /// </summary>
    public interface IOrderPhotoRepository
    {
        /// <summary>
        /// Add a new photo to an order
        /// </summary>
        Task<OrderPhoto> AddPhotoAsync(OrderPhoto photo);

        /// <summary>
        /// Get photo by ID
        /// </summary>
        Task<OrderPhoto?> GetPhotoByIdAsync(int photoId);

        /// <summary>
        /// Get all photos for a specific order
        /// </summary>
        Task<List<OrderPhoto>> GetPhotosByOrderIdAsync(int orderId);

        /// <summary>
        /// Get photos by order ID and photo type
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="photoType">Photo type (before_delivery, after_delivery, etc.)</param>
        Task<List<OrderPhoto>> GetPhotosByTypeAsync(int orderId, string photoType);

        /// <summary>
        /// Get photos uploaded by a specific user
        /// </summary>
        Task<List<OrderPhoto>> GetPhotosByUploaderAsync(int uploadedBy);

        /// <summary>
        /// Delete a photo (soft delete by removing record)
        /// </summary>
        Task<bool> DeletePhotoAsync(int photoId);

        /// <summary>
        /// Delete all photos for an order
        /// </summary>
        Task<bool> DeletePhotosByOrderIdAsync(int orderId);

        /// <summary>
        /// Check if photo exists
        /// </summary>
        Task<bool> PhotoExistsAsync(int photoId);

        /// <summary>
        /// Get total file size for an order's photos (in KB)
        /// </summary>
        Task<int> GetTotalFileSizeForOrderAsync(int orderId);

        /// <summary>
        /// Get count of photos by type for an order
        /// </summary>
        Task<int> GetPhotoCountByTypeAsync(int orderId, string photoType);
    }
}