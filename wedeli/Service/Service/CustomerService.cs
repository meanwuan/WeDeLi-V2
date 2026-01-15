using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Customer;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Service.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerAddressRepository _customerAddressRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository customerRepository,
            ICustomerAddressRepository customerAddressRepository,
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CustomerResponseDto> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                return _mapper.Map<CustomerResponseDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerDetailDto> GetCustomerDetailAsync(int customerId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                var addresses = await _customerAddressRepository.GetAllAsync();
                var customerAddresses = addresses.Where(a => a.CustomerId == customerId).ToList();

                var orders = await _orderRepository.GetAllAsync();
                var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();

                var detail = _mapper.Map<CustomerDetailDto>(customer);
                detail.Addresses = _mapper.Map<List<CustomerAddressDto>>(customerAddresses);
                detail.RecentOrders = _mapper.Map<List<RecentOrderDto>>(customerOrders.OrderByDescending(o => o.CreatedAt).Take(5));
                detail.PendingOrders = customerOrders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled");
                detail.CompletedOrders = customerOrders.Count(o => o.OrderStatus == "delivered");
                detail.CancelledOrders = customerOrders.Count(o => o.OrderStatus == "cancelled");

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer detail: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerResponseDto> GetCustomerByUserIdAsync(int userId)
        {
            try
            {
                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with user ID {userId} not found.");

                return _mapper.Map<CustomerResponseDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<CustomerResponseDto> GetCustomerByPhoneAsync(string phone)
        {
            try
            {
                var customer = await _customerRepository.GetByPhoneAsync(phone);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with phone {phone} not found.");

                return _mapper.Map<CustomerResponseDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by phone: {Phone}", phone);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerListItemDto>> GetAllCustomersAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();
                var pagedCustomers = customers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return _mapper.Map<IEnumerable<CustomerListItemDto>>(pagedCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetRegularCustomersAsync()
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();
                var regularCustomers = customers.Where(c => c.IsRegular == true).ToList();
                return _mapper.Map<IEnumerable<CustomerResponseDto>>(regularCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regular customers");
                throw;
            }
        }

        public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            try
            {
                var customer = _mapper.Map<Customer>(dto);
                customer.CreatedAt = DateTime.UtcNow;
                customer.IsRegular = false;

                var createdCustomer = await _customerRepository.AddAsync(customer);
                await _customerRepository.SaveChangesAsync();

                _logger.LogInformation("Customer created: {CustomerId}", createdCustomer.CustomerId);
                return _mapper.Map<CustomerResponseDto>(createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        public async Task<CustomerResponseDto> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                _mapper.Map(dto, customer);
                customer.UpdatedAt = DateTime.UtcNow;

                await _customerRepository.UpdateAsync(customer);

                _logger.LogInformation("Customer updated: {CustomerId}", customerId);
                return _mapper.Map<CustomerResponseDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateRegularStatusAsync(int customerId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                customer.IsRegular = !(customer.IsRegular == true);
                customer.UpdatedAt = DateTime.UtcNow;

                await _customerRepository.UpdateAsync(customer);

                _logger.LogInformation("Customer regular status toggled: {CustomerId}", customerId);
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
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                customer.PaymentPrivilege = privilege;
                customer.UpdatedAt = DateTime.UtcNow;

                await _customerRepository.UpdateAsync(customer);

                _logger.LogInformation("Customer payment privilege updated: {CustomerId}, Privilege: {Privilege}", customerId, privilege);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment privilege: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                var orders = await _orderRepository.GetAllAsync();
                var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();

                var successRate = customerOrders.Any() 
                    ? (double)customerOrders.Count(o => o.OrderStatus == "delivered") / customerOrders.Count * 100 
                    : 0;

                var now = DateTime.UtcNow;
                var thisMonth = customerOrders.Count(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Year == now.Year && o.CreatedAt.Value.Month == now.Month);
                var lastMonth = customerOrders.Count(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Year == now.Year && o.CreatedAt.Value.Month == now.Month - 1);
                var lastOrderDate = customerOrders.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt;
                var daysSinceLastOrder = lastOrderDate.HasValue ? (int)(now - lastOrderDate.Value).TotalDays : 0;

                return new CustomerStatisticsDto
                {
                    CustomerId = customerId,
                    CustomerName = customer.FullName,
                    IsRegular = customer.IsRegular ?? false,
                    TotalOrders = customerOrders.Count,
                    CompletedOrders = customerOrders.Count(o => o.OrderStatus == "delivered"),
                    PendingOrders = customerOrders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                    CancelledOrders = customerOrders.Count(o => o.OrderStatus == "cancelled"),
                    SuccessRate = successRate,
                    TotalRevenue = 0,
                    AverageOrderValue = customerOrders.Any() ? 0 : 0,
                    OutstandingBalance = 0,
                    FirstOrderDate = customerOrders.OrderBy(o => o.CreatedAt).FirstOrDefault()?.CreatedAt,
                    LastOrderDate = lastOrderDate,
                    DaysSinceLastOrder = daysSinceLastOrder,
                    OrdersThisMonth = thisMonth,
                    OrdersLastMonth = lastMonth,
                    LoyaltyPoints = 0,
                    LoyaltyTier = "Bronze"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerListItemDto>> SearchCustomersAsync(CustomerSearchDto searchDto)
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();
                var query = customers.AsEnumerable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(c => 
                        c.FullName.ToLower().Contains(term) || 
                        c.Phone.ToLower().Contains(term) ||
                        (c.Email ?? "").ToLower().Contains(term));
                }

                if (searchDto.IsRegular.HasValue)
                    query = query.Where(c => c.IsRegular == searchDto.IsRegular.Value);

                if (!string.IsNullOrEmpty(searchDto.PaymentPrivilege))
                    query = query.Where(c => c.PaymentPrivilege == searchDto.PaymentPrivilege);

                if (searchDto.MinOrders.HasValue)
                    query = query.Where(c => (c.TotalOrders ?? 0) >= searchDto.MinOrders.Value);

                if (searchDto.MaxOrders.HasValue)
                    query = query.Where(c => (c.TotalOrders ?? 0) <= searchDto.MaxOrders.Value);

                // Apply sorting
                query = searchDto.SortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(c => GetSortProperty(c, searchDto.SortBy))
                    : query.OrderBy(c => GetSortProperty(c, searchDto.SortBy));

                // Apply pagination
                var results = query
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToList();

                return _mapper.Map<IEnumerable<CustomerListItemDto>>(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers");
                throw;
            }
        }

        private object GetSortProperty(Customer customer, string? sortBy)
        {
            return (sortBy ?? "").ToLower() switch
            {
                "phone" => (object)(customer.Phone ?? ""),
                "totalorders" => customer.TotalOrders ?? 0,
                "totalrevenue" => customer.TotalRevenue ?? 0m,
                "createdat" => customer.CreatedAt ?? DateTime.MinValue,
                _ => (object)(customer.FullName ?? "")
            };
        }

        // Address Management
        public async Task<IEnumerable<CustomerAddressDto>> GetCustomerAddressesAsync(int customerId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                var addresses = await _customerAddressRepository.GetAllAsync();
                var customerAddresses = addresses.Where(a => a.CustomerId == customerId).ToList();

                return _mapper.Map<IEnumerable<CustomerAddressDto>>(customerAddresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer addresses: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddressDto> GetDefaultAddressAsync(int customerId)
        {
            try
            {
                var addresses = await _customerAddressRepository.GetAllAsync();
                var defaultAddress = addresses.FirstOrDefault(a => a.CustomerId == customerId && a.IsDefault == true);

                if (defaultAddress == null)
                    throw new KeyNotFoundException($"Default address not found for customer {customerId}");

                return _mapper.Map<CustomerAddressDto>(defaultAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddressDto> AddAddressAsync(int customerId, CreateCustomerAddressDto dto)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                var address = _mapper.Map<CustomerAddress>(dto);
                address.CustomerId = customerId;
                address.CreatedAt = DateTime.UtcNow;

                var createdAddress = await _customerAddressRepository.AddAsync(address);
                await _customerAddressRepository.SaveChangesAsync();

                _logger.LogInformation("Address added for customer: {CustomerId}, AddressId: {AddressId}", customerId, createdAddress.AddressId);
                return _mapper.Map<CustomerAddressDto>(createdAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddressDto> UpdateAddressAsync(int addressId, UpdateCustomerAddressDto dto)
        {
            try
            {
                var address = await _customerAddressRepository.GetByIdAsync(addressId);
                if (address == null)
                    throw new KeyNotFoundException($"Address with ID {addressId} not found.");

                _mapper.Map(dto, address);

                await _customerAddressRepository.UpdateAsync(address);

                _logger.LogInformation("Address updated: {AddressId}", addressId);
                return _mapper.Map<CustomerAddressDto>(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address: {AddressId}", addressId);
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            try
            {
                var address = await _customerAddressRepository.GetByIdAsync(addressId);
                if (address == null)
                    throw new KeyNotFoundException($"Address with ID {addressId} not found.");

                await _customerAddressRepository.DeleteAsync(addressId);

                _logger.LogInformation("Address deleted: {AddressId}", addressId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address: {AddressId}", addressId);
                throw;
            }
        }

        public async Task<bool> SetDefaultAddressAsync(int customerId, int addressId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                var address = await _customerAddressRepository.GetByIdAsync(addressId);
                if (address == null || address.CustomerId != customerId)
                    throw new KeyNotFoundException($"Address with ID {addressId} not found for this customer.");

                var addresses = await _customerAddressRepository.GetAllAsync();
                var oldDefault = addresses.FirstOrDefault(a => a.CustomerId == customerId && a.IsDefault == true);
                
                if (oldDefault != null)
                {
                    oldDefault.IsDefault = false;
                    await _customerAddressRepository.UpdateAsync(oldDefault);
                }

                address.IsDefault = true;
                await _customerAddressRepository.UpdateAsync(address);

                _logger.LogInformation("Default address set for customer: {CustomerId}, AddressId: {AddressId}", customerId, addressId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address: {CustomerId}, {AddressId}", customerId, addressId);
                throw;
            }
        }
    }
}
