using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ICustomerAddressRepository : IBaseRepository<CustomerAddress>
    {
        Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(int customerId);
        Task<CustomerAddress> GetDefaultAddressAsync(int customerId);
        Task<bool> SetDefaultAddressAsync(int customerId, int addressId);
        Task<bool> IncrementUsageCountAsync(int addressId);
        Task<IEnumerable<CustomerAddress>> GetMostUsedAddressesAsync(int customerId, int topN = 5);
        Task SaveChangesAsync();
    }
}
