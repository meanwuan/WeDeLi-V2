using CloudinaryDotNet.Actions;

namespace wedeli.service.Interface
{
    /// <summary>
    /// Service interface for Cloudinary image management operations
    /// </summary>
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload image to Cloudinary
        /// </summary>
        /// <param name="file">Image file stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="folder">Cloudinary folder (e.g., "orders", "profiles")</param>
        /// <returns>Upload result with URL and public ID</returns>
        Task<ImageUploadResult> UploadImageAsync(Stream file, string fileName, string folder = "orders");

        /// <summary>
        /// Upload image from file path
        /// </summary>
        Task<ImageUploadResult> UploadImageAsync(string filePath, string folder = "orders");

        /// <summary>
        /// Delete image from Cloudinary by public ID
        /// </summary>
        /// <param name="publicId">Cloudinary public ID (e.g., "orders/abc123")</param>
        /// <returns>Deletion result</returns>
        Task<DeletionResult> DeleteImageAsync(string publicId);

        /// <summary>
        /// Delete multiple images at once
        /// </summary>
        Task<DeletionResult> DeleteImagesAsync(List<string> publicIds);

        /// <summary>
        /// Get optimized image URL with transformations
        /// </summary>
        /// <param name="publicId">Cloudinary public ID</param>
        /// <param name="width">Target width (optional)</param>
        /// <param name="height">Target height (optional)</param>
        /// <param name="quality">Image quality (1-100)</param>
        /// <returns>Transformed image URL</returns>
        string GetOptimizedImageUrl(string publicId, int? width = null, int? height = null, int quality = 80);

        /// <summary>
        /// Extract public ID from Cloudinary URL
        /// </summary>
        string ExtractPublicIdFromUrl(string imageUrl);
    }

    /// <summary>
    /// Result model for image upload operations
    /// </summary>
    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? PublicId { get; set; }
        public long FileSizeBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Format { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Result model for image deletion operations
    /// </summary>
    public class DeletionResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}