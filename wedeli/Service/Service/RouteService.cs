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
        /// Get route by ID
        /// </summary>
        public async Task<RouteResponseDto> GetRouteByIdAsync(int routeId)
        {
            try
            {
                var route = await _routeRepository.GetByIdAsync(routeId);
                var routeDto = _mapper.Map<RouteResponseDto>(route);

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
    }
}
