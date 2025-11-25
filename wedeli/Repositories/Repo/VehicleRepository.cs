using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleRepository> _logger;

        public VehicleRepository(AppDbContext context, ILogger<VehicleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Company)
                    .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle by ID: {vehicleId}");
                throw;
            }
        }

        public async Task<Vehicle?> GetVehicleByLicensePlateAsync(string licensePlate)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Company)
                    .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle by license plate: {licensePlate}");
                throw;
            }
        }

        public async Task<(List<Vehicle> Vehicles, int TotalCount)> GetVehiclesAsync(VehicleFilterDto filter)
        {
            try
            {
                var query = _context.Vehicles
                    .Include(v => v.Company)
                    .AsQueryable();

                if (filter.CompanyId.HasValue)
                    query = query.Where(v => v.CompanyId == filter.CompanyId.Value);

                if (!string.IsNullOrEmpty(filter.VehicleType))
                    query = query.Where(v => v.VehicleType == filter.VehicleType);

                if (!string.IsNullOrEmpty(filter.CurrentStatus))
                    query = query.Where(v => v.CurrentStatus == filter.CurrentStatus);

                if (filter.GpsEnabled.HasValue)
                    query = query.Where(v => v.GpsEnabled == filter.GpsEnabled.Value);

                if (filter.ShowOnlyOverloaded == true)
                    query = query.Where(v => v.CapacityPercentage >= v.OverloadThreshold);

                if (filter.ShowOnlyAvailable == true)
                    query = query.Where(v => v.CurrentStatus == "available");

                var totalCount = await query.CountAsync();

                var vehicles = await query
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (vehicles, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles with filters");
                throw;
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByCompanyAsync(int companyId)
        {
            try
            {
                return await _context.Vehicles
                    .Where(v => v.CompanyId == companyId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicles for company: {companyId}");
                throw;
            }
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                vehicle.CreatedAt = DateTime.Now;
                vehicle.CurrentWeightKg = 0;
                vehicle.CapacityPercentage = 0;
                vehicle.CurrentStatus = "available";

                await _context.Vehicles.AddAsync(vehicle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Vehicle created: {vehicle.LicensePlate}");
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                throw;
            }
        }

        public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Vehicle updated: {vehicle.VehicleId}");
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle: {vehicle.VehicleId}");
                throw;
            }
        }

        public async Task<bool> DeleteVehicleAsync(int vehicleId)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                    return false;

                vehicle.CurrentStatus = "inactive";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Vehicle deleted (soft): {vehicleId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vehicle: {vehicleId}");
                throw;
            }
        }

        public async Task<Vehicle> UpdateVehicleLoadAsync(int vehicleId, decimal weightKg, bool isAdding = true)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");

                // Handle nullable CurrentWeightKg
                var currentWeight = vehicle.CurrentWeightKg ?? 0;

                if (isAdding)
                    vehicle.CurrentWeightKg = currentWeight + weightKg;
                else
                    vehicle.CurrentWeightKg = Math.Max(0, currentWeight - weightKg);

                // Calculate capacity percentage
                if (vehicle.MaxWeightKg.HasValue && vehicle.MaxWeightKg > 0)
                {
                    vehicle.CapacityPercentage = Math.Round(
                        (vehicle.CurrentWeightKg.Value / vehicle.MaxWeightKg.Value) * 100, 2);
                }

                // Auto-update status based on load
                var threshold = vehicle.OverloadThreshold ?? 95.00M;
                var capacity = vehicle.CapacityPercentage ?? 0;

                if (capacity >= threshold)
                    vehicle.CurrentStatus = "overloaded";
                else if (vehicle.CurrentStatus == "overloaded" && capacity < threshold)
                    vehicle.CurrentStatus = "available";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Vehicle load updated: {vehicleId}, Weight: {vehicle.CurrentWeightKg}kg");
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle load: {vehicleId}");
                throw;
            }
        }

        public async Task<Vehicle> ResetVehicleLoadAsync(int vehicleId)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");

                vehicle.CurrentWeightKg = 0;
                vehicle.CapacityPercentage = 0;

                if (vehicle.CurrentStatus == "overloaded")
                    vehicle.CurrentStatus = "available";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Vehicle load reset: {vehicleId}");
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting vehicle load: {vehicleId}");
                throw;
            }
        }

        public async Task<bool> CanAccommodateWeightAsync(int vehicleId, decimal weightKg)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null || !vehicle.MaxWeightKg.HasValue)
                    return false;

                var currentWeight = vehicle.CurrentWeightKg ?? 0;
                var newTotalWeight = currentWeight + weightKg;
                var newCapacityPercentage = (newTotalWeight / vehicle.MaxWeightKg.Value) * 100;

                var threshold = vehicle.OverloadThreshold ?? 95.00M;
                var allowOverload = vehicle.AllowOverload ?? false;

                if (newCapacityPercentage >= threshold && !allowOverload)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking vehicle capacity: {vehicleId}");
                throw;
            }
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesForWeightAsync(decimal weightKg, int? companyId = null)
        {
            try
            {
                var query = _context.Vehicles
                    .Where(v => v.CurrentStatus == "available" && v.MaxWeightKg.HasValue);

                if (companyId.HasValue)
                    query = query.Where(v => v.CompanyId == companyId.Value);

                var vehicles = await query.ToListAsync();

                var availableVehicles = vehicles
                    .Where(v =>
                    {
                        var currentWeight = v.CurrentWeightKg ?? 0;
                        var newTotalWeight = currentWeight + weightKg;
                        var newCapacity = (newTotalWeight / v.MaxWeightKg!.Value) * 100;
                        var threshold = v.OverloadThreshold ?? 95.00M;
                        var allowOverload = v.AllowOverload ?? false;
                        return newCapacity < threshold || allowOverload;
                    })
                    .OrderBy(v => v.CapacityPercentage ?? 0)
                    .ToList();

                return availableVehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available vehicles for weight");
                throw;
            }
        }

        public async Task<Vehicle> UpdateVehicleStatusAsync(int vehicleId, string newStatus)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");

                var validStatuses = new[] { "available", "in_transit", "maintenance", "inactive", "overloaded" };
                if (!validStatuses.Contains(newStatus))
                    throw new ArgumentException($"Invalid status: {newStatus}");

                vehicle.CurrentStatus = newStatus;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Vehicle status updated: {vehicleId} -> {newStatus}");
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle status: {vehicleId}");
                throw;
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                var query = _context.Vehicles
                    .Include(v => v.Company)
                    .Where(v => v.CurrentStatus == status);

                if (companyId.HasValue)
                    query = query.Where(v => v.CompanyId == companyId.Value);

                return await query.OrderBy(v => v.LicensePlate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicles by status: {status}");
                throw;
            }
        }

        public async Task<List<Vehicle>> GetOverloadedVehiclesAsync(int? companyId = null)
        {
            try
            {
                var query = _context.Vehicles
                    .Include(v => v.Company)
                    .Where(v => v.CapacityPercentage >= v.OverloadThreshold);

                if (companyId.HasValue)
                    query = query.Where(v => v.CompanyId == companyId.Value);

                return await query.OrderByDescending(v => v.CapacityPercentage).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overloaded vehicles");
                throw;
            }
        }

        public async Task<VehicleStatisticsDto> GetVehicleStatisticsAsync(int vehicleId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Trips)
                    .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");

                var completedTrips = vehicle.Trips.Where(t => t.TripStatus == "completed").ToList();

                var avgLoadPercentage = 0M;
                if (completedTrips.Any() && vehicle.MaxWeightKg.HasValue && vehicle.MaxWeightKg > 0)
                {
                    avgLoadPercentage = completedTrips.Average(t =>
                        (t.TotalWeightKg ?? 0) / vehicle.MaxWeightKg.Value * 100);
                }

                var createdAt = vehicle.CreatedAt ?? DateTime.Now;

                return new VehicleStatisticsDto
                {
                    VehicleId = vehicle.VehicleId,
                    LicensePlate = vehicle.LicensePlate,
                    TotalTrips = completedTrips.Count,
                    TotalOrdersDelivered = completedTrips.Sum(t => t.TotalOrders ?? 0),
                    TotalDistanceKm = 0,
                    AverageLoadPercentage = avgLoadPercentage,
                    DaysInService = (DateTime.Now - createdAt).Days,
                    LastTripDate = completedTrips.OrderByDescending(t => t.TripDate).FirstOrDefault()?.TripDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle statistics: {vehicleId}");
                throw;
            }
        }

        public async Task<int> GetVehicleCountByCompanyAsync(int companyId)
        {
            try
            {
                return await _context.Vehicles.CountAsync(v => v.CompanyId == companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle count for company: {companyId}");
                throw;
            }
        }

        public async Task<bool> LicensePlateExistsAsync(string licensePlate, int? excludeVehicleId = null)
        {
            try
            {
                var query = _context.Vehicles.Where(v => v.LicensePlate == licensePlate);

                if (excludeVehicleId.HasValue)
                    query = query.Where(v => v.VehicleId != excludeVehicleId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking license plate existence: {licensePlate}");
                throw;
            }
        }
    }
}