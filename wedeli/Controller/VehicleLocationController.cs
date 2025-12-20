using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO.Vehicle;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controller;

/// <summary>
/// REST API endpoints for vehicle location (non-realtime access)
/// </summary>
[ApiController]
[Route("api/v1/vehicle-locations")]
[Authorize]
public class VehicleLocationController : ControllerBase
{
    private readonly IVehicleLocationService _locationService;
    private readonly ILogger<VehicleLocationController> _logger;

    public VehicleLocationController(
        IVehicleLocationService locationService,
        ILogger<VehicleLocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy vị trí mới nhất của xe
    /// </summary>
    [HttpGet("vehicle/{vehicleId}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleLocationDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetVehicleLocation(int vehicleId)
    {
        try
        {
            var location = await _locationService.GetLatestLocationAsync(vehicleId);
            if (location == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Vehicle {vehicleId} not found", 404));
            }

            return Ok(ApiResponse<VehicleLocationDto>.SuccessResponse(location, "Vehicle location retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location for vehicle {VehicleId}", vehicleId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Lấy vị trí tất cả xe của công ty
    /// </summary>
    [HttpGet("company/{companyId}")]
    [ProducesResponseType(typeof(ApiResponse<CompanyVehicleLocationsDto>), 200)]
    public async Task<IActionResult> GetCompanyVehicleLocations(int companyId)
    {
        try
        {
            var locations = await _locationService.GetCompanyVehicleLocationsAsync(companyId);
            
            return Ok(ApiResponse<CompanyVehicleLocationsDto>.SuccessResponse(
                locations, 
                $"Found {locations.TotalVehicles} vehicles, {locations.OnlineVehicles} online"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting locations for company {CompanyId}", companyId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Driver cập nhật vị trí (REST alternative to SignalR)
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Driver,CompanyAdmin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<VehicleLocationDto>), 200)]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateVehicleLocationDto locationDto)
    {
        try
        {
            var location = await _locationService.UpdateLocationAsync(locationDto);
            
            return Ok(ApiResponse<VehicleLocationDto>.SuccessResponse(location, "Location updated"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location for vehicle {VehicleId}", locationDto.VehicleId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Lấy lịch sử vị trí xe
    /// </summary>
    [HttpGet("vehicle/{vehicleId}/history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VehicleLocationDto>>), 200)]
    public async Task<IActionResult> GetLocationHistory(
        int vehicleId, 
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
            var toDate = to ?? DateTime.UtcNow;

            var history = await _locationService.GetLocationHistoryAsync(vehicleId, fromDate, toDate);
            
            return Ok(ApiResponse<IEnumerable<VehicleLocationDto>>.SuccessResponse(
                history, 
                $"Found {history.Count()} location records"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history for vehicle {VehicleId}", vehicleId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error", 500));
        }
    }
}

