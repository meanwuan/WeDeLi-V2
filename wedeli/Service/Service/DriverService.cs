using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Driver service for managing drivers
    /// </summary>
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITransportCompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DriverService> _logger;

        public DriverService(
            IDriverRepository driverRepository,
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            ITransportCompanyRepository companyRepository,
            IMapper mapper,
            ILogger<DriverService> logger)
        {
            _driverRepository = driverRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get driver by ID
        /// </summary>
        public async Task<DriverResponseDto> GetDriverByIdAsync(int driverId)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(driverId);
                var driverDto = _mapper.Map<DriverResponseDto>(driver);

                // Enrich with company name from Platform DB
                if (driver.CompanyId > 0)
                {
                    var company = await _companyRepository.GetByIdAsync(driver.CompanyId);
                    driverDto.CompanyName = company?.CompanyName;
                }

                _logger.LogInformation("Retrieved driver: {DriverId}", driverId);
                return driverDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get driver by user ID
        /// </summary>
        public async Task<DriverResponseDto> GetDriverByUserIdAsync(int userId)
        {
            try
            {
                var driver = await _driverRepository.GetByUserIdAsync(userId);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver for user {userId} not found");

                var driverDto = _mapper.Map<DriverResponseDto>(driver);

                _logger.LogInformation("Retrieved driver by user ID: {UserId}", userId);
                return driverDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver by user ID: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get all drivers (for admin)
        /// </summary>
        public async Task<IEnumerable<DriverResponseDto>> GetAllDriversAsync()
        {
            try
            {
                var drivers = await _driverRepository.GetAllAsync();
                var driverDtos = _mapper.Map<IEnumerable<DriverResponseDto>>(drivers);

                _logger.LogInformation("Retrieved all {Count} drivers", driverDtos.Count());
                return driverDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all drivers");
                throw;
            }
        }

        /// <summary>
        /// Get drivers by company
        /// </summary>
        public async Task<IEnumerable<DriverResponseDto>> GetDriversByCompanyAsync(int companyId)
        {
            try
            {
                var drivers = await _driverRepository.GetByCompanyIdAsync(companyId);
                var driverDtos = _mapper.Map<IEnumerable<DriverResponseDto>>(drivers);

                _logger.LogInformation("Retrieved drivers for company: {CompanyId}", companyId);
                return driverDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get active drivers
        /// </summary>
        public async Task<IEnumerable<DriverResponseDto>> GetActiveDriversAsync(int companyId)
        {
            try
            {
                var drivers = await _driverRepository.GetActiveDriversAsync(companyId);
                var driverDtos = _mapper.Map<IEnumerable<DriverResponseDto>>(drivers);

                _logger.LogInformation("Retrieved active drivers for company: {CompanyId}", companyId);
                return driverDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active drivers for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get top performing drivers
        /// </summary>
        public async Task<IEnumerable<DriverResponseDto>> GetTopPerformingDriversAsync(int companyId, int topN = 10)
        {
            try
            {
                var drivers = await _driverRepository.GetTopPerformingDriversAsync(companyId, topN);
                var driverDtos = _mapper.Map<IEnumerable<DriverResponseDto>>(drivers);

                _logger.LogInformation("Retrieved top {TopN} performing drivers for company: {CompanyId}", topN, companyId);
                return driverDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top performing drivers for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Create driver
        /// </summary>
        public async Task<DriverResponseDto> CreateDriverAsync(CreateDriverDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Verify user exists
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                    throw new KeyNotFoundException($"User {dto.UserId} not found");

                // Check if user is already registered as a driver
                var existingDriver = await _driverRepository.GetByUserIdAsync(dto.UserId);
                if (existingDriver != null)
                    throw new InvalidOperationException($"User {dto.UserId} is already registered as a driver (DriverId: {existingDriver.DriverId})");

                var driver = _mapper.Map<Driver>(dto);
                var createdDriver = await _driverRepository.AddAsync(driver);
                var driverDto = _mapper.Map<DriverResponseDto>(createdDriver);

                _logger.LogInformation("Created driver: {DriverId} ({CompanyUserId})", createdDriver.DriverId, createdDriver.CompanyUserId);
                return driverDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                throw;
            }
        }

        /// <summary>
        /// Update driver
        /// </summary>
        public async Task<DriverResponseDto> UpdateDriverAsync(int driverId, UpdateDriverDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var driver = await _driverRepository.GetByIdAsync(driverId);
                _mapper.Map(dto, driver);

                var updatedDriver = await _driverRepository.UpdateAsync(driver);
                var driverDto = _mapper.Map<DriverResponseDto>(updatedDriver);

                _logger.LogInformation("Updated driver: {DriverId}", driverId);
                return driverDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Delete driver
        /// </summary>
        public async Task<bool> DeleteDriverAsync(int driverId)
        {
            try
            {
                var result = await _driverRepository.DeleteAsync(driverId);

                _logger.LogInformation("Deleted driver: {DriverId}", driverId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Toggle driver status
        /// </summary>
        public async Task<bool> ToggleDriverStatusAsync(int driverId, bool isActive)
        {
            try
            {
                var result = await _driverRepository.ToggleActiveStatusAsync(driverId, isActive);

                _logger.LogInformation("Toggled driver status - DriverId: {DriverId}, IsActive: {IsActive}", driverId, isActive);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling driver status: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get driver performance
        /// </summary>
        public async Task<DriverPerformanceDto> GetDriverPerformanceAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(driverId);

                var performanceDto = new DriverPerformanceDto
                {
                    DriverId = driver.DriverId,
                    DriverName = driver.CompanyUser?.FullName ?? "Unknown",
                    TotalTrips = driver.TotalTrips ?? 0,
                    SuccessRate = (double)(driver.SuccessRate ?? 0),
                    AverageRating = (double)(driver.Rating ?? 0),
                    AverageDeliveryTime = 0,
                };

                _logger.LogInformation("Retrieved driver performance: {DriverId}", driverId);
                return performanceDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver performance: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Update driver statistics
        /// </summary>
        public async Task<bool> UpdateDriverStatisticsAsync(int driverId)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(driverId);

                // Calculate statistics from orders
                var totalTrips = 0; // Would calculate from trips in real implementation
                var successRate = 0m; // Would calculate from completed trips
                var rating = driver.Rating ?? 0; // Keep existing rating

                var result = await _driverRepository.UpdateStatisticsAsync(driverId, totalTrips, successRate, rating);

                _logger.LogInformation("Updated driver statistics: {DriverId}", driverId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver statistics: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Update driver rating
        /// </summary>
        public async Task<bool> UpdateDriverRatingAsync(int driverId)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(driverId);

                // Calculate average rating from all ratings in Rating table
                var ratings = new List<decimal>(); // Would fetch from Rating table in real implementation
                var averageRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 2) : 0;

                var result = await _driverRepository.UpdateStatisticsAsync(
                    driverId,
                    driver.TotalTrips ?? 0,
                    driver.SuccessRate ?? 0,
                    averageRating);

                _logger.LogInformation("Updated driver rating: {DriverId}, NewRating: {Rating}", driverId, averageRating);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver rating: {DriverId}", driverId);
                throw;
            }
        }
    }
}
