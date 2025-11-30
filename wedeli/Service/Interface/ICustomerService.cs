using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Customer;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Customer service interface for customer management
    /// </summary>
    public interface ICustomerService
    {
        Task<CustomerResponseDto> GetCustomerByIdAsync(int customerId);
        Task<CustomerDetailDto> GetCustomerDetailAsync(int customerId);
        Task<CustomerResponseDto> GetCustomerByUserIdAsync(int userId);
        Task<CustomerResponseDto> GetCustomerByPhoneAsync(string phone);
        Task<IEnumerable<CustomerListItemDto>> GetAllCustomersAsync(int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<CustomerResponseDto>> GetRegularCustomersAsync();
        Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerResponseDto> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto);
        Task<bool> UpdateRegularStatusAsync(int customerId);
        Task<bool> UpdatePaymentPrivilegeAsync(int customerId, string privilege);
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId);
        Task<IEnumerable<CustomerListItemDto>> SearchCustomersAsync(CustomerSearchDto searchDto);
        
        // Address management
        Task<IEnumerable<CustomerAddressDto>> GetCustomerAddressesAsync(int customerId);
        Task<CustomerAddressDto> GetDefaultAddressAsync(int customerId);
        Task<CustomerAddressDto> AddAddressAsync(int customerId, CreateCustomerAddressDto dto);
        Task<CustomerAddressDto> UpdateAddressAsync(int addressId, UpdateCustomerAddressDto dto);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> SetDefaultAddressAsync(int customerId, int addressId);
    }
}
