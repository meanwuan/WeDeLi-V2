using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Customer;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Customer address service interface
    /// </summary>
    public interface ICustomerAddressService
    {
        Task<IEnumerable<CustomerAddressDto>> GetAddressesAsync(int customerId);
        Task<CustomerAddressDto> GetAddressByIdAsync(int addressId);
        Task<CustomerAddressDto> CreateAddressAsync(CreateCustomerAddressDto dto, int customerId);
        Task<CustomerAddressDto> UpdateAddressAsync(int addressId, UpdateCustomerAddressDto dto);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> SetDefaultAddressAsync(int addressId, int customerId);
        Task<IEnumerable<CustomerAddressDto>> GetFrequentAddressesAsync(int customerId, int topN = 5);
    }
}
