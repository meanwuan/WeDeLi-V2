using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Vehicle;
using wedeli.Models.DTO.Common;
using wedeli.Service.Interface;
using wedeli.Repositories.Interface;
using wedeli.Models.Response;

namespace wedeli.Controller
{
    /// <summary>
    /// Vehicles Controller for managing vehicles
    /// </summary>
    [ApiController]
    [Route("api/v1/vehicles")]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IOrderRepository _orderRepository;
        private readonly ITripRepository _tripRepository;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(
            IVehicleService vehicleService,
            IOrderRepository orderRepository,
            ITripRepository tripRepository,
            ILogger<VehiclesController> logger)
        {
            _vehicleService = vehicleService;
            _orderRepository = orderRepository;
            _tripRepository = tripRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all vehicles with paging and filtering
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="companyId">Filter by company</param>
        /// <param name="vehicleType">Filter by type (truck, van, motorbike)</param>
        /// <param name="status">Filter by status</param>
        /// <param name="searchTerm">Search by license plate</param>
        /// <returns>Paginated list of vehicles</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<VehicleResponseDto>>>> GetVehicles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? companyId = null,
            [FromQuery] string? vehicleType = null,
            [FromQuery] string? status = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (companyId.HasValue && companyId > 0)
                {
                    var vehicles = await _vehicleService.GetVehiclesByCompanyAsync(companyId.Value);
                    
                    // Apply search filter if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        vehicles = vehicles.Where(v => 
                            v.LicensePlate.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    // Apply status filter if provided
                    if (!string.IsNullOrEmpty(status))
                    {
                        vehicles = vehicles.Where(v => 
                            string.Equals(v.CurrentStatus, status, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    // Apply vehicle type filter if provided
                    if (!string.IsNullOrEmpty(vehicleType))
                    {
                        vehicles = vehicles.Where(v => 
                            string.Equals(v.VehicleType, vehicleType, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    return Ok(new ApiResponse<IEnumerable<VehicleResponseDto>>
                    {
                        Success = true,
                        Message = "Vehicles retrieved successfully",
                        Data = vehicles.ToList()
                    });
                }

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Company ID is required"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving vehicles"
                });
            }
        }

        /// <summary>
        /// Get vehicle by ID with details
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <returns>Vehicle details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> GetVehicle(int id)
        {
            try
            {
                var result = await _vehicleService.GetVehicleByIdAsync(id);
                return Ok(new ApiResponse<VehicleResponseDto>
                {
                    Success = true,
                    Message = "Vehicle retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vehicle not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving vehicle"
                });
            }
        }

        /// <summary>
        /// Get vehicle capacity information
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <returns>Vehicle capacity details</returns>
        [HttpGet("{id}/capacity")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<VehicleCapacityDto>>> GetCapacity(int id)
        {
            try
            {
                var result = await _vehicleService.GetVehicleCapacityAsync(id);
                return Ok(new ApiResponse<VehicleCapacityDto>
                {
                    Success = true,
                    Message = "Vehicle capacity retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vehicle not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle capacity");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving vehicle capacity"
                });
            }
        }

        /// <summary>
        /// Get current orders on vehicle
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <returns>List of orders</returns>
        [HttpGet("{id}/current-orders")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetCurrentOrders(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Vehicle not found"
                    });

                var orders = await _orderRepository.GetByVehicleIdAsync(vehicle.VehicleId);
                return Ok(new ApiResponse<IEnumerable<object>>
                {
                    Success = true,
                    Message = "Current orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current orders");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving current orders"
                });
            }
        }

        /// <summary>
        /// Get trip history for vehicle
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <param name="startDate">Filter start date</param>
        /// <param name="endDate">Filter end date</param>
        /// <param name="status">Filter by trip status</param>
        /// <returns>List of trips</returns>
        [HttpGet("{id}/trips")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetTrips(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Vehicle not found"
                    });

                var trips = await _tripRepository.GetByVehicleIdAsync(vehicle.VehicleId);
                return Ok(new ApiResponse<IEnumerable<object>>
                {
                    Success = true,
                    Message = "Trip history retrieved successfully",
                    Data = trips
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip history");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving trip history"
                });
            }
        }

        /// <summary>
        /// Create new vehicle
        /// </summary>
        /// <param name="dto">Create vehicle request</param>
        /// <returns>Created vehicle</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> CreateVehicle([FromBody] CreateVehicleDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _vehicleService.CreateVehicleAsync(dto);
                return CreatedAtAction(nameof(GetVehicle), new { id = result.VehicleId },
                    new ApiResponse<VehicleResponseDto>
                    {
                        Success = true,
                        Message = "Vehicle created successfully",
                        Data = result
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Company not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Duplicate vehicle: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error creating vehicle"
                });
            }
        }

        /// <summary>
        /// Update vehicle information
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <param name="dto">Update request</param>
        /// <returns>Updated vehicle</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> UpdateVehicle(
            int id,
            [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _vehicleService.UpdateVehicleAsync(id, dto);
                return Ok(new ApiResponse<VehicleResponseDto>
                {
                    Success = true,
                    Message = "Vehicle updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vehicle not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating vehicle"
                });
            }
        }

        /// <summary>
        /// Update vehicle status
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <param name="dto">Status update request</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(
            int id,
            [FromBody] UpdateVehicleStatusDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Status))
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Status is required"
                    });

                var result = await _vehicleService.UpdateVehicleStatusAsync(id, dto.Status);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Vehicle status updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vehicle not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating vehicle status"
                });
            }
        }

        /// <summary>
        /// Update vehicle load/weight
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <param name="dto">Load update request</param>
        /// <returns>Updated capacity info</returns>
        [HttpPut("{id}/load")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<VehicleCapacityDto>>> UpdateLoad(
            int id,
            [FromBody] UpdateVehicleLoadDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                var newWeight = (vehicle.CurrentWeightKg) + dto.WeightChangeKg;

                await _vehicleService.UpdateVehicleWeightAsync(id, newWeight);
                var capacity = await _vehicleService.GetVehicleCapacityAsync(id);

                return Ok(new ApiResponse<VehicleCapacityDto>
                {
                    Success = true,
                    Message = "Vehicle load updated successfully",
                    Data = capacity
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vehicle not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle load");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating vehicle load"
                });
            }
        }

        /// <summary>
        /// Allow overload for vehicle
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <param name="dto">Overload approval request</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/allow-overload")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> AllowOverload(
            int id,
            [FromBody] ApproveOverloadDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _vehicleService.AllowOverloadAsync(id, dto.AllowOverload);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Overload allowance updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vehicle not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allowing overload");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error allowing overload"
                });
            }
        }

        /// <summary>
        /// Get available vehicles
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="vehicleType">Filter by type</param>
        /// <param name="minCapacity">Minimum capacity percentage</param>
        /// <returns>List of available vehicles</returns>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<VehicleResponseDto>>>> GetAvailable(
            [FromQuery] int? companyId = null,
            [FromQuery] string? vehicleType = null,
            [FromQuery] decimal? minCapacity = null)
        {
            try
            {
                if (!companyId.HasValue || companyId <= 0)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Company ID is required"
                    });

                var result = await _vehicleService.GetAvailableVehiclesAsync(companyId.Value);
                return Ok(new ApiResponse<IEnumerable<VehicleResponseDto>>
                {
                    Success = true,
                    Message = "Available vehicles retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available vehicles");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving available vehicles"
                });
            }
        }

        /// <summary>
        /// Get overloaded vehicles
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of overloaded vehicles</returns>
        [HttpGet("overloaded")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<VehicleResponseDto>>>> GetOverloaded(
            [FromQuery] int companyId)
        {
            try
            {
                if (companyId <= 0)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Company ID is required"
                    });

                var result = await _vehicleService.GetOverloadedVehiclesAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<VehicleResponseDto>>
                {
                    Success = true,
                    Message = "Overloaded vehicles retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overloaded vehicles");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving overloaded vehicles"
                });
            }
        }

        /// <summary>
        /// Delete vehicle
        /// </summary>
        /// <param name="id">Vehicle ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteVehicle(int id)
        {
            try
            {
                var result = await _vehicleService.DeleteVehicleAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Vehicle deleted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vehicle not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete vehicle: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting vehicle"
                });
            }
        }
    }
}
