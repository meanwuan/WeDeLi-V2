using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface;

public interface IVehicleLocationRepository
{
    /// <summary>
    /// Lấy vị trí mới nhất của xe
    /// </summary>
    Task<VehicleLocation?> GetLatestByVehicleIdAsync(int vehicleId);
    
    /// <summary>
    /// Lấy vị trí mới nhất của tất cả xe của công ty
    /// </summary>
    Task<IEnumerable<VehicleLocation>> GetLatestByCompanyIdAsync(int companyId);
    
    /// <summary>
    /// Thêm vị trí mới
    /// </summary>
    Task<VehicleLocation> AddAsync(VehicleLocation location);
    
    /// <summary>
    /// Cập nhật hoặc thêm vị trí (upsert logic)
    /// </summary>
    Task<VehicleLocation> UpsertAsync(VehicleLocation location);
    
    /// <summary>
    /// Lấy lịch sử vị trí của xe trong khoảng thời gian
    /// </summary>
    Task<IEnumerable<VehicleLocation>> GetHistoryAsync(int vehicleId, DateTime from, DateTime to);
    
    /// <summary>
    /// Xóa lịch sử vị trí cũ (data retention)
    /// </summary>
    Task<int> DeleteOldRecordsAsync(DateTime before);
}
