using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Trip repository for managing trip data access
    /// </summary>
    public class TripRepository : ITripRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TripRepository> _logger;

        public TripRepository(AppDbContext context, ILogger<TripRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // IBaseRepository Implementation

        /// <summary>
        /// Get trip by ID
        /// </summary>
        public async Task<Trip> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Trip ID must be greater than 0");

                var trip = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser)
                    .Include(t => t.TripOrders)
                        .ThenInclude(to => to.Order)
                    .FirstOrDefaultAsync(t => t.TripId == id);

                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {id}");

                _logger.LogInformation("Retrieved trip: {TripId}", id);
                return trip;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip: {TripId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all trips
        /// </summary>
        public async Task<IEnumerable<Trip>> GetAllAsync()
        {
            try
            {
                var trips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} trips", trips.Count);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all trips");
                throw;
            }
        }

        /// <summary>
        /// Add new trip
        /// </summary>
        public async Task<Trip> AddAsync(Trip entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                entity.CreatedAt = DateTime.UtcNow;
                _context.Trips.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created trip: {TripId}", entity.TripId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                throw;
            }
        }

        /// <summary>
        /// Update trip
        /// </summary>
        public async Task<Trip> UpdateAsync(Trip entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.TripId <= 0)
                    throw new ArgumentException("Trip ID must be greater than 0");

                var existingTrip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == entity.TripId);
                if (existingTrip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {entity.TripId}");

                _context.Entry(existingTrip).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated trip: {TripId}", entity.TripId);
                return existingTrip;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip: {TripId}", entity?.TripId);
                throw;
            }
        }

        /// <summary>
        /// Delete trip
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Trip ID must be greater than 0");

                var trip = await _context.Trips
                    .Include(t => t.TripOrders)
                    .FirstOrDefaultAsync(t => t.TripId == id);

                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {id}");

                // Check if trip has active orders
                if (trip.TripOrders != null && trip.TripOrders.Any())
                    throw new InvalidOperationException("Cannot delete trip with assigned orders");

                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted trip: {TripId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trip: {TripId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if trip exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Trips.AnyAsync(t => t.TripId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking trip existence: {TripId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count all trips
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Trips.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting trips");
                throw;
            }
        }

        // ITripRepository Implementation

        /// <summary>
        /// Get trips by route
        /// </summary>
        public async Task<IEnumerable<Trip>> GetByRouteIdAsync(int routeId)
        {
            try
            {
                if (routeId <= 0)
                    throw new ArgumentException("Route ID must be greater than 0");

                var trips = await _context.Trips
                    .Where(t => t.RouteId == routeId)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser)
                    .OrderByDescending(t => t.TripDate)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} trips for route: {RouteId}", trips.Count, routeId);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by route: {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Get trips by vehicle
        /// </summary>
        public async Task<IEnumerable<Trip>> GetByVehicleIdAsync(int vehicleId)
        {
            try
            {
                if (vehicleId <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0");

                var trips = await _context.Trips
                    .Where(t => t.VehicleId == vehicleId)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser)
                    .OrderByDescending(t => t.TripDate)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} trips for vehicle: {VehicleId}", trips.Count, vehicleId);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by vehicle: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Get trips by driver
        /// </summary>
        public async Task<IEnumerable<Trip>> GetByDriverIdAsync(int driverId)
        {
            try
            {
                if (driverId <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0");

                var trips = await _context.Trips
                    .Where(t => t.DriverId == driverId)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser)
                    .OrderByDescending(t => t.TripDate)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} trips for driver: {DriverId}", trips.Count, driverId);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get trips by date
        /// </summary>
        public async Task<IEnumerable<Trip>> GetByDateAsync(DateTime date, int? companyId = null)
        {
            try
            {
                var query = _context.Trips
                    .Where(t => t.TripDate == DateOnly.FromDateTime(date))
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser);

                if (companyId.HasValue && companyId > 0)
                {
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Trip, CompanyUser>)query.Where(t => t.Route.CompanyId == companyId);
                }

                var trips = await query
                    .OrderBy(t => t.DepartureTime)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} trips for date: {Date}", trips.Count, date.Date);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by date: {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// Get trips by status
        /// </summary>
        public async Task<IEnumerable<Trip>> GetByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    throw new ArgumentException("Status cannot be empty");

                var query = _context.Trips
                    .Where(t => t.TripStatus == status)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser);

                if (companyId.HasValue && companyId > 0)
                {
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Trip, CompanyUser>)query.Where(t => t.Route.CompanyId == companyId);
                }

                var trips = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} trips with status: {Status}", trips.Count, status);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Update trip status
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int tripId, string newStatus)
        {
            try
            {
                if (tripId <= 0)
                    throw new ArgumentException("Trip ID must be greater than 0");

                if (string.IsNullOrEmpty(newStatus))
                    throw new ArgumentException("Status cannot be empty");

                var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                trip.TripStatus = newStatus;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated trip {TripId} status to: {Status}", tripId, newStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip status: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Update trip times
        /// </summary>
        public async Task<bool> UpdateTripTimesAsync(int tripId, DateTime? departureTime = null, DateTime? arrivalTime = null)
        {
            try
            {
                if (tripId <= 0)
                    throw new ArgumentException("Trip ID must be greater than 0");

                var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                if (departureTime.HasValue)
                    trip.DepartureTime = departureTime;

                if (arrivalTime.HasValue)
                    trip.ArrivalTime = arrivalTime;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated trip {TripId} times", tripId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip times: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Update trip statistics
        /// </summary>
        public async Task<bool> UpdateTripStatisticsAsync(int tripId, int totalOrders, decimal totalWeightKg)
        {
            try
            {
                if (tripId <= 0)
                    throw new ArgumentException("Trip ID must be greater than 0");

                if (totalOrders < 0)
                    throw new ArgumentException("Total orders cannot be negative");

                if (totalWeightKg < 0)
                    throw new ArgumentException("Total weight cannot be negative");

                var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                trip.TotalOrders = totalOrders;
                trip.TotalWeightKg = totalWeightKg;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated trip {TripId} statistics: Orders={Orders}, Weight={Weight}kg", 
                    tripId, totalOrders, totalWeightKg);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip statistics: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Get active trips
        /// </summary>
        public async Task<IEnumerable<Trip>> GetActiveTripsAsync(int? companyId = null)
        {
            try
            {
                var query = _context.Trips
                    .Where(t => t.TripStatus == "scheduled" || t.TripStatus == "in_progress")
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser);

                if (companyId.HasValue && companyId > 0)
                {
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Trip, CompanyUser>)query.Where(t => t.Route.CompanyId == companyId);
                }

                var trips = await query
                    .OrderBy(t => t.TripDate)
                    .ThenBy(t => t.DepartureTime)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} active trips", trips.Count);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active trips");
                throw;
            }
        }

        /// <summary>
        /// Get return trips
        /// </summary>
        public async Task<IEnumerable<Trip>> GetReturnTripsAsync(int? companyId = null)
        {
            try
            {
                var query = _context.Trips
                    .Where(t => t.IsReturnTrip == true)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Company)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.CompanyUser);

                if (companyId.HasValue && companyId > 0)
                {
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Trip, CompanyUser>)query.Where(t => t.Route.CompanyId == companyId);
                }

                var trips = await query
                    .OrderByDescending(t => t.TripDate)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} return trips", trips.Count);
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving return trips");
                throw;
            }
        }
    }
}
