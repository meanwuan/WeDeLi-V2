using wedeli.Models.Domain;
using wedeli.Models.DTO;

namespace wedeli.Repositories.Interface
{
    public interface IVehicleRepository
    {
        // CRUD Operations
        Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
        Task<Vehicle?> GetVehicleByLicensePlateAsync(string licensePlate);
        Task<(List<Vehicle> Vehicles, int TotalCount)> GetVehiclesAsync(VehicleFilterDto filter);
        Task<List<Vehicle>> GetVehiclesByCompanyAsync(int companyId);
        Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
        Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(int vehicleId);

        // Load Management
        Task<Vehicle> UpdateVehicleLoadAsync(int vehicleId, decimal weightKg, bool isAdding = true);
        Task<Vehicle> ResetVehicleLoadAsync(int vehicleId);
        Task<bool> CanAccommodateWeightAsync(int vehicleId, decimal weightKg);
        Task<List<Vehicle>> GetAvailableVehiclesForWeightAsync(decimal weightKg, int? companyId = null);

        // Status Management
        Task<Vehicle> UpdateVehicleStatusAsync(int vehicleId, string newStatus);
        Task<List<Vehicle>> GetVehiclesByStatusAsync(string status, int? companyId = null);
        Task<List<Vehicle>> GetOverloadedVehiclesAsync(int? companyId = null);

        // Statistics
        Task<VehicleStatisticsDto> GetVehicleStatisticsAsync(int vehicleId);
        Task<int> GetVehicleCountByCompanyAsync(int companyId);
        Task<bool> LicensePlateExistsAsync(string licensePlate, int? excludeVehicleId = null);
    }
}