using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Models.DTO;
using wedeli.Models.DTO.Customer;
using wedeli.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Repositories.Repo
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PlatformDbContext _context;
        private readonly AppDbContext _companyContext; // For cross-DB queries (Orders)
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(PlatformDbContext context, AppDbContext companyContext, ILogger<CustomerRepository> logger)
        {
            _context = context;
            _companyContext = companyContext;
            _logger = logger;
        }

        // ===== IBaseRepository Implementation =====

        public async Task<Customer> GetByIdAsync(int id)
        {
            try
            {
                var result = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);
                if (result == null)
                    throw new KeyNotFoundException($"Customer with ID {id} not found.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                throw;
            }
        }

        public async Task<Customer> AddAsync(Customer entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.TotalOrders = entity.TotalOrders ?? 0;
                entity.TotalRevenue = entity.TotalRevenue ?? 0;
                entity.IsRegular = entity.IsRegular ?? false;
                await _context.Customers.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer");
                throw;
            }
        }

        public void Update(Customer entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                _context.Customers.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                throw;
            }
        }

        public void Delete(Customer entity)
        {
            try
            {
                _context.Customers.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer");
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

        public async Task<Customer> UpdateAsync(Customer entity)
        {
            try
            {
                Update(entity);
                await SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var customer = await GetByIdAsync(id);
                if (customer == null)
                    return false;

                Delete(customer);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Customers.AnyAsync(c => c.CustomerId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer existence: {CustomerId}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Customers.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting customers");
                throw;
            }
        }

        // ===== ICustomerRepository Implementation =====

        public async Task<Customer> GetByUserIdAsync(int userId)
        {
            try
            {
                // Use raw SQL to ensure correct query
                var result = await _context.Customers
                    .FromSqlRaw("SELECT * FROM customers WHERE user_id = {0}", userId)
                    .Include(c => c.User)
                    .FirstOrDefaultAsync();
                    
                if (result == null)
                    throw new KeyNotFoundException($"Customer with user ID {userId} not found.");
                    
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<Customer> GetByPhoneAsync(string phone)
        {
            try
            {
                var result = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Phone == phone);
                if (result == null)
                    throw new KeyNotFoundException($"Customer with phone {phone} not found.");
                return result;
            }
            catch (KeyNotFoundException)
            {
                // Not finding customer by phone is expected for new customers - just rethrow
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by phone: {Phone}", phone);
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> GetRegularCustomersAsync()
        {
            try
            {
                return await _context.Customers
                    .Where(c => c.IsRegular == true)
                    .Include(c => c.User)
                    .OrderByDescending(c => c.TotalRevenue)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regular customers");
                throw;
            }
        }

        public async Task<bool> UpdateRegularStatusAsync(int customerId, bool isRegular)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                customer.IsRegular = isRegular;
                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regular status: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdatePaymentPrivilegeAsync(int customerId, string privilege)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                customer.PaymentPrivilege = privilege;
                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment privilege: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> IncrementTotalOrdersAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                customer.TotalOrders = (customer.TotalOrders ?? 0) + 1;
                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing total orders: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateTotalRevenueAsync(int customerId, decimal amount)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                customer.TotalRevenue = (customer.TotalRevenue ?? 0) + amount;
                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating total revenue: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    return new List<Customer>();

                var lower = searchTerm.ToLower();
                return await _context.Customers
                    .Include(c => c.User)
                    .Where(c => c.FullName.ToLower().Contains(lower) ||
                               c.Phone.Contains(searchTerm))
                    .OrderByDescending(c => c.TotalRevenue)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers: {SearchTerm}", searchTerm);
                throw;
            }
        }

        // ===== Customer CRUD Operations =====

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer by ID: {customerId}");
                throw;
            }
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer by user ID: {userId}");
                throw;
            }
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Phone == phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer by phone: {phone}");
                throw;
            }
        }

        public async Task<(List<Customer> Customers, int TotalCount)> GetCustomersAsync(CustomerSearchDto filter)
        {
            try
            {
                var query = _context.Customers
                    .Include(c => c.User)
                    .AsQueryable();

                if (filter.IsRegular.HasValue)
                    query = query.Where(c => c.IsRegular == filter.IsRegular.Value);

                if (!string.IsNullOrEmpty(filter.PaymentPrivilege))
                    query = query.Where(c => c.PaymentPrivilege == filter.PaymentPrivilege);

                if (filter.MinRevenue.HasValue)
                    query = query.Where(c => c.TotalRevenue >= filter.MinRevenue.Value);

                if (filter.MinOrders.HasValue)
                    query = query.Where(c => c.TotalOrders >= filter.MinOrders.Value);

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(c =>
                        c.FullName.ToLower().Contains(searchTerm) ||
                        c.Phone.Contains(filter.SearchTerm));
                }

                var totalCount = await query.CountAsync();

                var customers = await query
                    .OrderByDescending(c => c.TotalRevenue)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (customers, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers with filters");
                throw;
            }
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            try
            {
                customer.CreatedAt = DateTime.Now;
                customer.UpdatedAt = DateTime.Now;
                customer.TotalOrders = 0;
                customer.TotalRevenue = 0;
                customer.IsRegular = false;
                customer.PaymentPrivilege = customer.PaymentPrivilege ?? "standard";
                customer.CreditLimit = customer.CreditLimit ?? 0;

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer created: {customer.CustomerId}");
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                customer.UpdatedAt = DateTime.Now;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer updated: {customer.CustomerId}");
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer: {customer.CustomerId}");
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer deleted: {customerId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer: {customerId}");
                throw;
            }
        }

        // ===== Address Management =====

        public async Task<CustomerAddress?> GetAddressByIdAsync(int addressId)
        {
            try
            {
                return await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.AddressId == addressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting address by ID: {addressId}");
                throw;
            }
        }

        public async Task<List<CustomerAddress>> GetCustomerAddressesAsync(int customerId)
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
                _logger.LogError(ex, $"Error getting addresses for customer: {customerId}");
                throw;
            }
        }

        public async Task<CustomerAddress?> GetDefaultAddressAsync(int customerId)
        {
            try
            {
                return await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting default address for customer: {customerId}");
                throw;
            }
        }

        public async Task<CustomerAddress> CreateAddressAsync(CustomerAddress address)
        {
            try
            {
                address.CreatedAt = DateTime.Now;
                address.UsageCount = address.UsageCount ?? 0;

                // If this is set as default, unset other defaults
                if (address.IsDefault == true)
                {
                    var otherAddresses = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == address.CustomerId && a.IsDefault == true)
                        .ToListAsync();

                    foreach (var addr in otherAddresses)
                    {
                        addr.IsDefault = false;
                    }
                }

                await _context.CustomerAddresses.AddAsync(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Address created for customer: {address.CustomerId}");
                return address;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                throw;
            }
        }

        public async Task<CustomerAddress> UpdateAddressAsync(CustomerAddress address)
        {
            try
            {
                // If setting as default, unset other defaults
                if (address.IsDefault == true)
                {
                    var otherAddresses = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == address.CustomerId &&
                               a.AddressId != address.AddressId &&
                               a.IsDefault == true)
                        .ToListAsync();

                    foreach (var addr in otherAddresses)
                    {
                        addr.IsDefault = false;
                    }
                }

                _context.CustomerAddresses.Update(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Address updated: {address.AddressId}");
                return address;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating address: {address.AddressId}");
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses.FindAsync(addressId);
                if (address == null)
                    return false;

                _context.CustomerAddresses.Remove(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Address deleted: {addressId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting address: {addressId}");
                throw;
            }
        }

        public async Task<bool> SetDefaultAddressAsync(int customerId, int addressId)
        {
            try
            {
                // Unset all defaults for this customer
                var allAddresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId)
                    .ToListAsync();

                foreach (var addr in allAddresses)
                {
                    addr.IsDefault = addr.AddressId == addressId;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Default address set: Customer {customerId}, Address {addressId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting default address: {addressId}");
                throw;
            }
        }

        public async Task IncrementAddressUsageAsync(int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses.FindAsync(addressId);
                if (address != null)
                {
                    address.UsageCount = (address.UsageCount ?? 0) + 1;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error incrementing address usage: {addressId}");
                throw;
            }
        }

        // ===== Order History =====

        public async Task<List<Order>> GetCustomerOrdersAsync(int customerId, string? status = null)
        {
            try
            {
                var query = _companyContext.Orders
                    .Where(o => o.CustomerId == customerId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.OrderStatus == status);

                return await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting orders for customer: {customerId}");
                throw;
            }
        }

        public async Task<int> GetCustomerOrderCountAsync(int customerId)
        {
            try
            {
                return await _companyContext.Orders.CountAsync(o => o.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order count for customer: {customerId}");
                throw;
            }
        }

        // ===== Statistics =====

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Orders)
                    .Include(c => c.CustomerAddresses)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                    throw new KeyNotFoundException($"Customer not found: {customerId}");

                var orders = customer.Orders.ToList();
                var completedOrders = orders.Where(o => o.OrderStatus == "delivered").ToList();

                var lastOrderDate = orders.Any()
                    ? orders.OrderByDescending(o => o.CreatedAt).First().CreatedAt
                    : (DateTime?)null;

                var daysSinceLastOrder = lastOrderDate.HasValue
                    ? (DateTime.Now - lastOrderDate.Value).Days
                    : 0;

                return new CustomerStatisticsDto
                {
                    CustomerId = customerId,
                    CustomerName = customer.FullName,
                    IsRegular = customer.IsRegular == true,
                    TotalOrders = orders.Count,
                    CompletedOrders = completedOrders.Count,
                    CancelledOrders = orders.Count(o => o.OrderStatus == "cancelled"),
                    PendingOrders = orders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                    TotalRevenue = customer.TotalRevenue ?? 0,
                    AverageOrderValue = orders.Any() ? orders.Average(o => o.ShippingFee) : 0,
                    LastOrderDate = lastOrderDate,
                    DaysSinceLastOrder = daysSinceLastOrder,
                    SuccessRate = orders.Any() ? (double)completedOrders.Count / orders.Count : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer statistics: {customerId}");
                throw;
            }
        }

        public async Task UpdateCustomerStatisticsAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return;

                var orders = await _companyContext.Orders
                    .Where(o => o.CustomerId == customerId)
                    .ToListAsync();

                customer.TotalOrders = orders.Count;
                customer.TotalRevenue = orders
                    .Where(o => o.OrderStatus == "delivered")
                    .Sum(o => o.ShippingFee);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Customer statistics updated: {customerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer statistics: {customerId}");
                throw;
            }
        }

        // ===== Customer Status (Regular/VIP) =====

        public async Task<bool> UpdateCustomerStatusAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                // Business rule: Regular customer if >= 5 orders
                var totalOrders = customer.TotalOrders ?? 0;
                customer.IsRegular = totalOrders >= 5;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Customer status updated: {customerId}, IsRegular: {customer.IsRegular}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer status: {customerId}");
                throw;
            }
        }

        public async Task<List<Customer>> GetVipCustomersAsync(int minOrders = 10, decimal minRevenue = 10000000)
        {
            try
            {
                return await _context.Customers
                    .Where(c => c.IsRegular == true &&
                           c.TotalOrders >= minOrders &&
                           c.TotalRevenue >= minRevenue)
                    .OrderByDescending(c => c.TotalRevenue)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VIP customers");
                throw;
            }
        }

        // ===== Helper Methods =====

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeCustomerId = null)
        {
            try
            {
                var query = _context.Customers.Where(c => c.Phone == phone);

                if (excludeCustomerId.HasValue)
                    query = query.Where(c => c.CustomerId != excludeCustomerId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking phone existence: {phone}");
                throw;
            }
        }
    }
}