using wedeli.Models.DTO;

namespace wedeli.service.Interface
{
    public interface ICustomerService
    {
        // Customer CRUD
        Task<CustomerDto?> GetCustomerByIdAsync(int customerId);
        Task<CustomerDto?> GetCustomerByUserIdAsync(int userId);
        Task<CustomerDto?> GetCustomerByPhoneAsync(string phone);
        Task<(List<CustomerDto> Customers, int TotalCount)> GetCustomersAsync(CustomerFilterDto filter);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto);
        Task<bool> DeleteCustomerAsync(int customerId);

        // Address Management
        Task<List<CustomerAddressDto>> GetCustomerAddressesAsync(int customerId);
        Task<CustomerAddressDto?> GetDefaultAddressAsync(int customerId);
        Task<CustomerAddressDto> CreateAddressAsync(CreateCustomerAddressDto dto);
        Task<CustomerAddressDto> UpdateAddressAsync(int addressId, UpdateCustomerAddressDto dto);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> SetDefaultAddressAsync(int customerId, int addressId);

        // Order History
        Task<CustomerOrderHistoryDto> GetCustomerOrderHistoryAsync(int customerId, string? status = null);

        // Statistics
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId);

        // Customer Status
        Task<List<CustomerDto>> GetRegularCustomersAsync();
        Task<List<CustomerDto>> GetVipCustomersAsync(int minOrders = 10, decimal minRevenue = 10000000);
    }
}