using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Route;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Route service for managing route operations
    /// </summary>
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly ITransportCompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RouteService> _logger;

        public RouteService(
            IRouteRepository routeRepository,
            ITransportCompanyRepository companyRepository,
            IMapper mapper,
            ILogger<RouteService> logger)
        {
            _routeRepository = routeRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all routes with company info
        /// </summary>
        public async Task<IEnumerable<RouteResponseDto>> GetAllRoutesAsync()
        {
            try
            {
                var routes = await _routeRepository.GetAllWithCompanyAsync();
                var routeDtos = routes.Select(r => new RouteResponseDto
                {
                    RouteId = r.RouteId,
                    CompanyId = r.CompanyId,
                    CompanyName = r.Company?.CompanyName ?? "",
                    RouteName = r.RouteName ?? "",
                    OriginProvince = r.OriginProvince ?? "",
                    OriginDistrict = r.OriginDistrict ?? "",
                    DestinationProvince = r.DestinationProvince ?? "",
                    DestinationDistrict = r.DestinationDistrict ?? "",
                    DistanceKm = r.DistanceKm,
                    EstimatedDurationHours = r.EstimatedDurationHours,
                    BasePrice = r.BasePrice ?? 0,
                    IsActive = r.IsActive ?? false,
                    CreatedAt = r.CreatedAt ?? DateTime.UtcNow
                }).ToList();

                _logger.LogInformation("Retrieved all {Count} routes", routeDtos.Count);
                return routeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all routes");
                throw;
            }
        }

        /// <summary>
        /// Get route by ID
        /// </summary>
        public async Task<RouteResponseDto> GetRouteByIdAsync(int routeId)
        {
            try
            {
                var route = await _routeRepository.GetByIdAsync(routeId);
                var routeDto = _mapper.Map<RouteResponseDto>(route);

                // Enrich with company name from Platform DB
                if (route.CompanyId > 0)
                {
                    var company = await _companyRepository.GetByIdAsync(route.CompanyId);
                    routeDto.CompanyName = company?.CompanyName;
                }

                _logger.LogInformation("Retrieved route: {RouteId}", routeId);
                return routeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving route: {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Get routes by company
        /// </summary>
        public async Task<IEnumerable<RouteResponseDto>> GetRoutesByCompanyAsync(int companyId)
        {
            try
            {
                var routes = await _routeRepository.GetByCompanyIdAsync(companyId);
                var routeDtos = _mapper.Map<IEnumerable<RouteResponseDto>>(routes);

                _logger.LogInformation("Retrieved routes for company: {CompanyId}", companyId);
                return routeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get active routes by company
        /// </summary>
        public async Task<IEnumerable<RouteResponseDto>> GetActiveRoutesAsync(int companyId)
        {
            try
            {
                var routes = await _routeRepository.GetActiveRoutesAsync(companyId);
                var routeDtos = _mapper.Map<IEnumerable<RouteResponseDto>>(routes);

                _logger.LogInformation("Retrieved active routes for company: {CompanyId}", companyId);
                return routeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active routes: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Search routes by provinces
        /// </summary>
        public async Task<IEnumerable<RouteResponseDto>> SearchRoutesAsync(string originProvince, string destinationProvince)
        {
            try
            {
                var routes = await _routeRepository.SearchRoutesAsync(originProvince, destinationProvince);
                var routeDtos = _mapper.Map<IEnumerable<RouteResponseDto>>(routes);

                _logger.LogInformation("Searched routes from {Origin} to {Destination}", originProvince, destinationProvince);
                return routeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching routes");
                throw;
            }
        }

        /// <summary>
        /// Get optimal route between provinces
        /// </summary>
        public async Task<RouteResponseDto> GetOptimalRouteAsync(string originProvince, string destProvince, int? companyId = null)
        {
            try
            {
                var route = await _routeRepository.GetOptimalRouteAsync(originProvince, destProvince, companyId);
                var routeDto = _mapper.Map<RouteResponseDto>(route);

                _logger.LogInformation("Retrieved optimal route from {Origin} to {Destination}", originProvince, destProvince);
                return routeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimal route");
                throw;
            }
        }

        /// <summary>
        /// Create new route
        /// </summary>
        public async Task<RouteResponseDto> CreateRouteAsync(CreateRouteDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Verify company exists
                var company = await _companyRepository.GetByIdAsync(dto.CompanyId);
                if (company == null)
                    throw new KeyNotFoundException($"Company {dto.CompanyId} not found");

                var route = _mapper.Map<Models.Domain.Route>(dto);
                var createdRoute = await _routeRepository.AddAsync(route);
                var routeDto = _mapper.Map<RouteResponseDto>(createdRoute);

                _logger.LogInformation("Created route: {RouteId} ({RouteName})", createdRoute.RouteId, createdRoute.RouteName);
                return routeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route");
                throw;
            }
        }

        /// <summary>
        /// Update route
        /// </summary>
        public async Task<RouteResponseDto> UpdateRouteAsync(int routeId, UpdateRouteDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var route = await _routeRepository.GetByIdAsync(routeId);
                _mapper.Map(dto, route);

                var updatedRoute = await _routeRepository.UpdateAsync(route);
                var routeDto = _mapper.Map<RouteResponseDto>(updatedRoute);

                _logger.LogInformation("Updated route: {RouteId}", routeId);
                return routeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route: {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Delete route
        /// </summary>
        public async Task<bool> DeleteRouteAsync(int routeId)
        {
            try
            {
                var result = await _routeRepository.DeleteAsync(routeId);

                _logger.LogInformation("Deleted route: {RouteId}", routeId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting route: {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Toggle route active status
        /// </summary>
        public async Task<bool> ToggleRouteStatusAsync(int routeId, bool isActive)
        {
            try
            {
                var result = await _routeRepository.ToggleActiveStatusAsync(routeId, isActive);

                _logger.LogInformation("Toggled route status - RouteId: {RouteId}, IsActive: {IsActive}", routeId, isActive);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling route status: {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Calculate shipping fee based on route, weight, parcel type, and declared value
        /// </summary>
        public async Task<ShippingFeeResponseDto> CalculateShippingFeeAsync(CalculateShippingFeeDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Get route info
                var route = await _routeRepository.GetByIdAsync(dto.RouteId);
                if (route == null)
                    throw new KeyNotFoundException($"Route {dto.RouteId} not found");

                var basePrice = route.BasePrice ?? 0m;

                // Weight fee: 5,000đ per kg for weight > 1kg
                decimal weightFee = 0;
                if (dto.WeightKg > 1)
                {
                    weightFee = (dto.WeightKg - 1) * 5000m;
                }

                // Parcel type surcharge (percentage of base price)
                decimal parcelTypeFee = 0;
                var parcelType = dto.ParcelType?.ToLower() ?? "other";
                var surchargeRate = parcelType switch
                {
                    "fragile" => 0.20m,      // +20%
                    "electronics" => 0.15m,  // +15%
                    "cold" => 0.30m,         // +30%
                    "food" => 0.10m,         // +10%
                    "document" => 0m,        // +0%
                    _ => 0m                  // other: +0%
                };
                parcelTypeFee = basePrice * surchargeRate;

                // Insurance fee: 0.5% of declared value
                decimal insuranceFee = 0;
                if (dto.DeclaredValue.HasValue && dto.DeclaredValue > 0)
                {
                    insuranceFee = dto.DeclaredValue.Value * 0.005m;
                }

                // Calculate total
                decimal totalFee = basePrice + weightFee + parcelTypeFee + insuranceFee;

                var result = new ShippingFeeResponseDto
                {
                    RouteId = route.RouteId,
                    RouteName = route.RouteName ?? "",
                    BasePrice = basePrice,
                    WeightFee = weightFee,
                    ParcelTypeFee = parcelTypeFee,
                    InsuranceFee = insuranceFee,
                    TotalFee = totalFee,
                    EstimatedDurationHours = route.EstimatedDurationHours
                };

                _logger.LogInformation("Calculated shipping fee for route {RouteId}: {TotalFee}đ", dto.RouteId, totalFee);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping fee for route: {RouteId}", dto.RouteId);
                throw;
            }
        }

        /// <summary>
        /// Get transport companies that have routes matching the origin and destination
        /// Sorted by recommendation score (based on price and rating)
        /// </summary>
        public async Task<IEnumerable<CompanyRouteRecommendationDto>> GetCompaniesForRouteAsync(string originProvince, string destProvince)
        {
            try
            {
                if (string.IsNullOrEmpty(originProvince) || string.IsNullOrEmpty(destProvince))
                    throw new ArgumentException("Origin and destination provinces are required");

                // Get all active routes matching the origin and destination
                var routes = await _routeRepository.SearchRoutesAsync(originProvince, destProvince);
                
                if (!routes.Any())
                    return Enumerable.Empty<CompanyRouteRecommendationDto>();

                // Group routes by company and select the best route (cheapest) for each company
                var recommendations = routes
                    .Where(r => r.Company != null && r.Company.IsActive == true)
                    .GroupBy(r => r.CompanyId)
                    .Select(g =>
                    {
                        // Get the cheapest route for this company
                        var bestRoute = g.OrderBy(r => r.BasePrice ?? 0).First();
                        var company = bestRoute.Company!;
                        
                        // Calculate recommendation score:
                        // - Lower price is better (normalize: 1000000 / (price + 1) to avoid division by zero)
                        // - Higher rating is better (multiply by 100)
                        // Combined: prioritize price first, then rating
                        var priceScore = 1000000m / ((bestRoute.BasePrice ?? 100000m) + 1);
                        var ratingScore = (company.Rating ?? 3m) * 100;
                        var score = priceScore + ratingScore;

                        return new CompanyRouteRecommendationDto
                        {
                            CompanyId = company.CompanyId,
                            CompanyName = company.CompanyName ?? "",
                            Rating = company.Rating,
                            Phone = company.Phone ?? "",
                            Address = company.Address ?? "",
                            RouteId = bestRoute.RouteId,
                            RouteName = bestRoute.RouteName ?? "",
                            BasePrice = bestRoute.BasePrice ?? 0,
                            DistanceKm = bestRoute.DistanceKm,
                            EstimatedDurationHours = bestRoute.EstimatedDurationHours,
                            RecommendationScore = Math.Round(score, 2)
                        };
                    })
                    .OrderByDescending(r => r.RecommendationScore) // Best score first
                    .ToList();

                _logger.LogInformation("Found {Count} companies for route {Origin} -> {Dest}", 
                    recommendations.Count, originProvince, destProvince);
                
                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies for route: {Origin} -> {Dest}", originProvince, destProvince);
                throw;
            }
        }
    }
}
