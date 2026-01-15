using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Infrastructure;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Route repository for managing route data access
    /// </summary>
    public class RouteRepository : IRouteRepository
    {
        private readonly AppDbContext _context;
        private readonly PlatformDbContext _platformContext;
        private readonly ILogger<RouteRepository> _logger;

        public RouteRepository(AppDbContext context, PlatformDbContext platformContext, ILogger<RouteRepository> logger)
        {
            _context = context;
            _platformContext = platformContext;
            _logger = logger;
        }

        /// <summary>
        /// Get route by ID
        /// </summary>
        public async Task<Models.Domain.Route> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Route ID must be greater than 0", nameof(id));

                var route = await _context.Routes
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .FirstOrDefaultAsync(r => r.RouteId == id);

                if (route == null)
                    throw new KeyNotFoundException($"Route {id} not found");

                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving route: {RouteId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get routes by company
        /// </summary>
        public async Task<IEnumerable<Models.Domain.Route>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var routes = await _context.Routes
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Where(r => r.CompanyId == companyId)
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();

                return routes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes by company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get active routes by company
        /// </summary>
        public async Task<IEnumerable<Models.Domain.Route>> GetActiveRoutesAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var routes = await _context.Routes
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Where(r => r.CompanyId == companyId && r.IsActive == true)
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();

                return routes;
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
        public async Task<IEnumerable<Models.Domain.Route>> SearchRoutesAsync(string originProvince, string destinationProvince)
        {
            try
            {
                var query = _context.Routes
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .AsQueryable();

                if (!string.IsNullOrEmpty(originProvince) && originProvince != null)
                    query = query.Where(r => r.OriginProvince != null && r.OriginProvince.Contains(originProvince));

                if (!string.IsNullOrEmpty(destinationProvince) && destinationProvince != null)
                    query = query.Where(r => r.DestinationProvince != null && r.DestinationProvince.Contains(destinationProvince));

                var routes = await query
                    .Where(r => r.IsActive == true)
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();

                return routes;
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
        public async Task<Models.Domain.Route> GetOptimalRouteAsync(string originProvince, string destProvince, int? companyId = null)
        {
            try
            {
                var query = _context.Routes
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Where(r => r.OriginProvince == originProvince && 
                               r.DestinationProvince == destProvince &&
                               r.IsActive == true);

                if (companyId.HasValue && companyId > 0)
                    query = query.Where(r => r.CompanyId == companyId);

                var route = await query
                    .OrderBy(r => r.DistanceKm)
                    .FirstOrDefaultAsync();

                if (route == null)
                    throw new KeyNotFoundException($"No active route found from {originProvince} to {destProvince}");

                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimal route from {Origin} to {Destination}", originProvince, destProvince);
                throw;
            }
        }

        /// <summary>
        /// Get all routes with company info (active only for customer view)
        /// </summary>
        public async Task<IEnumerable<Models.Domain.Route>> GetAllWithCompanyAsync()
        {
            try
            {
                var routes = await _context.Routes
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    // Company.IsActive check must be done separately in Platform DB
                    .Where(r => r.IsActive == true)
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();

                return routes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all routes with company");
                throw;
            }
        }

        /// <summary>
        /// Get all routes
        /// </summary>
        public async Task<IEnumerable<Models.Domain.Route>> GetAllAsync()
        {
            try
            {
                var routes = await _context.Routes
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();

                return routes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all routes");
                throw;
            }
        }

        /// <summary>
        /// Add new route
        /// </summary>
        public async Task<Models.Domain.Route> AddAsync(Models.Domain.Route entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.CompanyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0");

                // Verify company exists (from Platform database)
                var companyExists = await _platformContext.TransportCompanies
                    .AnyAsync(c => c.CompanyId == entity.CompanyId);
                if (!companyExists)
                    throw new KeyNotFoundException($"Company {entity.CompanyId} not found");

                entity.CreatedAt = DateTime.UtcNow;
                entity.IsActive = true;

                _context.Routes.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new route: {RouteId} ({RouteName})", entity.RouteId, entity.RouteName);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding route");
                throw;
            }
        }

        /// <summary>
        /// Update route
        /// </summary>
        public async Task<Models.Domain.Route> UpdateAsync(Models.Domain.Route entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingRoute = await _context.Routes
                    .FirstOrDefaultAsync(r => r.RouteId == entity.RouteId);

                if (existingRoute == null)
                    throw new KeyNotFoundException($"Route {entity.RouteId} not found");

                existingRoute.RouteName = entity.RouteName ?? existingRoute.RouteName;
                existingRoute.OriginProvince = entity.OriginProvince ?? existingRoute.OriginProvince;
                existingRoute.OriginDistrict = entity.OriginDistrict ?? existingRoute.OriginDistrict;
                existingRoute.DestinationProvince = entity.DestinationProvince ?? existingRoute.DestinationProvince;
                existingRoute.DestinationDistrict = entity.DestinationDistrict ?? existingRoute.DestinationDistrict;
                existingRoute.DistanceKm = entity.DistanceKm ?? existingRoute.DistanceKm;
                existingRoute.EstimatedDurationHours = entity.EstimatedDurationHours ?? existingRoute.EstimatedDurationHours;
                existingRoute.BasePrice = entity.BasePrice ?? existingRoute.BasePrice;

                if (entity.IsActive.HasValue)
                    existingRoute.IsActive = entity.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated route: {RouteId}", entity.RouteId);
                return existingRoute;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route: {RouteId}", entity?.RouteId);
                throw;
            }
        }

        /// <summary>
        /// Delete route
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Route ID must be greater than 0", nameof(id));

                var route = await _context.Routes.FirstOrDefaultAsync(r => r.RouteId == id);

                if (route == null)
                    throw new KeyNotFoundException($"Route {id} not found");

                // Check if route has active trips
                var activeTrips = await _context.Trips
                    .AnyAsync(t => t.RouteId == id && (t.TripStatus == "scheduled" || t.TripStatus == "in_progress"));

                if (activeTrips)
                    throw new InvalidOperationException("Cannot delete route with active trips");

                _context.Routes.Remove(route);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted route: {RouteId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting route: {RouteId}", id);
                throw;
            }
        }

        /// <summary>
        /// Toggle route active status
        /// </summary>
        public async Task<bool> ToggleActiveStatusAsync(int routeId, bool isActive)
        {
            try
            {
                if (routeId <= 0)
                    throw new ArgumentException("Route ID must be greater than 0", nameof(routeId));

                var route = await _context.Routes.FirstOrDefaultAsync(r => r.RouteId == routeId);

                if (route == null)
                    throw new KeyNotFoundException($"Route {routeId} not found");

                route.IsActive = isActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Toggled route status - RouteId: {RouteId}, IsActive: {IsActive}", routeId, isActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling route status: {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Check if route exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Routes.AnyAsync(r => r.RouteId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if route exists: {RouteId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count total routes
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Routes.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting routes");
                throw;
            }
        }
    }
}
