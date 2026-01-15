using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class CustomerAddressRepository : ICustomerAddressRepository
    {
        private readonly PlatformDbContext _context;
        private readonly ILogger<CustomerAddressRepository> _logger;

        public CustomerAddressRepository(PlatformDbContext context, ILogger<CustomerAddressRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CustomerAddress> GetByIdAsync(int addressId)
        {
            try
            {
                return await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.AddressId == addressId) ?? new CustomerAddress();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address: {AddressId}", addressId);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerAddress>> GetAllAsync()
        {
            try
            {
                return await _context.CustomerAddresses.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all addresses");
                throw;
            }
        }

        public async Task<CustomerAddress> AddAsync(CustomerAddress entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _context.CustomerAddresses.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address");
                throw;
            }
        }

        public void Update(CustomerAddress entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                _context.CustomerAddresses.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address");
                throw;
            }
        }

        public void Delete(CustomerAddress entity)
        {
            try
            {
                _context.CustomerAddresses.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.UsageCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses by customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddress> GetDefaultAddressAsync(int customerId)
        {
            try
            {
                return await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault == true) ?? new CustomerAddress();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> SetDefaultAddressAsync(int customerId, int addressId)
        {
            try
            {
                var addresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId)
                    .ToListAsync();

                foreach (var addr in addresses)
                {
                    addr.IsDefault = (addr.AddressId == addressId);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address: {AddressId}", addressId);
                throw;
            }
        }

        public async Task<bool> IncrementUsageCountAsync(int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses.FindAsync(addressId);
                if (address == null)
                    return false;

                address.UsageCount = (address.UsageCount ?? 0) + 1;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing usage count: {AddressId}", addressId);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerAddress>> GetMostUsedAddressesAsync(int customerId, int topN = 5)
        {
            try
            {
                return await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId)
                    .OrderByDescending(a => a.UsageCount)
                    .Take(topN)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most used addresses: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddress> UpdateAsync(CustomerAddress entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer address");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var address = await GetByIdAsync(id);
                if (address == null)
                    return false;

                Delete(address);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer address: {AddressId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.CustomerAddresses.AnyAsync(a => a.AddressId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer address existence: {AddressId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.CustomerAddresses.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting customer addresses");
                throw;
            }
        }
    }
}
