using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    /// <summary>
    /// Service implementation for Cloudinary image operations
    /// Handles upload, delete, and URL optimization for order photos
    /// </summary>
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            // Load Cloudinary credentials from appsettings.json
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary credentials are not properly configured in appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Always use HTTPS
        }

        /// <summary>
        /// Upload image from stream to Cloudinary
        /// </summary>
        public async Task<Interface.ImageUploadResult> UploadImageAsync(Stream file, string fileName, string folder = "orders")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new Interface.ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = "File is empty or null"
                    };
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return new Interface.ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = "File size exceeds 10MB limit"
                    };
                }

                // Generate unique public ID
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var randomString = Guid.NewGuid().ToString("N").Substring(0, 8);
                var publicId = $"{folder}/{timestamp}_{randomString}";

                // Upload parameters
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, file),
                    PublicId = publicId,
                    Folder = folder,
                    Overwrite = false,
                    Transformation = new Transformation()
                        .Quality("auto") // Auto quality optimization
                        .FetchFormat("auto") // Auto format selection (WebP for modern browsers)
                };

                // Execute upload
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Image uploaded successfully: {uploadResult.SecureUrl}");

                    return new Interface.ImageUploadResult
                    {
                        Success = true,
                        ImageUrl = uploadResult.SecureUrl.ToString(),
                        PublicId = uploadResult.PublicId,
                        FileSizeBytes = uploadResult.Bytes,
                        Width = uploadResult.Width,
                        Height = uploadResult.Height,
                        Format = uploadResult.Format
                    };
                }
                else
                {
                    _logger.LogError($"Cloudinary upload failed: {uploadResult.Error?.Message}");
                    return new Interface.ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = uploadResult.Error?.Message ?? "Upload failed"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                return new Interface.ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Upload image from file path
        /// </summary>
        public async Task<Interface.ImageUploadResult> UploadImageAsync(string filePath, string folder = "orders")
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new Interface.ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = "File not found"
                    };
                }

                using var stream = File.OpenRead(filePath);
                var fileName = Path.GetFileName(filePath);
                return await UploadImageAsync(stream, fileName, folder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file from path: {filePath}");
                return new Interface.ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Delete image from Cloudinary by public ID
        /// </summary>
        public async Task<Interface.DeletionResult> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return new Interface.DeletionResult
                    {
                        Success = false,
                        Message = "Public ID is required"
                    };
                }

                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "ok")
                {
                    _logger.LogInformation($"Image deleted successfully: {publicId}");
                    return new Interface.DeletionResult
                    {
                        Success = true,
                        Message = "Image deleted successfully"
                    };
                }
                else
                {
                    _logger.LogWarning($"Failed to delete image: {publicId}. Result: {result.Result}");
                    return new Interface.DeletionResult
                    {
                        Success = false,
                        Message = result.Result ?? "Deletion failed"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {publicId}");
                return new Interface.DeletionResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Delete multiple images at once (batch delete)
        /// </summary>
        public async Task<Interface.DeletionResult> DeleteImagesAsync(List<string> publicIds)
        {
            try
            {
                if (publicIds == null || !publicIds.Any())
                {
                    return new Interface.DeletionResult
                    {
                        Success = false,
                        Message = "No public IDs provided"
                    };
                }

                int successCount = 0;
                int failCount = 0;

                foreach (var publicId in publicIds)
                {
                    var result = await DeleteImageAsync(publicId);
                    if (result.Success)
                        successCount++;
                    else
                        failCount++;
                }

                return new Interface.DeletionResult
                {
                    Success = failCount == 0,
                    Message = $"Deleted {successCount}/{publicIds.Count} images successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch deleting images");
                return new Interface.DeletionResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Get optimized image URL with transformations (resize, quality, format)
        /// </summary>
        public string GetOptimizedImageUrl(string publicId, int? width = null, int? height = null, int quality = 80)
        {
            try
            {
                var transformation = new Transformation()
                    .Quality($"auto:{quality}")
                    .FetchFormat("auto");

                if (width.HasValue)
                    transformation.Width(width.Value);

                if (height.HasValue)
                    transformation.Height(height.Value);

                return _cloudinary.Api.UrlImgUp
                    .Transform(transformation)
                    .BuildUrl(publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating optimized URL for: {publicId}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Extract public ID from full Cloudinary URL
        /// Example: https://res.cloudinary.com/demo/image/upload/v1234/orders/abc123.jpg -> orders/abc123
        /// </summary>
        public string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return string.Empty;

                // Cloudinary URL format: https://res.cloudinary.com/{cloud_name}/image/upload/{version}/{public_id}.{format}
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');

                // Find "upload" segment
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
                    return string.Empty;

                // Get everything after version (skip version segment)
                var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();
                var publicIdWithExtension = string.Join("/", publicIdParts);

                // Remove file extension
                var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
                if (lastDotIndex > 0)
                    return publicIdWithExtension.Substring(0, lastDotIndex);

                return publicIdWithExtension;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting public ID from URL: {imageUrl}");
                return string.Empty;
            }
        }
    }
}