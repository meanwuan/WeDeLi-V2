using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IVehicleRepository : IBaseRepository<Vehicle>
    {
        Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId);
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(int companyId);
        Task<Vehicle> GetByLicensePlateAsync(string licensePlate);
        Task<bool> UpdateCurrentWeightAsync(int vehicleId, decimal weightKg);
        Task<bool> UpdateStatusAsync(int vehicleId, string status);
        Task<bool> AllowOverloadAsync(int vehicleId, bool allow);
        Task<IEnumerable<Vehicle>> GetOverloadedVehiclesAsync(int companyId);
        Task<bool> CheckCapacityAsync(int vehicleId, decimal additionalWeightKg);
    }
}
