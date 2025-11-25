using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO;
using wedeli.service.Interface;

namespace wedeli.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        /// <summary>
        /// Get all vehicles with filtering and pagination
        /// GET /api/vehicles
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetVehicles([FromQuery] VehicleFilterDto filter)
        {
            try
            {
                var (vehicles, totalCount) = await _vehicleService.GetVehiclesAsync(filter);

                return Ok(new
                {
                    data = vehicles,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get vehicle by ID
        /// GET /api/vehicles/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                    return NotFound(new { message = "Vehicle not found" });

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get vehicle by license plate
        /// GET /api/vehicles/license-plate/{licensePlate}
        /// </summary>
        [HttpGet("license-plate/{licensePlate}")]
        public async Task<IActionResult> GetVehicleByLicensePlate(string licensePlate)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByLicensePlateAsync(licensePlate);
                if (vehicle == null)
                    return NotFound(new { message = "Vehicle not found" });

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle by license plate: {licensePlate}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get vehicles by company
        /// GET /api/vehicles/company/{companyId}
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetVehiclesByCompany(int companyId)
        {
            try
            {
                var vehicles = await _vehicleService.GetVehiclesByCompanyAsync(companyId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicles for company: {companyId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new vehicle
        /// POST /api/vehicles
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto dto)
        {
            try
            {
                var vehicle = await _vehicleService.CreateVehicleAsync(dto);
                return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.VehicleId }, vehicle);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update vehicle information
        /// PUT /api/vehicles/{id}
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                var vehicle = await _vehicleService.UpdateVehicleAsync(id, dto);
                return Ok(vehicle);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Vehicle not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete vehicle (soft delete)
        /// DELETE /api/vehicles/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var deleted = await _vehicleService.DeleteVehicleAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Vehicle not found" });

                return Ok(new { message = "Vehicle deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vehicle: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Add weight to vehicle
        /// POST /api/vehicles/{id}/add-weight
        /// </summary>
        [HttpPost("{id}/add-weight")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> AddWeightToVehicle(int id, [FromBody] UpdateVehicleLoadDto dto)
        {
            try
            {
                var result = await _vehicleService.AddWeightToVehicleAsync(id, dto.WeightKg);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Vehicle not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding weight to vehicle: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Remove weight from vehicle
        /// POST /api/vehicles/{id}/remove-weight
        /// </summary>
        [HttpPost("{id}/remove-weight")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> RemoveWeightFromVehicle(int id, [FromBody] UpdateVehicleLoadDto dto)
        {
            try
            {
                var result = await _vehicleService.RemoveWeightFromVehicleAsync(id, dto.WeightKg);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Vehicle not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing weight from vehicle: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Reset vehicle load to zero
        /// POST /api/vehicles/{id}/reset-load
        /// </summary>
        [HttpPost("{id}/reset-load")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> ResetVehicleLoad(int id)
        {
            try
            {
                var vehicle = await _vehicleService.ResetVehicleLoadAsync(id);
                return Ok(vehicle);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Vehicle not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting vehicle load: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if vehicle can accommodate weight
        /// GET /api/vehicles/{id}/can-accommodate/{weightKg}
        /// </summary>
        [HttpGet("{id}/can-accommodate/{weightKg}")]
        public async Task<IActionResult> CanAccommodateWeight(int id, decimal weightKg)
        {
            try
            {
                var canAccommodate = await _vehicleService.CanAccommodateWeightAsync(id, weightKg);
                return Ok(new { vehicleId = id, weightKg, canAccommodate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking vehicle capacity: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Find suitable vehicles for order weight
        /// GET /api/vehicles/find-suitable?weightKg=100&companyId=1
        /// </summary>
        [HttpGet("find-suitable")]
        public async Task<IActionResult> FindSuitableVehicles([FromQuery] decimal weightKg, [FromQuery] int? companyId = null)
        {
            try
            {
                var vehicles = await _vehicleService.FindSuitableVehiclesForOrderAsync(weightKg, companyId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding suitable vehicles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update vehicle status
        /// PUT /api/vehicles/{id}/status
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> UpdateVehicleStatus(int id, [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.CurrentStatus))
                    return BadRequest(new { message = "Status is required" });

                var vehicle = await _vehicleService.UpdateVehicleStatusAsync(id, dto.CurrentStatus);
                return Ok(vehicle);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Vehicle not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle status: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get available vehicles
        /// GET /api/vehicles/available?companyId=1
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableVehicles([FromQuery] int? companyId = null)
        {
            try
            {
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync(companyId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available vehicles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get overloaded vehicles
        /// GET /api/vehicles/overloaded?companyId=1
        /// </summary>
        [HttpGet("overloaded")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetOverloadedVehicles([FromQuery] int? companyId = null)
        {
            try
            {
                var vehicles = await _vehicleService.GetOverloadedVehiclesAsync(companyId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overloaded vehicles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get vehicle statistics
        /// GET /api/vehicles/{id}/statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetVehicleStatistics(int id)
        {
            try
            {
                var stats = await _vehicleService.GetVehicleStatisticsAsync(id);
                return Ok(stats);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Vehicle not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle statistics: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get vehicle count by status
        /// GET /api/vehicles/count-by-status?companyId=1
        /// </summary>
        [HttpGet("count-by-status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetVehicleCountByStatus([FromQuery] int? companyId = null)
        {
            try
            {
                var counts = await _vehicleService.GetVehicleCountByStatusAsync(companyId);
                return Ok(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle count by status");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}