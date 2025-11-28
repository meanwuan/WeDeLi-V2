using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        // ===== Customer CRUD =====

        public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<CustomerDto?> GetCustomerByUserIdAsync(int userId)
        {
            var customer = await _customerRepository.GetCustomerByUserIdAsync(userId);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<CustomerDto?> GetCustomerByPhoneAsync(string phone)
        {
            var customer = await _customerRepository.GetCustomerByPhoneAsync(phone);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<(List<CustomerDto> Customers, int TotalCount)> GetCustomersAsync(CustomerFilterDto filter)
        {
            var (customers, totalCount) = await _customerRepository.GetCustomersAsync(filter);
            var customerDtos = customers.Select(MapToDto).ToList();
            return (customerDtos, totalCount);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            // Validate phone uniqueness
            if (await _customerRepository.PhoneExistsAsync(dto.Phone))
            {
                throw new InvalidOperationException($"Phone number already exists: {dto.Phone}");
            }

            var customer = new Customer
            {
                UserId = dto.UserId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email
            };

            var createdCustomer = await _customerRepository.CreateCustomerAsync(customer);
            _logger.LogInformation($"Customer created successfully: {createdCustomer.CustomerId}");

            return MapToDto(createdCustomer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer not found: {customerId}");
            }

            if (!string.IsNullOrEmpty(dto.FullName))
                customer.FullName = dto.FullName;

            if (!string.IsNullOrEmpty(dto.Phone))
            {
                if (await _customerRepository.PhoneExistsAsync(dto.Phone, customerId))
                {
                    throw new InvalidOperationException($"Phone number already exists: {dto.Phone}");
                }
                customer.Phone = dto.Phone;
            }

            if (dto.Email != null)
                customer.Email = dto.Email;

            if (dto.IsRegular.HasValue)
                customer.IsRegular = dto.IsRegular.Value;

            if (!string.IsNullOrEmpty(dto.PaymentPrivilege))
                customer.PaymentPrivilege = dto.PaymentPrivilege;

            if (dto.CreditLimit.HasValue)
                customer.CreditLimit = dto.CreditLimit.Value;

            var updatedCustomer = await _customerRepository.UpdateCustomerAsync(customer);
            _logger.LogInformation($"Customer updated successfully: {customerId}");

            return MapToDto(updatedCustomer);
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var deleted = await _customerRepository.DeleteCustomerAsync(customerId);
            if (deleted)
            {
                _logger.LogInformation($"Customer deleted successfully: {customerId}");
            }
            return deleted;
        }

        // ===== Address Management =====

        public async Task<List<CustomerAddressDto>> GetCustomerAddressesAsync(int customerId)
        {
            var addresses = await _customerRepository.GetCustomerAddressesAsync(customerId);
            return addresses.Select(MapAddressToDto).ToList();
        }

        public async Task<CustomerAddressDto?> GetDefaultAddressAsync(int customerId)
        {
            var address = await _customerRepository.GetDefaultAddressAsync(customerId);
            return address == null ? null : MapAddressToDto(address);
        }

        public async Task<CustomerAddressDto> CreateAddressAsync(CreateCustomerAddressDto dto)
        {
            var address = new CustomerAddress
            {
                CustomerId = dto.CustomerId,
                AddressLabel = dto.AddressLabel,
                FullAddress = dto.FullAddress,
                Province = dto.Province,
                District = dto.District,
                Ward = dto.Ward,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsDefault = dto.IsDefault
            };

            var createdAddress = await _customerRepository.CreateAddressAsync(address);
            _logger.LogInformation($"Address created for customer: {dto.CustomerId}");

            return MapAddressToDto(createdAddress);
        }

        public async Task<CustomerAddressDto> UpdateAddressAsync(int addressId, UpdateCustomerAddressDto dto)
        {
            var address = await _customerRepository.GetAddressByIdAsync(addressId);
            if (address == null)
            {
                throw new KeyNotFoundException($"Address not found: {addressId}");
            }

            if (!string.IsNullOrEmpty(dto.AddressLabel))
                address.AddressLabel = dto.AddressLabel;

            if (!string.IsNullOrEmpty(dto.FullAddress))
                address.FullAddress = dto.FullAddress;

            if (dto.Province != null)
                address.Province = dto.Province;

            if (dto.District != null)
                address.District = dto.District;

            if (dto.Ward != null)
                address.Ward = dto.Ward;

            if (dto.Latitude.HasValue)
                address.Latitude = dto.Latitude.Value;

            if (dto.Longitude.HasValue)
                address.Longitude = dto.Longitude.Value;

            if (dto.IsDefault.HasValue)
                address.IsDefault = dto.IsDefault.Value;

            var updatedAddress = await _customerRepository.UpdateAddressAsync(address);
            _logger.LogInformation($"Address updated: {addressId}");

            return MapAddressToDto(updatedAddress);
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            var deleted = await _customerRepository.DeleteAddressAsync(addressId);
            if (deleted)
            {
                _logger.LogInformation($"Address deleted: {addressId}");
            }
            return deleted;
        }

        public async Task<bool> SetDefaultAddressAsync(int customerId, int addressId)
        {
            return await _customerRepository.SetDefaultAddressAsync(customerId, addressId);
        }

        // ===== Order History =====

        public async Task<CustomerOrderHistoryDto> GetCustomerOrderHistoryAsync(int customerId, string? status = null)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer not found: {customerId}");
            }

            var orders = await _customerRepository.GetCustomerOrdersAsync(customerId, status);

            return new CustomerOrderHistoryDto
            {
                CustomerId = customerId,
                FullName = customer.FullName,
                TotalOrders = orders.Count,
                Orders = orders.Select(o => new OrderListItem
                {

                    OrderId = o.OrderId,
                    TrackingCode = o.TrackingCode,
                    SenderName = o.SenderName,
                    SenderPhone = o.SenderPhone,
                    ReceiverName = o.ReceiverName,
                    ReceiverPhone = o.ReceiverPhone,
                    ReceiverAddress = o.ReceiverAddress,
                    ReceiverProvince = o.ReceiverProvince,
                    ShippingFee = o.ShippingFee,
                    CodAmount = o.CodAmount ?? 0,
                    OrderStatus = o.OrderStatus ?? "pending_pickup",
                    PaymentStatus = o.PaymentStatus ?? "unpaid",
                    DriverName = o.Driver?.User?.FullName,
                    RouteName = o.Route?.RouteName,
                    CreatedAt = o.CreatedAt ?? DateTime.UtcNow
                }).ToList()
            };
        }

        // ===== Statistics =====

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId)
        {
            return await _customerRepository.GetCustomerStatisticsAsync(customerId);
        }

        // ===== Customer Status =====

        public async Task<List<CustomerDto>> GetRegularCustomersAsync()
        {
            var customers = await _customerRepository.GetRegularCustomersAsync();
            return customers.Select(MapToDto).ToList();
        }

        public async Task<List<CustomerDto>> GetVipCustomersAsync(int minOrders = 10, decimal minRevenue = 10000000)
        {
            var customers = await _customerRepository.GetVipCustomersAsync(minOrders, minRevenue);
            return customers.Select(MapToDto).ToList();
        }

        // ===== Helper Methods =====

        private CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                UserId = customer.UserId,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Email = customer.Email,
                IsRegular = customer.IsRegular == true,
                TotalOrders = customer.TotalOrders ?? 0,
                TotalRevenue = customer.TotalRevenue ?? 0,
                PaymentPrivilege = customer.PaymentPrivilege ?? "prepay",
                CreditLimit = customer.CreditLimit ?? 0,
                CreatedAt = customer.CreatedAt ?? DateTime.Now,
                UpdatedAt = customer.UpdatedAt ?? DateTime.Now
            };
        }

        private CustomerAddressDto MapAddressToDto(CustomerAddress address)
        {
            return new CustomerAddressDto
            {
                AddressId = address.AddressId,
                CustomerId = address.CustomerId,
                AddressLabel = address.AddressLabel,
                FullAddress = address.FullAddress,
                Province = address.Province,
                District = address.District,
                Ward = address.Ward,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                IsDefault = (bool)address.IsDefault,
                UsageCount = address.UsageCount ?? 0,
                CreatedAt = address.CreatedAt ?? DateTime.Now
            };
        }
    }
}