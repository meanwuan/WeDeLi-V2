using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using wedeli.Hubs;
using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace wedeli.Controllers
{
    /// <summary>
    /// API Controller for managing order photos
    /// Supports: Upload photos, Get photos, Delete photos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhotosController : ControllerBase
    {
        private readonly IOrderPhotoRepository _photoRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IHubContext<TrackingHub> _trackingHub;
        private readonly ILogger<PhotosController> _logger;

        public PhotosController(
            IOrderPhotoRepository photoRepository,
            IOrderRepository orderRepository,
            ICloudinaryService cloudinaryService,
            IHubContext<TrackingHub> trackingHub,
            ILogger<PhotosController> logger)
        {
            _photoRepository = photoRepository;
            _orderRepository = orderRepository;
            _cloudinaryService = cloudinaryService;
            _trackingHub = trackingHub;
            _logger = logger;
        }

        /// <summary>
        /// Upload a single photo for an order
        /// POST /api/photos/upload
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(PhotoUploadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoRequestDto request)
        {
            try
            {
                // Validate input
                if (request.Photo == null || request.Photo.Length == 0)
                    return BadRequest(new { message = "Photo file is required" });

                // Validate photo type
                var validPhotoTypes = new[] { "before_delivery", "after_delivery", "parcel_condition", "signature", "damage_proof" };
                if (!validPhotoTypes.Contains(request.PhotoType))
                    return BadRequest(new { message = "Invalid photo type" });

                // Check if order exists
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                    return NotFound(new { message = "Order not found" });

                // Get current user ID from JWT claims
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                // Upload to Cloudinary
                using var stream = request.Photo.OpenReadStream();
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    stream,
                    request.Photo.FileName,
                    folder: "orders"
                );

                if (!uploadResult.Success)
                    return BadRequest(new { message = uploadResult.ErrorMessage });

                // Save photo info to database
                var orderPhoto = new OrderPhoto
                {
                    OrderId = request.OrderId,
                    PhotoType = request.PhotoType,
                    PhotoUrl = uploadResult.ImageUrl!,
                    FileName = request.Photo.FileName,
                    FileSizeKb = (int)(uploadResult.FileSizeBytes / 1024),
                    UploadedBy = userId,
                    UploadedAt = DateTime.Now
                };

                var savedPhoto = await _photoRepository.AddPhotoAsync(orderPhoto);

                // Broadcast photo upload via SignalR
                await _trackingHub.NotifyPhotoUpload(
                    request.OrderId,
                    request.PhotoType,
                    uploadResult.ImageUrl!,
                    userId
                );

                _logger.LogInformation($"Photo uploaded successfully for Order {request.OrderId} by User {userId}");

                // Return response
                return Ok(new PhotoUploadResponseDto
                {
                    PhotoId = savedPhoto.PhotoId,
                    OrderId = savedPhoto.OrderId,
                    PhotoType = savedPhoto.PhotoType,
                    PhotoUrl = savedPhoto.PhotoUrl,
                    PublicId = uploadResult.PublicId,
                    FileSizeKb = savedPhoto.FileSizeKb,
                    UploadedBy = savedPhoto.UploadedBy,
                    UploadedAt = (DateTime)savedPhoto.UploadedAt,
                    Message = "Photo uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Upload multiple photos at once
        /// POST /api/photos/batch-upload
        /// </summary>
        [HttpPost("batch-upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BatchUploadResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> BatchUploadPhotos([FromForm] BatchUploadPhotosRequestDto request)
        {
            var response = new BatchUploadResponseDto();

            try
            {
                // Check if order exists
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                    return NotFound(new { message = "Order not found" });

                // Get current user ID
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                // Upload each photo
                foreach (var photo in request.Photos)
                {
                    try
                    {
                        using var stream = photo.OpenReadStream();
                        var uploadResult = await _cloudinaryService.UploadImageAsync(
                            stream,
                            photo.FileName,
                            folder: "orders"
                        );

                        if (uploadResult.Success)
                        {
                            var orderPhoto = new OrderPhoto
                            {
                                OrderId = request.OrderId,
                                PhotoType = request.PhotoType,
                                PhotoUrl = uploadResult.ImageUrl!,
                                FileName = photo.FileName,
                                FileSizeKb = (int)(uploadResult.FileSizeBytes / 1024),
                                UploadedBy = userId,
                                UploadedAt = DateTime.Now
                            };

                            var savedPhoto = await _photoRepository.AddPhotoAsync(orderPhoto);

                            response.UploadedPhotos.Add(new PhotoUploadResponseDto
                            {
                                PhotoId = savedPhoto.PhotoId,
                                OrderId = savedPhoto.OrderId,
                                PhotoType = savedPhoto.PhotoType,
                                PhotoUrl = savedPhoto.PhotoUrl,
                                UploadedBy = savedPhoto.UploadedBy,
                                UploadedAt = (DateTime)savedPhoto.UploadedAt
                            });

                            response.TotalUploaded++;
                        }
                        else
                        {
                            response.TotalFailed++;
                            response.ErrorMessages.Add($"{photo.FileName}: {uploadResult.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        response.TotalFailed++;
                        response.ErrorMessages.Add($"{photo.FileName}: {ex.Message}");
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch upload");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all photos for a specific order
        /// GET /api/photos/order/{orderId}
        /// </summary>
        [HttpGet("order/{orderId}")]
        [AllowAnonymous] // Allow public access for tracking
        [ProducesResponseType(typeof(List<OrderPhotoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPhotosByOrderId(int orderId)
        {
            try
            {
                var photos = await _photoRepository.GetPhotosByOrderIdAsync(orderId);

                var photoDtos = photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    OrderId = p.OrderId,
                    PhotoType = p.PhotoType,
                    PhotoUrl = p.PhotoUrl,
                    FileName = p.FileName,
                    FileSizeKb = p.FileSizeKb,
                    UploadedBy = p.UploadedBy,
                    UploaderName = p.UploadedByNavigation?.FullName,
                    UploadedAt = (DateTime)p.UploadedAt
                }).ToList();

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting photos for Order {orderId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get photos by type for an order
        /// GET /api/photos/order/{orderId}/type/{photoType}
        /// </summary>
        [HttpGet("order/{orderId}/type/{photoType}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<OrderPhotoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPhotosByType(int orderId, string photoType)
        {
            try
            {
                var photos = await _photoRepository.GetPhotosByTypeAsync(orderId, photoType);

                var photoDtos = photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    OrderId = p.OrderId,
                    PhotoType = p.PhotoType,
                    PhotoUrl = p.PhotoUrl,
                    FileName = p.FileName,
                    FileSizeKb = p.FileSizeKb,
                    UploadedBy = p.UploadedBy,
                    UploaderName = p.UploadedByNavigation?.FullName,
                    UploadedAt = (DateTime)p.UploadedAt
                }).ToList();

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting photos for Order {orderId}, Type: {photoType}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a photo
        /// DELETE /api/photos/{photoId}
        /// </summary>
        [HttpDelete("{photoId}")]
        [Authorize(Roles = "admin,driver,warehouse_staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            try
            {
                // Get photo info
                var photo = await _photoRepository.GetPhotoByIdAsync(photoId);
                if (photo == null)
                    return NotFound(new { message = "Photo not found" });

                // Extract public ID from URL
                var publicId = _cloudinaryService.ExtractPublicIdFromUrl(photo.PhotoUrl);

                // Delete from Cloudinary
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }

                // Delete from database
                var deleted = await _photoRepository.DeletePhotoAsync(photoId);
                if (!deleted)
                    return NotFound(new { message = "Failed to delete photo from database" });

                _logger.LogInformation($"Photo deleted successfully: {photoId}");
                return Ok(new { message = "Photo deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting photo {photoId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get photo by ID
        /// GET /api/photos/{photoId}
        /// </summary>
        [HttpGet("{photoId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(OrderPhotoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPhotoById(int photoId)
        {
            try
            {
                var photo = await _photoRepository.GetPhotoByIdAsync(photoId);
                if (photo == null)
                    return NotFound(new { message = "Photo not found" });

                var photoDto = new OrderPhotoDto
                {
                    PhotoId = photo.PhotoId,
                    OrderId = photo.OrderId,
                    PhotoType = photo.PhotoType,
                    PhotoUrl = photo.PhotoUrl,
                    FileName = photo.FileName,
                    FileSizeKb = photo.FileSizeKb,
                    UploadedBy = photo.UploadedBy,
                    UploaderName = photo.UploadedByNavigation?.FullName,
                    UploadedAt = (DateTime)photo.UploadedAt
                };

                return Ok(photoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting photo {photoId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}