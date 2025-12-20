using wedeli.Models.Domain;
using wedeli.Models.DTO.Vehicle;

namespace wedeli.Service.Interface;

public interface IVehicleLocationService
{
    /// <summary>
    /// Lấy vị trí mới nhất của xe
    /// </summary>
    Task<VehicleLocationDto?> GetLatestLocationAsync(int vehicleId);
    
    /// <summary>
    /// Lấy vị trí tất cả xe của công ty
    /// </summary>
    Task<CompanyVehicleLocationsDto> GetCompanyVehicleLocationsAsync(int companyId);
    
    /// <summary>
    /// Cập nhật vị trí xe (từ driver app)
    /// </summary>
    Task<VehicleLocationDto> UpdateLocationAsync(UpdateVehicleLocationDto locationDto);
    
    /// <summary>
    /// Lấy lịch sử vị trí xe
    /// </summary>
    Task<IEnumerable<VehicleLocationDto>> GetLocationHistoryAsync(int vehicleId, DateTime from, DateTime to);
}
