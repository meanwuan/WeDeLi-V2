using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IRatingRepository : IBaseRepository<Rating>
    {
        Task<IEnumerable<Rating>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Rating>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Rating>> GetByDriverIdAsync(int driverId);
        Task<decimal> GetAverageRatingAsync(int driverId);
    }
}
