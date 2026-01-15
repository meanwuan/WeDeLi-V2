using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Vehicle;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Vehicle service interface for vehicle management
    /// </summary>
    public interface IVehicleService
    {
        Task<VehicleResponseDto> GetVehicleByIdAsync(int vehicleId);
        Task<VehicleResponseDto> GetVehicleByLicensePlateAsync(string licensePlate);
        Task<IEnumerable<VehicleResponseDto>> GetVehiclesByCompanyAsync(int companyId);
        Task<IEnumerable<VehicleResponseDto>> GetAvailableVehiclesAsync(int companyId);
        Task<IEnumerable<VehicleResponseDto>> GetOverloadedVehiclesAsync(int companyId);
        Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto dto);
        Task<VehicleResponseDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto dto);
        Task<bool> DeleteVehicleAsync(int vehicleId);
        Task<bool> UpdateVehicleStatusAsync(int vehicleId, string status);
        Task<bool> UpdateVehicleWeightAsync(int vehicleId, decimal weightKg);
        Task<bool> AllowOverloadAsync(int vehicleId, bool allow, int? approvedBy = null);
        Task<VehicleCapacityDto> GetVehicleCapacityAsync(int vehicleId);
        Task<bool> CheckCapacityAsync(int vehicleId, decimal additionalWeightKg);
    }
}
