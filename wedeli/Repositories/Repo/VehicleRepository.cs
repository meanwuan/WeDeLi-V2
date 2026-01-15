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
    /// Vehicle repository for managing vehicle data access
    /// </summary>
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;
        private readonly PlatformDbContext _platformContext;
        private readonly ILogger<VehicleRepository> _logger;

        public VehicleRepository(AppDbContext context, PlatformDbContext platformContext, ILogger<VehicleRepository> logger)
        {
            _context = context;
            _platformContext = platformContext;
            _logger = logger;
        }

        /// <summary>
        /// Get vehicle by ID
        /// </summary>
        public async Task<Vehicle> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0", nameof(id));

                var vehicle = await _context.Vehicles
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .FirstOrDefaultAsync(v => v.VehicleId == id);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle {id} not found");

                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle: {VehicleId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get vehicle by license plate
        /// </summary>
        public async Task<Vehicle> GetByLicensePlateAsync(string licensePlate)
        {
            try
            {
                if (string.IsNullOrEmpty(licensePlate))
                    throw new ArgumentException("License plate cannot be empty", nameof(licensePlate));

                var vehicle = await _context.Vehicles
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle with license plate {licensePlate} not found");

                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle by license plate: {LicensePlate}", licensePlate);
                throw;
            }
        }

        /// <summary>
        /// Get vehicles by company
        /// </summary>
        public async Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var vehicles = await _context.Vehicles
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Where(v => v.CompanyId == companyId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();

                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles by company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get available vehicles
        /// </summary>
        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var vehicles = await _context.Vehicles
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Where(v => v.CompanyId == companyId && 
                               v.CurrentStatus == "available" && 
                               (v.CapacityPercentage < v.OverloadThreshold || v.AllowOverload == true))
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();

                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available vehicles: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get overloaded vehicles
        /// </summary>
        public async Task<IEnumerable<Vehicle>> GetOverloadedVehiclesAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var vehicles = await _context.Vehicles
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Where(v => v.CompanyId == companyId && 
                               (v.CurrentStatus == "overloaded" || v.CapacityPercentage > v.OverloadThreshold))
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();

                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overloaded vehicles: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get all vehicles
        /// </summary>
        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();

                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vehicles");
                throw;
            }
        }

        /// <summary>
        /// Add new vehicle
        /// </summary>
        public async Task<Vehicle> AddAsync(Vehicle entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.CompanyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0");

                // Check license plate uniqueness
                var existingVehicle = await _context.Vehicles
                    .AnyAsync(v => v.LicensePlate == entity.LicensePlate);
                if (existingVehicle)
                    throw new InvalidOperationException($"Vehicle with license plate {entity.LicensePlate} already exists");

                // Verify company exists (from Platform database)
                var companyExists = await _platformContext.TransportCompanies
                    .AnyAsync(c => c.CompanyId == entity.CompanyId);
                if (!companyExists)
                    throw new KeyNotFoundException($"Company {entity.CompanyId} not found");

                entity.CreatedAt = DateTime.UtcNow;
                entity.CurrentStatus = "available";
                entity.CurrentWeightKg = 0;
                entity.CapacityPercentage = 0;
                entity.AllowOverload = false;

                _context.Vehicles.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new vehicle: {VehicleId} ({LicensePlate})", entity.VehicleId, entity.LicensePlate);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicle");
                throw;
            }
        }

        /// <summary>
        /// Update vehicle
        /// </summary>
        public async Task<Vehicle> UpdateAsync(Vehicle entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleId == entity.VehicleId);

                if (existingVehicle == null)
                    throw new KeyNotFoundException($"Vehicle {entity.VehicleId} not found");

                // Check license plate uniqueness if changed
                if (!existingVehicle.LicensePlate.Equals(entity.LicensePlate, StringComparison.OrdinalIgnoreCase))
                {
                    var duplicatePlate = await _context.Vehicles
                        .AnyAsync(v => v.LicensePlate == entity.LicensePlate && v.VehicleId != entity.VehicleId);
                    if (duplicatePlate)
                        throw new InvalidOperationException($"Vehicle with license plate {entity.LicensePlate} already exists");
                }

                existingVehicle.LicensePlate = entity.LicensePlate ?? existingVehicle.LicensePlate;
                existingVehicle.VehicleType = entity.VehicleType ?? existingVehicle.VehicleType;
                existingVehicle.MaxWeightKg = entity.MaxWeightKg ?? existingVehicle.MaxWeightKg;
                existingVehicle.MaxVolumeM3 = entity.MaxVolumeM3 ?? existingVehicle.MaxVolumeM3;
                existingVehicle.OverloadThreshold = entity.OverloadThreshold ?? existingVehicle.OverloadThreshold;
                existingVehicle.GpsEnabled = entity.GpsEnabled ?? existingVehicle.GpsEnabled;

                if (!string.IsNullOrEmpty(entity.CurrentStatus))
                    existingVehicle.CurrentStatus = entity.CurrentStatus;

                if (entity.AllowOverload.HasValue)
                    existingVehicle.AllowOverload = entity.AllowOverload;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated vehicle: {VehicleId}", entity.VehicleId);
                return existingVehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle: {VehicleId}", entity?.VehicleId);
                throw;
            }
        }

        /// <summary>
        /// Delete vehicle (soft delete)
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0", nameof(id));

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle {id} not found");

                // Check if vehicle has active trips
                var activeTrips = await _context.Trips
                    .AnyAsync(t => t.VehicleId == id && (t.TripStatus == "scheduled" || t.TripStatus == "in_progress"));

                if (activeTrips)
                    throw new InvalidOperationException("Cannot delete vehicle with active trips");

                vehicle.CurrentStatus = "inactive";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted vehicle: {VehicleId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle: {VehicleId}", id);
                throw;
            }
        }

        /// <summary>
        /// Update vehicle current weight
        /// </summary>
        public async Task<bool> UpdateCurrentWeightAsync(int vehicleId, decimal weightKg)
        {
            try
            {
                if (vehicleId <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0", nameof(vehicleId));

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle {vehicleId} not found");

                vehicle.CurrentWeightKg = weightKg;
                vehicle.CapacityPercentage = (weightKg / vehicle.MaxWeightKg) * 100;

                if (vehicle.CapacityPercentage > vehicle.OverloadThreshold && vehicle.AllowOverload != true)
                    vehicle.CurrentStatus = "overloaded";
                else if (vehicle.CurrentStatus == "overloaded" && vehicle.CapacityPercentage <= vehicle.OverloadThreshold)
                    vehicle.CurrentStatus = "available";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated vehicle weight: {VehicleId}, Weight: {Weight}kg", vehicleId, weightKg);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle weight: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Update vehicle status
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int vehicleId, string status)
        {
            try
            {
                if (vehicleId <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0", nameof(vehicleId));

                if (string.IsNullOrEmpty(status))
                    throw new ArgumentException("Status cannot be empty", nameof(status));

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle {vehicleId} not found");

                vehicle.CurrentStatus = status;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated vehicle status: {VehicleId}, Status: {Status}", vehicleId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle status: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Allow overload for vehicle
        /// </summary>
        public async Task<bool> AllowOverloadAsync(int vehicleId, bool allow)
        {
            try
            {
                if (vehicleId <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0", nameof(vehicleId));

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle {vehicleId} not found");

                vehicle.AllowOverload = allow;

                if (!allow && vehicle.CurrentStatus == "overloaded")
                    vehicle.CurrentStatus = "available";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated overload allowance: {VehicleId}, Allow: {Allow}", vehicleId, allow);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allowing overload: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Check vehicle capacity
        /// </summary>
        public async Task<bool> CheckCapacityAsync(int vehicleId, decimal additionalWeightKg)
        {
            try
            {
                if (vehicleId <= 0)
                    throw new ArgumentException("Vehicle ID must be greater than 0", nameof(vehicleId));

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle {vehicleId} not found");

                var totalWeight = (vehicle.CurrentWeightKg ?? 0) + additionalWeightKg;
                var capacityPercentage = (totalWeight / vehicle.MaxWeightKg) * 100;

                if (capacityPercentage > 100)
                    return false;

                if (capacityPercentage > vehicle.OverloadThreshold && vehicle.AllowOverload != true)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking capacity: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Check if vehicle exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Vehicles.AnyAsync(v => v.VehicleId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if vehicle exists: {VehicleId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count total vehicles
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Vehicles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting vehicles");
                throw;
            }
        }
    }
}
