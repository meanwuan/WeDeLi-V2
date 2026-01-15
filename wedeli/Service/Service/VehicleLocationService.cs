using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Vehicle;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service;

public class VehicleLocationService : IVehicleLocationService
{
    private readonly IVehicleLocationRepository _locationRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITransportCompanyRepository _companyRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ILogger<VehicleLocationService> _logger;

    public VehicleLocationService(
        IVehicleLocationRepository locationRepository,
        IVehicleRepository vehicleRepository,
        ITransportCompanyRepository companyRepository,
        IDriverRepository driverRepository,
        ILogger<VehicleLocationService> logger)
    {
        _locationRepository = locationRepository;
        _vehicleRepository = vehicleRepository;
        _companyRepository = companyRepository;
        _driverRepository = driverRepository;
        _logger = logger;
    }

    public async Task<VehicleLocationDto?> GetLatestLocationAsync(int vehicleId)
    {
        var location = await _locationRepository.GetLatestByVehicleIdAsync(vehicleId);
        if (location == null)
        {
            // Xe chưa có vị trí, trả về thông tin cơ bản
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return null;

            return new VehicleLocationDto
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                VehicleType = vehicle.VehicleType,
                CompanyId = vehicle.CompanyId,
                CurrentStatus = vehicle.CurrentStatus,
                Status = "Offline"
            };
        }

        return MapToDto(location);
    }

    public async Task<CompanyVehicleLocationsDto> GetCompanyVehicleLocationsAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
        {
            throw new ArgumentException($"Company {companyId} not found");
        }

        var locations = await _locationRepository.GetLatestByCompanyIdAsync(companyId);
        var allVehicles = await _vehicleRepository.GetByCompanyIdAsync(companyId);

        var vehicleLocationDtos = new List<VehicleLocationDto>();
        var onlineCount = 0;

        foreach (var vehicle in allVehicles)
        {
            var location = locations.FirstOrDefault(l => l.VehicleId == vehicle.VehicleId);
            
            // Kiểm tra xe online (có cập nhật trong 5 phút gần)
            var isOnline = location != null && location.RecordedAt > DateTime.UtcNow.AddMinutes(-5);
            if (isOnline) onlineCount++;

            if (location != null)
            {
                vehicleLocationDtos.Add(MapToDto(location));
            }
            else
            {
                // Xe chưa có vị trí
                vehicleLocationDtos.Add(new VehicleLocationDto
                {
                    VehicleId = vehicle.VehicleId,
                    LicensePlate = vehicle.LicensePlate,
                    VehicleType = vehicle.VehicleType,
                    CompanyId = vehicle.CompanyId,
                    CompanyName = company.CompanyName,
                    CurrentStatus = vehicle.CurrentStatus,
                    Status = "Offline"
                });
            }
        }

        return new CompanyVehicleLocationsDto
        {
            CompanyId = companyId,
            CompanyName = company.CompanyName ?? "",
            TotalVehicles = allVehicles.Count(),
            OnlineVehicles = onlineCount,
            Vehicles = vehicleLocationDtos
        };
    }

    public async Task<VehicleLocationDto> UpdateLocationAsync(UpdateVehicleLocationDto locationDto)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(locationDto.VehicleId);
        if (vehicle == null)
        {
            throw new ArgumentException($"Vehicle {locationDto.VehicleId} not found");
        }

        var location = new VehicleLocation
        {
            VehicleId = locationDto.VehicleId,
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            Speed = locationDto.Speed,
            Heading = locationDto.Heading,
            Accuracy = locationDto.Accuracy,
            Status = locationDto.Status ?? DetermineStatus(locationDto.Speed),
            RecordedAt = DateTime.UtcNow
        };

        var saved = await _locationRepository.UpsertAsync(location);
        
        _logger.LogInformation(
            "Vehicle {VehicleId} location updated: ({Lat}, {Lng})", 
            locationDto.VehicleId, 
            locationDto.Latitude, 
            locationDto.Longitude);

        // Reload with navigation properties
        var result = await _locationRepository.GetLatestByVehicleIdAsync(saved.VehicleId);
        return MapToDto(result!);
    }

    public async Task<IEnumerable<VehicleLocationDto>> GetLocationHistoryAsync(int vehicleId, DateTime from, DateTime to)
    {
        var history = await _locationRepository.GetHistoryAsync(vehicleId, from, to);
        return history.Select(MapToDto);
    }

    private VehicleLocationDto MapToDto(VehicleLocation location)
    {
        return new VehicleLocationDto
        {
            VehicleId = location.VehicleId,
            LicensePlate = location.Vehicle?.LicensePlate ?? "",
            VehicleType = location.Vehicle?.VehicleType ?? "",
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Speed = location.Speed,
            Heading = location.Heading,
            Accuracy = location.Accuracy,
            Status = location.Status,
            RecordedAt = location.RecordedAt,
            CompanyId = location.Vehicle?.CompanyId ?? 0,
            CompanyName = location.Vehicle?.Company?.CompanyName,
            CurrentStatus = location.Vehicle?.CurrentStatus
        };
    }

    private string DetermineStatus(decimal? speed)
    {
        if (speed == null || speed < 1) return "Stopped";
        if (speed < 5) return "Idle";
        return "Moving";
    }
}
