using wedeli.Models.DTO;

namespace wedeli.service.Interface
{
    public interface IVehicleService
    {
        public Task<VehicleDto?> GetVehicleByIdAsync(int vehicleId);
        public Task<VehicleDto?> GetVehicleByLicensePlateAsync(string licensePlate);
        public Task<(List<VehicleDto> Vehicles, int TotalCount)> GetVehiclesAsync(VehicleFilterDto filter);
        public Task<List<VehicleDto>> GetVehiclesByCompanyAsync(int companyId);
        public Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto);
        public Task<VehicleDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto dto);
        public Task<bool> DeleteVehicleAsync(int vehicleId);
        public Task<VehicleLoadResponseDto> AddWeightToVehicleAsync(int vehicleId, decimal weightKg);
        public Task<VehicleLoadResponseDto> RemoveWeightFromVehicleAsync(int vehicleId, decimal weightKg);
        public  Task<VehicleDto> ResetVehicleLoadAsync(int vehicleId);
        public Task<bool> CanAccommodateWeightAsync(int vehicleId, decimal weightKg);
        public Task<List<VehicleDto>> FindSuitableVehiclesForOrderAsync(decimal weightKg, int? companyId = null);
        public Task<VehicleDto> UpdateVehicleStatusAsync(int vehicleId, string newStatus);
        public Task<List<VehicleDto>> GetAvailableVehiclesAsync(int? companyId = null);
        public Task<List<VehicleDto>> GetOverloadedVehiclesAsync(int? companyId = null);
        public Task<VehicleStatisticsDto> GetVehicleStatisticsAsync(int vehicleId);
        public Task<Dictionary<string, int>> GetVehicleCountByStatusAsync(int? companyId = null);
    }
}
