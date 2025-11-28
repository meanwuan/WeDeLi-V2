using wedeli.Models.Domain;
using wedeli.Models.DTO;

namespace wedeli.Repositories.Interface
{
    public interface ICustomerRepository
    {
        // ===== Customer CRUD Operations =====
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer?> GetCustomerByUserIdAsync(int userId);
        Task<Customer?> GetCustomerByPhoneAsync(string phone);
        Task<(List<Customer> Customers, int TotalCount)> GetCustomersAsync(CustomerFilterDto filter);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int customerId);

        // ===== Address Management =====
        Task<CustomerAddress?> GetAddressByIdAsync(int addressId);
        Task<List<CustomerAddress>> GetCustomerAddressesAsync(int customerId);
        Task<CustomerAddress?> GetDefaultAddressAsync(int customerId);
        Task<CustomerAddress> CreateAddressAsync(CustomerAddress address);
        Task<CustomerAddress> UpdateAddressAsync(CustomerAddress address);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> SetDefaultAddressAsync(int customerId, int addressId);
        Task IncrementAddressUsageAsync(int addressId);

        // ===== Order History =====
        Task<List<Order>> GetCustomerOrdersAsync(int customerId, string? status = null);
        Task<int> GetCustomerOrderCountAsync(int customerId);

        // ===== Statistics =====
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId);
        Task UpdateCustomerStatisticsAsync(int customerId);

        // ===== Customer Status (Regular/VIP) =====
        Task<bool> UpdateCustomerStatusAsync(int customerId);
        Task<List<Customer>> GetRegularCustomersAsync();
        Task<List<Customer>> GetVipCustomersAsync(int minOrders = 10, decimal minRevenue = 10000000);

        // ===== Helper Methods =====
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> PhoneExistsAsync(string phone, int? excludeCustomerId = null);
    }
}