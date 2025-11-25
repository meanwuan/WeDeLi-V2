using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace wedeli.Services.Implementation
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DriverService> _logger;

        public DriverService(
            IDriverRepository driverRepository,
            IUserRepository userRepository,
            ILogger<DriverService> logger)
        {
            _driverRepository = driverRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        // ===== CRUD Operations =====

        public async Task<DriverDto?> GetDriverByIdAsync(int driverId)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(driverId);
            return driver == null ? null : MapToDto(driver);
        }

        public async Task<DriverDto?> GetDriverByUserIdAsync(int userId)
        {
            var driver = await _driverRepository.GetDriverByUserIdAsync(userId);
            return driver == null ? null : MapToDto(driver);
        }

        public async Task<(List<DriverDto> Drivers, int TotalCount)> GetDriversAsync(DriverFilterDto filter)
        {
            var (drivers, totalCount) = await _driverRepository.GetDriversAsync(filter);
            var driverDtos = drivers.Select(MapToDto).ToList();
            return (driverDtos, totalCount);
        }

        public async Task<List<DriverDto>> GetDriversByCompanyAsync(int companyId)
        {
            var drivers = await _driverRepository.GetDriversByCompanyAsync(companyId);
            return drivers.Select(MapToDto).ToList();
        }

        public async Task<DriverDto> CreateDriverAsync(CreateDriverDto dto)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User not found: {dto.UserId}");
            }

            // Check if user is already a driver
            if (await _driverRepository.UserIsDriverAsync(dto.UserId))
            {
                throw new InvalidOperationException($"User {dto.UserId} is already registered as a driver");
            }

            // Validate license expiry
            if (dto.LicenseExpiry.HasValue && dto.LicenseExpiry.Value < DateTime.Now)
            {
                throw new ArgumentException("Driver license has expired");
            }

            var driver = new Driver
            {
                UserId = dto.UserId,
                CompanyId = dto.CompanyId,
                DriverLicense = dto.DriverLicense,
                LicenseExpiry = dto.LicenseExpiry.HasValue ? DateOnly.FromDateTime(dto.LicenseExpiry.Value) : null
            };

            var createdDriver = await _driverRepository.CreateDriverAsync(driver);
            _logger.LogInformation($"Driver created successfully: {createdDriver.DriverId}");

            return (await GetDriverByIdAsync(createdDriver.DriverId))!;
        }

        public async Task<DriverDto> UpdateDriverAsync(int driverId, UpdateDriverDto dto)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(driverId);
            if (driver == null)
            {
                throw new KeyNotFoundException($"Driver not found: {driverId}");
            }

            if (!string.IsNullOrEmpty(dto.DriverLicense))
                driver.DriverLicense = dto.DriverLicense;

            if (dto.LicenseExpiry.HasValue)
            {
                if (dto.LicenseExpiry.Value < DateTime.Now)
                {
                    throw new ArgumentException("License expiry date cannot be in the past");
                }
                driver.LicenseExpiry = DateOnly.FromDateTime(dto.LicenseExpiry.Value);
            }

            if (dto.IsActive.HasValue)
                driver.IsActive = dto.IsActive.Value;

            if (dto.Rating.HasValue && dto.Rating.Value >= 0 && dto.Rating.Value <= 5)
                driver.Rating = dto.Rating.Value;

            var updatedDriver = await _driverRepository.UpdateDriverAsync(driver);
            _logger.LogInformation($"Driver updated successfully: {driverId}");

            return MapToDto(updatedDriver);
        }

        public async Task<bool> DeleteDriverAsync(int driverId)
        {
            var deleted = await _driverRepository.DeleteDriverAsync(driverId);
            if (deleted)
            {
                _logger.LogInformation($"Driver deleted successfully: {driverId}");
            }
            return deleted;
        }

        // ===== Performance & Statistics =====

        public async Task<DriverPerformanceDto> GetDriverPerformanceAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _driverRepository.GetDriverPerformanceAsync(driverId, startDate, endDate);
        }

        public async Task<DriverOrdersDto> GetDriverOrdersAsync(int driverId, string? status = null)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(driverId);
            if (driver == null)
            {
                throw new KeyNotFoundException($"Driver not found: {driverId}");
            }

            var orders = await _driverRepository.GetDriverOrdersAsync(driverId, status);

            return new DriverOrdersDto
            {
                DriverId = driverId,
                FullName = driver.User.FullName,
                TotalOrders = orders.Count,
                Orders = orders.Select(o => new OrderListItem
                {
                    OrderId = o.OrderId,
                    TrackingCode = o.TrackingCode,
                    SenderName = o.Customer.FullName,
                    SenderPhone = o.Customer.Phone ?? string.Empty,
                    ReceiverName = o.ReceiverName,
                    ReceiverPhone = o.ReceiverPhone,
                    ReceiverAddress = o.ReceiverAddress,
                    ReceiverProvince = o.ReceiverAddress,
                    ShippingFee = o.ShippingFee,
                    CodAmount = o.CodAmount ?? 0M,
                    OrderStatus = o.OrderStatus ?? "pending",
                    PaymentStatus = o.PaymentStatus ?? "pending",
                    DriverName = driver.User?.FullName,
                    RouteName = null,
                    CreatedAt = o.CreatedAt ?? DateTime.Now
                }).ToList()
            };
        }

        public async Task<DriverDailySummaryDto> GetDriverDailySummaryAsync(int driverId, DateTime date)
        {
            return await _driverRepository.GetDriverDailySummaryAsync(driverId, date);
        }

        // ===== Availability =====

        public async Task<List<DriverDto>> GetAvailableDriversAsync(int? companyId = null)
        {
            var drivers = await _driverRepository.GetAvailableDriversAsync(companyId);
            return drivers.Select(MapToDto).ToList();
        }

        public async Task<bool> IsDriverAvailableAsync(int driverId)
        {
            return await _driverRepository.IsDriverAvailableAsync(driverId);
        }

        // ===== License Management =====

        public async Task<List<DriverDto>> GetDriversWithExpiringLicenseAsync(int daysThreshold = 30)
        {
            var drivers = await _driverRepository.GetDriversWithExpiringLicenseAsync(daysThreshold);
            return drivers.Select(MapToDto).ToList();
        }

        // ===== COD Management =====

        public async Task<decimal> GetDriverPendingCodAsync(int driverId)
        {
            return await _driverRepository.GetDriverPendingCodAsync(driverId);
        }

        // ===== Helper Methods =====

        private DriverDto MapToDto(Driver driver)
        {
            return new DriverDto
            {
                DriverId = driver.DriverId,
                UserId = driver.UserId,
                UserName = driver.User?.Username,
                FullName = driver.User?.FullName,
                Phone = driver.User?.Phone,
                CompanyId = driver.CompanyId,
                CompanyName = driver.Company?.CompanyName,
                DriverLicense = driver.DriverLicense,
                LicenseExpiry = driver.LicenseExpiry.HasValue ? driver.LicenseExpiry.Value.ToDateTime(TimeOnly.MinValue) : null,
                TotalTrips = driver.TotalTrips ?? 0,
                SuccessRate = driver.SuccessRate ?? 0,
                Rating = driver.Rating ?? 0,
                IsActive = driver.IsActive ?? false,
                CreatedAt = driver.CreatedAt ?? DateTime.Now
            };
        }
    }
}