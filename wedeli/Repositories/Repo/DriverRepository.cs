using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Infrastructure;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Driver repository for driver data access operations
    /// </summary>
    public class DriverRepository : IDriverRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DriverRepository> _logger;

        public DriverRepository(AppDbContext context, ILogger<DriverRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get driver by user ID
        /// </summary>
        public async Task<Driver> GetByUserIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0", nameof(userId));

                var driver = await _context.Drivers
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Include(d => d.CompanyUser)
                    .FirstOrDefaultAsync(d => d.CompanyUserId == userId);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver with user ID {userId} not found.");
                return driver;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver by user ID: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get drivers by company ID
        /// </summary>
        public async Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var drivers = await _context.Drivers
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Include(d => d.CompanyUser)
                    .Where(d => d.CompanyId == companyId)
                    .OrderBy(d => d.CompanyUser.FullName)
                    .ToListAsync();

                return drivers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get active drivers by company ID
        /// </summary>
        public async Task<IEnumerable<Driver>> GetActiveDriversAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var drivers = await _context.Drivers
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Include(d => d.CompanyUser)
                    .Where(d => d.CompanyId == companyId && d.IsActive == true)
                    .OrderBy(d => d.CompanyUser.FullName)
                    .ToListAsync();

                return drivers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active drivers for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Update driver statistics
        /// </summary>
        public async Task<bool> UpdateStatisticsAsync(int driverId, int totalTrips, decimal successRate, decimal rating)
        {
            try
            {
                if (driverId <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(driverId));

                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver {driverId} not found");

                driver.TotalTrips = totalTrips;
                driver.SuccessRate = successRate;
                driver.Rating = rating;
                driver.CreatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated driver statistics - DriverId: {DriverId}", driverId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver statistics: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Toggle driver active status
        /// </summary>
        public async Task<bool> ToggleActiveStatusAsync(int driverId, bool isActive)
        {
            try
            {
                if (driverId <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(driverId));

                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver {driverId} not found");

                driver.IsActive = isActive;
                driver.CreatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Toggled driver active status - DriverId: {DriverId}, IsActive: {IsActive}", driverId, isActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling driver active status: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get top performing drivers
        /// </summary>
        public async Task<IEnumerable<Driver>> GetTopPerformingDriversAsync(int companyId, int topN = 10)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));

                var drivers = await _context.Drivers
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Include(d => d.CompanyUser)
                    .Where(d => d.CompanyId == companyId && d.IsActive == true)
                    .OrderByDescending(d => d.Rating)
                    .ThenByDescending(d => d.SuccessRate)
                    .Take(topN)
                    .ToListAsync();

                return drivers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top performing drivers for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get driver by ID
        /// </summary>
        public async Task<Driver> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(id));

                var driver = await _context.Drivers
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Include(d => d.CompanyUser)
                    .FirstOrDefaultAsync(d => d.DriverId == id);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver {id} not found");

                return driver;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver: {DriverId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all drivers
        /// </summary>
        public async Task<IEnumerable<Driver>> GetAllAsync()
        {
            try
            {
                var drivers = await _context.Drivers
                    // NOTE: Company (TransportCompany) is in Platform DB, cannot be included in same query
                    .Include(d => d.CompanyUser)
                    .OrderBy(d => d.CompanyUser.FullName)
                    .ToListAsync();

                return drivers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all drivers");
                throw;
            }
        }

        /// <summary>
        /// Add new driver
        /// </summary>
        public async Task<Driver> AddAsync(Driver entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Check if license already exists
                if (await _context.Drivers.AnyAsync(d => d.DriverLicense == entity.DriverLicense))
                    throw new InvalidOperationException($"Driver with license number {entity.DriverLicense} already exists");

                // Check if company user already exists as driver
                if (await _context.Drivers.AnyAsync(d => d.CompanyUserId == entity.CompanyUserId))
                    throw new InvalidOperationException($"CompanyUser {entity.CompanyUserId} is already a driver");

                entity.CreatedAt = DateTime.UtcNow;
                entity.IsActive = true;
                entity.TotalTrips = 0;
                entity.SuccessRate = 0;
                entity.Rating = 0;

                _context.Drivers.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new driver: {DriverId} ({CompanyUserId})", entity.DriverId, entity.CompanyUserId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding driver");
                throw;
            }
        }

        /// <summary>
        /// Update driver
        /// </summary>
        public async Task<Driver> UpdateAsync(Driver entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingDriver = await _context.Drivers
                    .FirstOrDefaultAsync(d => d.DriverId == entity.DriverId);

                if (existingDriver == null)
                    throw new KeyNotFoundException($"Driver {entity.DriverId} not found");

                // Check for duplicate license number (if changed)
                if (existingDriver.DriverLicense != entity.DriverLicense &&
                    await _context.Drivers.AnyAsync(d => d.DriverLicense == entity.DriverLicense))
                    throw new InvalidOperationException($"Driver with license number {entity.DriverLicense} already exists");

                existingDriver.DriverLicense = entity.DriverLicense;
                existingDriver.LicenseExpiry = entity.LicenseExpiry;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated driver: {DriverId} ({CompanyUserId})", entity.DriverId, entity.CompanyUserId);
                return existingDriver;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver: {DriverId}", entity?.DriverId);
                throw;
            }
        }

        /// <summary>
        /// Delete driver by ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(id));

                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == id);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver {id} not found");

                // Check if driver has active trips
                var activeTrips = await _context.TripOrders
                    .Where(t => t.Trip.DriverId == id && t.Trip.TripStatus != "completed")
                    .CountAsync();

                if (activeTrips > 0)
                    throw new InvalidOperationException("Cannot delete driver with active trips");

                _context.Drivers.Remove(driver);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted driver: {DriverId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver: {DriverId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if driver exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Drivers.AnyAsync(d => d.DriverId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if driver exists: {DriverId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count total drivers
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Drivers.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting drivers");
                throw;
            }
        }
    }
}
