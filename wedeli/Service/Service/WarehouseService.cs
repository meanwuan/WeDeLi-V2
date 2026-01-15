using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Service.Service
{
    public class WarehouseService : IWarehouseStaffService
    {
        private readonly IWarehouseStaffRepository _warehouseStaffRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(
            IWarehouseStaffRepository warehouseStaffRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<WarehouseService> logger)
        {
            _warehouseStaffRepository = warehouseStaffRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<WarehouseStaffDto> GetStaffByIdAsync(int staffId)
        {
            try
            {
                var staff = await _warehouseStaffRepository.GetByIdAsync(staffId);
                if (staff == null)
                    throw new KeyNotFoundException($"Warehouse staff with ID {staffId} not found.");

                return _mapper.Map<WarehouseStaffDto>(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse staff: {StaffId}", staffId);
                throw;
            }
        }

        public async Task<WarehouseStaffDto> GetStaffByUserIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");

                var staffList = await _warehouseStaffRepository.GetAllAsync();
                var staff = staffList.FirstOrDefault(s => s.CompanyUserId == userId);
                
                if (staff == null)
                    throw new KeyNotFoundException($"Warehouse staff for user ID {userId} not found.");

                return _mapper.Map<WarehouseStaffDto>(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse staff by user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<WarehouseStaffDto>> GetAllStaffAsync()
        {
            try
            {
                var staffList = await _warehouseStaffRepository.GetAllAsync();
                _logger.LogInformation("Retrieved all {Count} warehouse staff", staffList.Count());
                return _mapper.Map<IEnumerable<WarehouseStaffDto>>(staffList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all warehouse staff");
                throw;
            }
        }

        public async Task<IEnumerable<WarehouseStaffDto>> GetStaffByCompanyAsync(int companyId)
        {
            try
            {
                var staffList = await _warehouseStaffRepository.GetAllAsync();
                var companyStaff = staffList.Where(s => s.CompanyId == companyId).ToList();

                return _mapper.Map<IEnumerable<WarehouseStaffDto>>(companyStaff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse staff by company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<WarehouseStaffDto> CreateStaffAsync(CreateWarehouseStaffDto dto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");

                // Check if user is already registered as warehouse staff
                var allStaff = await _warehouseStaffRepository.GetAllAsync();
                var existingStaff = allStaff.FirstOrDefault(s => s.CompanyUserId == dto.UserId);
                if (existingStaff != null)
                    throw new InvalidOperationException($"CompanyUser {dto.UserId} is already registered as warehouse staff (StaffId: {existingStaff.StaffId})");

                var staff = new WarehouseStaff
                {
                    CompanyUserId = dto.UserId,
                    CompanyId = dto.CompanyId,
                    WarehouseLocation = dto.WarehouseLocation,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdStaff = await _warehouseStaffRepository.AddAsync(staff);
                await _warehouseStaffRepository.SaveChangesAsync();

                _logger.LogInformation("Warehouse staff created: {StaffId}, CompanyId: {CompanyId}", createdStaff.StaffId, createdStaff.CompanyId);
                return _mapper.Map<WarehouseStaffDto>(createdStaff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse staff");
                throw;
            }
        }

        public async Task<WarehouseStaffDto> UpdateStaffAsync(int staffId, UpdateWarehouseStaffDto dto)
        {
            try
            {
                var staff = await _warehouseStaffRepository.GetByIdAsync(staffId);
                if (staff == null)
                    throw new KeyNotFoundException($"Warehouse staff with ID {staffId} not found.");

                staff.WarehouseLocation = dto.WarehouseLocation ?? staff.WarehouseLocation;
                staff.CompanyId = dto.CompanyId;
                staff.IsActive = dto.IsActive;

                await _warehouseStaffRepository.UpdateAsync(staff);

                _logger.LogInformation("Warehouse staff updated: {StaffId}", staffId);
                return _mapper.Map<WarehouseStaffDto>(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse staff: {StaffId}", staffId);
                throw;
            }
        }

        public async Task<bool> DeleteStaffAsync(int staffId)
        {
            try
            {
                var staff = await _warehouseStaffRepository.GetByIdAsync(staffId);
                if (staff == null)
                    throw new KeyNotFoundException($"Warehouse staff with ID {staffId} not found.");

                return await _warehouseStaffRepository.DeleteAsync(staffId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse staff: {StaffId}", staffId);
                throw;
            }
        }

        public async Task<bool> ToggleStaffStatusAsync(int staffId, bool isActive)
        {
            try
            {
                var staff = await _warehouseStaffRepository.GetByIdAsync(staffId);
                if (staff == null)
                    throw new KeyNotFoundException($"Warehouse staff with ID {staffId} not found.");

                staff.IsActive = isActive;
                await _warehouseStaffRepository.UpdateAsync(staff);

                _logger.LogInformation("Warehouse staff status toggled: {StaffId}, IsActive: {IsActive}", staffId, isActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling warehouse staff status: {StaffId}", staffId);
                throw;
            }
        }
    }
}
