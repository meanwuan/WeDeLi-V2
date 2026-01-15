using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        Task<Customer> GetByUserIdAsync(int userId);
        Task<Customer> GetByPhoneAsync(string phone);
        Task<IEnumerable<Customer>> GetRegularCustomersAsync();
        Task<bool> UpdateRegularStatusAsync(int customerId, bool isRegular);
        Task<bool> UpdatePaymentPrivilegeAsync(int customerId, string privilege);
        Task<bool> IncrementTotalOrdersAsync(int customerId);
        Task<bool> UpdateTotalRevenueAsync(int customerId, decimal amount);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task SaveChangesAsync();
    }
}
