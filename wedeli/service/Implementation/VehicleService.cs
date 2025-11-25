using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    /// <summary>
    /// Service implementation for vehicle business logic
    /// </summary>
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(IVehicleRepository vehicleRepository, ILogger<VehicleService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        // ===== CRUD Operations =====

        public async Task<VehicleDto?> GetVehicleByIdAsync(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
            return vehicle == null ? null : MapToDto(vehicle);
        }

        public async Task<VehicleDto?> GetVehicleByLicensePlateAsync(string licensePlate)
        {
            var vehicle = await _vehicleRepository.GetVehicleByLicensePlateAsync(licensePlate);
            return vehicle == null ? null : MapToDto(vehicle);
        }

        public async Task<(List<VehicleDto> Vehicles, int TotalCount)> GetVehiclesAsync(VehicleFilterDto filter)
        {
            var (vehicles, totalCount) = await _vehicleRepository.GetVehiclesAsync(filter);
            var vehicleDtos = vehicles.Select(MapToDto).ToList();
            return (vehicleDtos, totalCount);
        }

        public async Task<List<VehicleDto>> GetVehiclesByCompanyAsync(int companyId)
        {
            var vehicles = await _vehicleRepository.GetVehiclesByCompanyAsync(companyId);
            return vehicles.Select(MapToDto).ToList();
        }

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
        {
            // Validate license plate uniqueness
            if (await _vehicleRepository.LicensePlateExistsAsync(dto.LicensePlate))
            {
                throw new InvalidOperationException($"License plate already exists: {dto.LicensePlate}");
            }

            // Validate vehicle type
            var validTypes = new[] { "truck", "van", "motorbike" };
            if (!validTypes.Contains(dto.VehicleType))
            {
                throw new ArgumentException($"Invalid vehicle type. Must be one of: {string.Join(", ", validTypes)}");
            }

            var vehicle = new Vehicle
            {
                CompanyId = dto.CompanyId,
                LicensePlate = dto.LicensePlate.ToUpper().Trim(),
                VehicleType = dto.VehicleType,
                MaxWeightKg = dto.MaxWeightKg,
                MaxVolumeM3 = dto.MaxVolumeM3,
                CurrentWeightKg = 0,
                CapacityPercentage = 0,
                OverloadThreshold = dto.OverloadThreshold,
                AllowOverload = dto.AllowOverload,
                CurrentStatus = "available",
                GpsEnabled = dto.GpsEnabled
            };

            var createdVehicle = await _vehicleRepository.CreateVehicleAsync(vehicle);
            _logger.LogInformation($"Vehicle created successfully: {createdVehicle.LicensePlate}");

            return MapToDto(createdVehicle);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto dto)
        {
            var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.LicensePlate))
            {
                // Check if new license plate already exists
                if (await _vehicleRepository.LicensePlateExistsAsync(dto.LicensePlate, vehicleId))
                {
                    throw new InvalidOperationException($"License plate already exists: {dto.LicensePlate}");
                }
                vehicle.LicensePlate = dto.LicensePlate.ToUpper().Trim();
            }

            if (!string.IsNullOrEmpty(dto.VehicleType))
                vehicle.VehicleType = dto.VehicleType;

            if (dto.MaxWeightKg.HasValue)
                vehicle.MaxWeightKg = dto.MaxWeightKg.Value;

            if (dto.MaxVolumeM3.HasValue)
                vehicle.MaxVolumeM3 = dto.MaxVolumeM3.Value;

            if (dto.CurrentWeightKg.HasValue)
                vehicle.CurrentWeightKg = dto.CurrentWeightKg.Value;

            if (!string.IsNullOrEmpty(dto.CurrentStatus))
                vehicle.CurrentStatus = dto.CurrentStatus;

            if (dto.OverloadThreshold.HasValue)
                vehicle.OverloadThreshold = dto.OverloadThreshold.Value;

            if (dto.AllowOverload.HasValue)
                vehicle.AllowOverload = dto.AllowOverload.Value;

            if (dto.GpsEnabled.HasValue)
                vehicle.GpsEnabled = dto.GpsEnabled.Value;

            // Recalculate capacity percentage
            if (vehicle.MaxWeightKg.HasValue && vehicle.MaxWeightKg > 0)
            {
                vehicle.CapacityPercentage = Math.Round(
                    (decimal)((vehicle.CurrentWeightKg / vehicle.MaxWeightKg.Value) * 100),
                    2
                );
            }

            var updatedVehicle = await _vehicleRepository.UpdateVehicleAsync(vehicle);
            _logger.LogInformation($"Vehicle updated successfully: {vehicleId}");

            return MapToDto(updatedVehicle);
        }

        public async Task<bool> DeleteVehicleAsync(int vehicleId)
        {
            var deleted = await _vehicleRepository.DeleteVehicleAsync(vehicleId);
            if (deleted)
            {
                _logger.LogInformation($"Vehicle deleted successfully: {vehicleId}");
            }
            return deleted;
        }

        // ===== Load Management =====

        public async Task<VehicleLoadResponseDto> AddWeightToVehicleAsync(int vehicleId, decimal weightKg)
        {
            if (weightKg <= 0)
            {
                throw new ArgumentException("Weight must be greater than zero");
            }

            var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");
            }

            var oldWeight = vehicle.CurrentWeightKg;

            // Check if can accommodate
            if (!await _vehicleRepository.CanAccommodateWeightAsync(vehicleId, weightKg))
            {
                throw new InvalidOperationException(
                    $"Vehicle {vehicle.LicensePlate} cannot accommodate {weightKg}kg. " +
                    $"Current: {vehicle.CurrentWeightKg}kg, Max: {vehicle.MaxWeightKg}kg, " +
                    $"Threshold: {vehicle.OverloadThreshold}%"
                );
            }

            var updatedVehicle = await _vehicleRepository.UpdateVehicleLoadAsync(vehicleId, weightKg, isAdding: true);

            return new VehicleLoadResponseDto
            {
                VehicleId = vehicleId,
                OldWeightKg = (decimal)oldWeight,
                NewWeightKg = (decimal)updatedVehicle.CurrentWeightKg,
                CapacityPercentage = (decimal)updatedVehicle.CapacityPercentage,
                IsOverloaded = updatedVehicle.CapacityPercentage >= updatedVehicle.OverloadThreshold,
                CurrentStatus = updatedVehicle.CurrentStatus,
                Message = $"Added {weightKg}kg to vehicle. New load: {updatedVehicle.CurrentWeightKg}kg ({updatedVehicle.CapacityPercentage}%)",
                CanAddMoreOrders = updatedVehicle.CapacityPercentage < (updatedVehicle.OverloadThreshold ?? 95.00M) || (updatedVehicle.AllowOverload ?? false)
            };
        }

        public async Task<VehicleLoadResponseDto> RemoveWeightFromVehicleAsync(int vehicleId, decimal weightKg)
        {
            if (weightKg <= 0)
            {
                throw new ArgumentException("Weight must be greater than zero");
            }

            var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");
            }

            var oldWeight = vehicle.CurrentWeightKg;
            var updatedVehicle = await _vehicleRepository.UpdateVehicleLoadAsync(vehicleId, weightKg, isAdding: false);

            return new VehicleLoadResponseDto
            {
                VehicleId = vehicleId,
                OldWeightKg = (decimal)oldWeight,
                NewWeightKg = (decimal)updatedVehicle.CurrentWeightKg,
                CapacityPercentage = (decimal)updatedVehicle.CapacityPercentage,
                IsOverloaded = updatedVehicle.CapacityPercentage >= updatedVehicle.OverloadThreshold,
                CurrentStatus = updatedVehicle.CurrentStatus,
                Message = $"Removed {weightKg}kg from vehicle. New load: {updatedVehicle.CurrentWeightKg}kg ({updatedVehicle.CapacityPercentage}%)",
                CanAddMoreOrders = updatedVehicle.CapacityPercentage < (updatedVehicle.OverloadThreshold ?? 95.00M) || (updatedVehicle.AllowOverload ?? false)
            };
        }

        public async Task<VehicleDto> ResetVehicleLoadAsync(int vehicleId)
        {
            var vehicle = await _vehicleRepository.ResetVehicleLoadAsync(vehicleId);
            _logger.LogInformation($"Vehicle load reset: {vehicleId}");
            return MapToDto(vehicle);
        }

        public async Task<bool> CanAccommodateWeightAsync(int vehicleId, decimal weightKg)
        {
            return await _vehicleRepository.CanAccommodateWeightAsync(vehicleId, weightKg);
        }

        public async Task<List<VehicleDto>> FindSuitableVehiclesForOrderAsync(decimal weightKg, int? companyId = null)
        {
            var vehicles = await _vehicleRepository.GetAvailableVehiclesForWeightAsync(weightKg, companyId);
            return vehicles.Select(MapToDto).ToList();
        }

        // ===== Status Management =====

        public async Task<VehicleDto> UpdateVehicleStatusAsync(int vehicleId, string newStatus)
        {
            var vehicle = await _vehicleRepository.UpdateVehicleStatusAsync(vehicleId, newStatus);
            _logger.LogInformation($"Vehicle status updated: {vehicleId} -> {newStatus}");
            return MapToDto(vehicle);
        }

        public async Task<List<VehicleDto>> GetAvailableVehiclesAsync(int? companyId = null)
        {
            var vehicles = await _vehicleRepository.GetVehiclesByStatusAsync("available", companyId);
            return vehicles.Select(MapToDto).ToList();
        }

        public async Task<List<VehicleDto>> GetOverloadedVehiclesAsync(int? companyId = null)
        {
            var vehicles = await _vehicleRepository.GetOverloadedVehiclesAsync(companyId);
            return vehicles.Select(MapToDto).ToList();
        }

        // ===== Statistics & Reports =====

        public async Task<VehicleStatisticsDto> GetVehicleStatisticsAsync(int vehicleId)
        {
            return await _vehicleRepository.GetVehicleStatisticsAsync(vehicleId);
        }

        public async Task<Dictionary<string, int>> GetVehicleCountByStatusAsync(int? companyId = null)
        {
            var filter = new VehicleFilterDto { CompanyId = companyId, PageSize = 1000 };
            var (vehicles, _) = await _vehicleRepository.GetVehiclesAsync(filter);

            return vehicles
                .GroupBy(v => v.CurrentStatus)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // ===== Helper Methods =====

        private VehicleDto MapToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                VehicleId = vehicle.VehicleId,
                CompanyId = vehicle.CompanyId,
                CompanyName = vehicle.Company?.CompanyName,
                LicensePlate = vehicle.LicensePlate,
                VehicleType = vehicle.VehicleType,
                MaxWeightKg = vehicle.MaxWeightKg,
                MaxVolumeM3 = vehicle.MaxVolumeM3,
                CurrentWeightKg = (decimal)vehicle.CurrentWeightKg,
                CapacityPercentage = (decimal)vehicle.CapacityPercentage,
                OverloadThreshold = (decimal)vehicle.OverloadThreshold,
                AllowOverload = (bool)vehicle.AllowOverload,
                CurrentStatus = vehicle.CurrentStatus,
                GpsEnabled = (bool)vehicle.GpsEnabled,
                CreatedAt = (DateTime)vehicle.CreatedAt
            };
        }
    }
}