using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Rating;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    /// <summary>
    /// Ratings controller for managing order ratings and reviews
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/ratings")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingController> _logger;

        public RatingController(
            IRatingService ratingService,
            ILogger<RatingController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        /// <summary>
        /// Create new rating
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RatingResponseDto>), 201)]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Rating data is required" });

                var result = await _ratingService.CreateRatingAsync(dto);
                if (result == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to create rating" });

                _logger.LogInformation("Rating created for order: {OrderId}", dto.OrderId);

                return CreatedAtAction(nameof(GetRatingDetail), new { id = result.RatingId },
                    new ApiResponse<RatingResponseDto>
                    {
                        Success = true,
                        Message = "Rating created successfully",
                        Data = result
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating");
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error creating rating" });
            }
        }

        /// <summary>
        /// Get rating by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RatingResponseDto>), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingDetail([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid rating ID" });

                var result = await _ratingService.GetRatingByIdAsync(id);
                if (result == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Rating not found" });

                return Ok(new ApiResponse<RatingResponseDto>
                {
                    Success = true,
                    Message = "Rating retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rating: {RatingId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving rating" });
            }
        }

        /// <summary>
        /// Get ratings by order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RatingResponseDto>>), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByOrder([FromRoute] int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid order ID" });

                var result = await _ratingService.GetRatingsByOrderAsync(orderId);

                return Ok(new ApiResponse<IEnumerable<RatingResponseDto>>
                {
                    Success = true,
                    Message = "Ratings retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings for order: {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving ratings" });
            }
        }

        /// <summary>
        /// Get ratings by driver
        /// </summary>
        [HttpGet("driver/{driverId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RatingResponseDto>>), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByDriver([FromRoute] int driverId)
        {
            try
            {
                if (driverId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid driver ID" });

                var result = await _ratingService.GetRatingsByDriverAsync(driverId);

                return Ok(new ApiResponse<IEnumerable<RatingResponseDto>>
                {
                    Success = true,
                    Message = "Ratings retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings for driver: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving ratings" });
            }
        }

        /// <summary>
        /// Get driver average rating
        /// </summary>
        [HttpGet("driver/{driverId}/average")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDriverAverageRating([FromRoute] int driverId)
        {
            try
            {
                if (driverId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid driver ID" });

                var result = await _ratingService.GetAverageDriverRatingAsync(driverId);

                return Ok(new ApiResponse<decimal>
                {
                    Success = true,
                    Message = "Average rating retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average rating for driver: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving average rating" });
            }
        }

        /// <summary>
        /// Get driver rating summary
        /// </summary>
        [HttpGet("driver/{driverId}/summary")]
        [ProducesResponseType(typeof(ApiResponse<DriverRatingSummaryDto>), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDriverRatingSummary([FromRoute] int driverId)
        {
            try
            {
                if (driverId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid driver ID" });

                var result = await _ratingService.GetDriverRatingSummaryAsync(driverId);

                return Ok(new ApiResponse<DriverRatingSummaryDto>
                {
                    Success = true,
                    Message = "Rating summary retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rating summary for driver: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving rating summary" });
            }
        }

        /// <summary>
        /// Get ratings by customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RatingResponseDto>>), 200)]
        public async Task<IActionResult> GetRatingsByCustomer([FromRoute] int customerId)
        {
            try
            {
                if (customerId <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid customer ID" });

                var result = await _ratingService.GetRatingsByCustomerAsync(customerId);

                return Ok(new ApiResponse<IEnumerable<RatingResponseDto>>
                {
                    Success = true,
                    Message = "Ratings retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings for customer: {CustomerId}", customerId);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error retrieving ratings" });
            }
        }

        /// <summary>
        /// Update rating
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> UpdateRating(
            [FromRoute] int id,
            [FromBody] UpdateRatingDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid rating ID" });

                if (dto == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Rating data is required" });

                var result = await _ratingService.UpdateRatingAsync(id, dto);
                if (!result)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to update rating" });

                _logger.LogInformation("Rating updated: {RatingId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Rating updated successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating: {RatingId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error updating rating" });
            }
        }

        /// <summary>
        /// Delete rating
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> DeleteRating([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid rating ID" });

                var result = await _ratingService.DeleteRatingAsync(id);
                if (!result)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to delete rating" });

                _logger.LogInformation("Rating deleted: {RatingId}", id);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Rating deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating: {RatingId}", id);
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Error deleting rating" });
            }
        }
    }
}
