using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Route;
using wedeli.Models.DTO.Common;
using wedeli.Service.Interface;
using wedeli.Models.Response;
using wedeli.Models.DTO.Trip;

namespace wedeli.Controller
{
    /// <summary>
    /// Trips Controller for managing trips
    /// </summary>
    [ApiController]
    [Route("api/v1/trips")]
    [Authorize]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ILogger<TripsController> _logger;

        public TripsController(
            ITripService tripService,
            ILogger<TripsController> logger)
        {
            _tripService = tripService;
            _logger = logger;
        }

        /// <summary>
        /// Get trip by ID
        /// </summary>
        /// <param name="id">Trip ID</param>
        /// <returns>Trip details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<TripResponseDto>>> GetTrip(int id)
        {
            try
            {
                var result = await _tripService.GetTripByIdAsync(id);
                return Ok(new ApiResponse<TripResponseDto>
                {
                    Success = true,
                    Message = "Trip retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Trip not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trip"
                });
            }
        }

        /// <summary>
        /// Get trips by route
        /// </summary>
        /// <param name="routeId">Route ID</param>
        /// <returns>List of trips</returns>
        [HttpGet("route/{routeId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripResponseDto>>>> GetTripsByRoute(int routeId)
        {
            try
            {
                var result = await _tripService.GetTripsByRouteAsync(routeId);
                return Ok(new ApiResponse<IEnumerable<TripResponseDto>>
                {
                    Success = true,
                    Message = "Trips retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by route");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trips"
                });
            }
        }

        /// <summary>
        /// Get trips by vehicle
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <returns>List of trips</returns>
        [HttpGet("vehicle/{vehicleId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripResponseDto>>>> GetTripsByVehicle(int vehicleId)
        {
            try
            {
                var result = await _tripService.GetTripsByVehicleAsync(vehicleId);
                return Ok(new ApiResponse<IEnumerable<TripResponseDto>>
                {
                    Success = true,
                    Message = "Trips retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by vehicle");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trips"
                });
            }
        }

        /// <summary>
        /// Get trips by driver
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <returns>List of trips</returns>
        [HttpGet("driver/{driverId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripResponseDto>>>> GetTripsByDriver(int driverId)
        {
            try
            {
                var result = await _tripService.GetTripsByDriverAsync(driverId);
                return Ok(new ApiResponse<IEnumerable<TripResponseDto>>
                {
                    Success = true,
                    Message = "Trips retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by driver");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trips"
                });
            }
        }

        /// <summary>
        /// Get trips by date
        /// </summary>
        /// <param name="date">Trip date</param>
        /// <param name="companyId">Company ID filter</param>
        /// <returns>List of trips</returns>
        [HttpGet("date/{date}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripResponseDto>>>> GetTripsByDate(
            DateTime date,
            [FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _tripService.GetTripsByDateAsync(date, companyId);
                return Ok(new ApiResponse<IEnumerable<TripResponseDto>>
                {
                    Success = true,
                    Message = "Trips retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by date");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trips"
                });
            }
        }

        /// <summary>
        /// Get trips by status
        /// </summary>
        /// <param name="status">Trip status (scheduled, in_progress, completed, cancelled)</param>
        /// <param name="companyId">Company ID filter</param>
        /// <returns>List of trips</returns>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripResponseDto>>>> GetTripsByStatus(
            string status,
            [FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _tripService.GetTripsByStatusAsync(status, companyId);
                return Ok(new ApiResponse<IEnumerable<TripResponseDto>>
                {
                    Success = true,
                    Message = "Trips retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trips"
                });
            }
        }

        /// <summary>
        /// Get active trips
        /// </summary>
        /// <param name="companyId">Company ID filter</param>
        /// <returns>List of active trips</returns>
        [HttpGet("active")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripResponseDto>>>> GetActiveTrips(
            [FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _tripService.GetActiveTripsAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<TripResponseDto>>
                {
                    Success = true,
                    Message = "Active trips retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active trips");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trips"
                });
            }
        }

        /// <summary>
        /// Get return trips
        /// </summary>
        /// <param name="companyId">Company ID filter</param>
        /// <returns>List of return trips</returns>
        [HttpGet("return")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripResponseDto>>>> GetReturnTrips(
            [FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _tripService.GetReturnTripsAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<TripResponseDto>>
                {
                    Success = true,
                    Message = "Return trips retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving return trips");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trips"
                });
            }
        }

        /// <summary>
        /// Create new trip
        /// </summary>
        /// <param name="dto">Create trip request</param>
        /// <returns>Created trip</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<TripResponseDto>>> CreateTrip([FromBody] CreateTripDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _tripService.CreateTripAsync(dto);
                return CreatedAtAction(nameof(GetTrip), new { id = result.TripId },
                    new ApiResponse<TripResponseDto>
                    {
                        Success = true,
                        Message = "Trip created successfully",
                        Data = result
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Resource not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentNullException)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error creating trip"
                });
            }
        }

        /// <summary>
        /// Update trip
        /// </summary>
        /// <param name="id">Trip ID</param>
        /// <param name="dto">Update trip request</param>
        /// <returns>Updated trip</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<TripResponseDto>>> UpdateTrip(
            int id,
            [FromBody] UpdateTripDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _tripService.UpdateTripAsync(id, dto);
                return Ok(new ApiResponse<TripResponseDto>
                {
                    Success = true,
                    Message = "Trip updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Trip not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating trip"
                });
            }
        }

        /// <summary>
        /// Update trip status
        /// </summary>
        /// <param name="id">Trip ID</param>
        /// <param name="dto">Status update request</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(
            int id,
            [FromBody] UpdateTripStatusDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Status))
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Status is required"
                    });

                var result = await _tripService.UpdateTripStatusAsync(id, dto.Status);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Trip status updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Trip not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating trip status"
                });
            }
        }

        /// <summary>
        /// Start trip (change status to in_progress)
        /// </summary>
        /// <param name="id">Trip ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/start")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> StartTrip(int id)
        {
            try
            {
                var result = await _tripService.StartTripAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Trip started successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Trip not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error starting trip"
                });
            }
        }

        /// <summary>
        /// Complete trip (change status to completed)
        /// </summary>
        /// <param name="id">Trip ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> CompleteTrip(int id)
        {
            try
            {
                var result = await _tripService.CompleteTripAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Trip completed successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Trip not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error completing trip"
                });
            }
        }

        /// <summary>
        /// Delete trip
        /// </summary>
        /// <param name="id">Trip ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTrip(int id)
        {
            try
            {
                var result = await _tripService.DeleteTripAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Trip deleted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Trip not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete trip: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting trip"
                });
            }
        }

        /// <summary>
        /// Assign order to trip
        /// </summary>
        /// <param name="tripId">Trip ID</param>
        /// <param name="orderId">Order ID</param>
        /// <param name="sequenceNumber">Delivery sequence number</param>
        /// <returns>Success status</returns>
        [HttpPost("{tripId}/orders/{orderId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignOrder(
            int tripId,
            int orderId,
            [FromQuery] int? sequenceNumber = null)
        {
            try
            {
                var result = await _tripService.AssignOrderToTripAsync(tripId, orderId, sequenceNumber);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order assigned to trip successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Resource not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning order to trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error assigning order to trip"
                });
            }
        }

        /// <summary>
        /// Remove order from trip
        /// </summary>
        /// <param name="tripId">Trip ID</param>
        /// <param name="orderId">Order ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{tripId}/orders/{orderId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveOrder(int tripId, int orderId)
        {
            try
            {
                var result = await _tripService.RemoveOrderFromTripAsync(tripId, orderId);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order removed from trip successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Resource not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order from trip");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error removing order from trip"
                });
            }
        }

        /// <summary>
        /// Get trip orders
        /// </summary>
        /// <param name="tripId">Trip ID</param>
        /// <returns>List of trip orders</returns>
        [HttpGet("{tripId}/orders")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripOrderDto>>>> GetTripOrders(int tripId)
        {
            try
            {
                var result = await _tripService.GetTripOrdersAsync(tripId);
                return Ok(new ApiResponse<IEnumerable<TripOrderDto>>
                {
                    Success = true,
                    Message = "Trip orders retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Trip not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip orders");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trip orders"
                });
            }
        }
    }

    /// <summary>
    /// Update trip status DTO
    /// </summary>
    public class UpdateTripStatusDto
    {
        public string? Status { get; set; }
    }
}
