using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Vehicle;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Vehicle service for managing vehicle operations
    /// </summary>
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ITransportCompanyRepository _companyRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(
            IVehicleRepository vehicleRepository,
            ITransportCompanyRepository companyRepository,
            IOrderRepository orderRepository,
            ITripRepository tripRepository,
            IMapper mapper,
            ILogger<VehicleService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _companyRepository = companyRepository;
            _orderRepository = orderRepository;
            _tripRepository = tripRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get vehicle by ID
        /// </summary>
        public async Task<VehicleResponseDto> GetVehicleByIdAsync(int vehicleId)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                var vehicleDto = _mapper.Map<VehicleResponseDto>(vehicle);

                // Enrich with company name from Platform DB
                if (vehicle.CompanyId > 0)
                {
                    var company = await _companyRepository.GetByIdAsync(vehicle.CompanyId);
                    vehicleDto.CompanyName = company?.CompanyName;
                }

                _logger.LogInformation("Retrieved vehicle: {VehicleId}", vehicleId);
                return vehicleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Get vehicle by license plate
        /// </summary>
        public async Task<VehicleResponseDto> GetVehicleByLicensePlateAsync(string licensePlate)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByLicensePlateAsync(licensePlate);
                var vehicleDto = _mapper.Map<VehicleResponseDto>(vehicle);

                _logger.LogInformation("Retrieved vehicle by license plate: {LicensePlate}", licensePlate);
                return vehicleDto;
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
        public async Task<IEnumerable<VehicleResponseDto>> GetVehiclesByCompanyAsync(int companyId)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetByCompanyIdAsync(companyId);
                var vehicleDtos = _mapper.Map<IEnumerable<VehicleResponseDto>>(vehicles);

                _logger.LogInformation("Retrieved vehicles for company: {CompanyId}", companyId);
                return vehicleDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get available vehicles
        /// </summary>
        public async Task<IEnumerable<VehicleResponseDto>> GetAvailableVehiclesAsync(int companyId)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetAvailableVehiclesAsync(companyId);
                var vehicleDtos = _mapper.Map<IEnumerable<VehicleResponseDto>>(vehicles);

                _logger.LogInformation("Retrieved available vehicles for company: {CompanyId}", companyId);
                return vehicleDtos;
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
        public async Task<IEnumerable<VehicleResponseDto>> GetOverloadedVehiclesAsync(int companyId)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetOverloadedVehiclesAsync(companyId);
                var vehicleDtos = _mapper.Map<IEnumerable<VehicleResponseDto>>(vehicles);

                _logger.LogInformation("Retrieved overloaded vehicles for company: {CompanyId}", companyId);
                return vehicleDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overloaded vehicles: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Create new vehicle
        /// </summary>
        public async Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Verify company exists
                var company = await _companyRepository.GetByIdAsync(dto.CompanyId);
                if (company == null)
                    throw new KeyNotFoundException($"Company {dto.CompanyId} not found");

                var vehicle = _mapper.Map<Vehicle>(dto);
                var createdVehicle = await _vehicleRepository.AddAsync(vehicle);
                var vehicleDto = _mapper.Map<VehicleResponseDto>(createdVehicle);

                _logger.LogInformation("Created vehicle: {VehicleId} ({LicensePlate})", createdVehicle.VehicleId, createdVehicle.LicensePlate);
                return vehicleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                throw;
            }
        }

        /// <summary>
        /// Update vehicle
        /// </summary>
        public async Task<VehicleResponseDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                _mapper.Map(dto, vehicle);

                var updatedVehicle = await _vehicleRepository.UpdateAsync(vehicle);
                var vehicleDto = _mapper.Map<VehicleResponseDto>(updatedVehicle);

                _logger.LogInformation("Updated vehicle: {VehicleId}", vehicleId);
                return vehicleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Delete vehicle
        /// </summary>
        public async Task<bool> DeleteVehicleAsync(int vehicleId)
        {
            try
            {
                var result = await _vehicleRepository.DeleteAsync(vehicleId);

                _logger.LogInformation("Deleted vehicle: {VehicleId}", vehicleId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Update vehicle status
        /// </summary>
        public async Task<bool> UpdateVehicleStatusAsync(int vehicleId, string status)
        {
            try
            {
                var result = await _vehicleRepository.UpdateStatusAsync(vehicleId, status);

                _logger.LogInformation("Updated vehicle status: {VehicleId}, Status: {Status}", vehicleId, status);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle status: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Update vehicle weight
        /// </summary>
        public async Task<bool> UpdateVehicleWeightAsync(int vehicleId, decimal weightKg)
        {
            try
            {
                var result = await _vehicleRepository.UpdateCurrentWeightAsync(vehicleId, weightKg);

                _logger.LogInformation("Updated vehicle weight: {VehicleId}, Weight: {Weight}kg", vehicleId, weightKg);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle weight: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Allow overload
        /// </summary>
        public async Task<bool> AllowOverloadAsync(int vehicleId, bool allow, int? approvedBy = null)
        {
            try
            {
                var result = await _vehicleRepository.AllowOverloadAsync(vehicleId, allow);

                _logger.LogInformation("Updated overload allowance: {VehicleId}, Allow: {Allow}, ApprovedBy: {ApprovedBy}", 
                    vehicleId, allow, approvedBy);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allowing overload: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Get vehicle capacity
        /// </summary>
        public async Task<VehicleCapacityDto> GetVehicleCapacityAsync(int vehicleId)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                
                var currentOrders = await _orderRepository.GetByVehicleIdAsync(vehicle.VehicleId);
                var orderCount = currentOrders == null ? 0 : ((IEnumerable<Order>)currentOrders).Count();

                var capacityDto = new VehicleCapacityDto
                {
                    VehicleId = vehicle.VehicleId,
                    LicensePlate = vehicle.LicensePlate,
                    MaxWeightKg = vehicle.MaxWeightKg ?? 0,
                    CurrentWeightKg = vehicle.CurrentWeightKg ?? 0,
                    AvailableWeightKg = (vehicle.MaxWeightKg ?? 0) - (vehicle.CurrentWeightKg ?? 0),
                    CapacityPercentage = vehicle.CapacityPercentage ?? 0,
                    OverloadThreshold = vehicle.OverloadThreshold ?? 95,
                    IsOverloaded = (vehicle.CapacityPercentage ?? 0) > (vehicle.OverloadThreshold ?? 95),
                    AllowOverload = vehicle.AllowOverload ?? false,
                    CurrentOrderCount = orderCount,
                    CanAcceptMoreOrders = await _vehicleRepository.CheckCapacityAsync(vehicleId, 0)
                };

                _logger.LogInformation("Retrieved vehicle capacity: {VehicleId}", vehicleId);
                return capacityDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle capacity: {VehicleId}", vehicleId);
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
                var result = await _vehicleRepository.CheckCapacityAsync(vehicleId, additionalWeightKg);

                _logger.LogInformation("Checked vehicle capacity: {VehicleId}, AdditionalWeight: {Weight}kg", vehicleId, additionalWeightKg);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking vehicle capacity: {VehicleId}", vehicleId);
                throw;
            }
        }
    }
}
