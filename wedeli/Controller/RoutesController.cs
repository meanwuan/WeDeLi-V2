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

namespace wedeli.Controller
{
    /// <summary>
    /// Routes Controller for managing delivery routes
    /// </summary>
    [ApiController]
    [Route("api/v1/routes")]
    [Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;
        private readonly ILogger<RoutesController> _logger;

        public RoutesController(IRouteService routeService, ILogger<RoutesController> logger)
        {
            _routeService = routeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all routes (for customers to select)
        /// </summary>
        /// <returns>List of all active routes</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<RouteResponseDto>>>> GetAllRoutes()
        {
            try
            {
                var result = await _routeService.GetAllRoutesAsync();
                return Ok(new ApiResponse<IEnumerable<RouteResponseDto>>
                {
                    Success = true,
                    Message = "Routes retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all routes");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving routes"
                });
            }
        }

        /// <summary>
        /// Get route by ID
        /// </summary>
        /// <param name="routeId">Route ID</param>
        /// <returns>Route details</returns>
        [HttpGet("{routeId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<RouteResponseDto>>> GetRoute(int routeId)
        {
            try
            {
                var result = await _routeService.GetRouteByIdAsync(routeId);
                return Ok(new ApiResponse<RouteResponseDto>
                {
                    Success = true,
                    Message = "Route retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Route not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving route");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving route"
                });
            }
        }

        /// <summary>
        /// Get all routes for company
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of routes</returns>
        [HttpGet("company/{companyId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RouteResponseDto>>>> GetRoutesByCompany(int companyId)
        {
            try
            {
                var result = await _routeService.GetRoutesByCompanyAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<RouteResponseDto>>
                {
                    Success = true,
                    Message = "Routes retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes for company: {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving routes"
                });
            }
        }

        /// <summary>
        /// Get active routes for company
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of active routes</returns>
        [HttpGet("company/{companyId}/active")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RouteResponseDto>>>> GetActiveRoutes(int companyId)
        {
            try
            {
                var result = await _routeService.GetActiveRoutesAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<RouteResponseDto>>
                {
                    Success = true,
                    Message = "Active routes retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active routes: {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving active routes"
                });
            }
        }

        /// <summary>
        /// Search routes by provinces
        /// </summary>
        /// <param name="originProvince">Origin province</param>
        /// <param name="destinationProvince">Destination province</param>
        /// <returns>List of matching routes</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<RouteResponseDto>>>> SearchRoutes(
            [FromQuery] string originProvince,
            [FromQuery] string destinationProvince)
        {
            try
            {
                if (string.IsNullOrEmpty(originProvince) && string.IsNullOrEmpty(destinationProvince))
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "At least one search parameter is required"
                    });

                var result = await _routeService.SearchRoutesAsync(originProvince, destinationProvince);
                return Ok(new ApiResponse<IEnumerable<RouteResponseDto>>
                {
                    Success = true,
                    Message = "Routes searched successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching routes");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error searching routes"
                });
            }
        }

        /// <summary>
        /// Get optimal route between provinces
        /// </summary>
        /// <param name="originProvince">Origin province</param>
        /// <param name="destProvince">Destination province</param>
        /// <param name="companyId">Optional company filter</param>
        /// <returns>Optimal route</returns>
        [HttpGet("optimal")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RouteResponseDto>>> GetOptimalRoute(
            [FromQuery] string originProvince,
            [FromQuery] string destProvince,
            [FromQuery] int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(originProvince) || string.IsNullOrEmpty(destProvince))
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Origin and destination provinces are required"
                    });

                var result = await _routeService.GetOptimalRouteAsync(originProvince, destProvince, companyId);
                return Ok(new ApiResponse<RouteResponseDto>
                {
                    Success = true,
                    Message = "Optimal route retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Route not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimal route");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving optimal route"
                });
            }
        }

        /// <summary>
        /// Get recommended transport companies for a route
        /// Returns companies sorted by recommendation score (based on price and rating)
        /// </summary>
        /// <param name="originProvince">Origin province</param>
        /// <param name="destProvince">Destination province</param>
        /// <returns>List of recommended companies with their best routes</returns>
        [HttpGet("companies")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<CompanyRouteRecommendationDto>>>> GetCompaniesForRoute(
            [FromQuery] string originProvince,
            [FromQuery] string destProvince)
        {
            try
            {
                if (string.IsNullOrEmpty(originProvince) || string.IsNullOrEmpty(destProvince))
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Origin and destination provinces are required"
                    });

                var result = await _routeService.GetCompaniesForRouteAsync(originProvince, destProvince);
                return Ok(new ApiResponse<IEnumerable<CompanyRouteRecommendationDto>>
                {
                    Success = true,
                    Message = $"Found {result.Count()} companies for route {originProvince} -> {destProvince}",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies for route");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error getting companies for route"
                });
            }
        }

        /// <summary>
        /// Create new route
        /// </summary>
        /// <param name="dto">Create route request</param>
        /// <returns>Created route</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<RouteResponseDto>>> CreateRoute([FromBody] CreateRouteDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _routeService.CreateRouteAsync(dto);
                return CreatedAtAction(nameof(GetRoute), new { routeId = result.RouteId },
                    new ApiResponse<RouteResponseDto>
                    {
                        Success = true,
                        Message = "Route created successfully",
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
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error creating route"
                });
            }
        }

        /// <summary>
        /// Update route
        /// </summary>
        /// <param name="routeId">Route ID</param>
        /// <param name="dto">Update request</param>
        /// <returns>Updated route</returns>
        [HttpPut("{routeId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<RouteResponseDto>>> UpdateRoute(int routeId, [FromBody] UpdateRouteDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _routeService.UpdateRouteAsync(routeId, dto);
                return Ok(new ApiResponse<RouteResponseDto>
                {
                    Success = true,
                    Message = "Route updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Route not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route: {RouteId}", routeId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating route"
                });
            }
        }

        /// <summary>
        /// Toggle route active status
        /// </summary>
        /// <param name="routeId">Route ID</param>
        /// <param name="isActive">Active status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{routeId}/status")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleStatus(int routeId, [FromQuery] bool isActive)
        {
            try
            {
                var result = await _routeService.ToggleRouteStatusAsync(routeId, isActive);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Route status changed to {(isActive ? "active" : "inactive")}",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Route not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling route status: {RouteId}", routeId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error toggling route status"
                });
            }
        }

        /// <summary>
        /// Delete route
        /// </summary>
        /// <param name="routeId">Route ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{routeId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRoute(int routeId)
        {
            try
            {
                var result = await _routeService.DeleteRouteAsync(routeId);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Route deleted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Route not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operation not allowed: {Message}", ex.Message);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting route: {RouteId}", routeId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting route"
                });
            }
        }

        /// <summary>
        /// Calculate shipping fee based on route, weight, parcel type, and declared value
        /// </summary>
        /// <param name="dto">Shipping fee calculation request</param>
        /// <returns>Calculated shipping fee details</returns>
        [HttpPost("calculate-fee")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ShippingFeeResponseDto>>> CalculateFee([FromBody] CalculateShippingFeeDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });

                var result = await _routeService.CalculateShippingFeeAsync(dto);
                return Ok(new ApiResponse<ShippingFeeResponseDto>
                {
                    Success = true,
                    Message = "Shipping fee calculated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Route not found: {Message}", ex.Message);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping fee");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error calculating shipping fee"
                });
            }
        }
    }
}
