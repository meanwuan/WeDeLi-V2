using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IDriverRepository : IBaseRepository<Driver>
    {
        Task<Driver> GetByUserIdAsync(int userId);
        Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId);
        Task<IEnumerable<Driver>> GetActiveDriversAsync(int companyId);
        Task<bool> UpdateStatisticsAsync(int driverId, int totalTrips, decimal successRate, decimal rating);
        Task<bool> ToggleActiveStatusAsync(int driverId, bool isActive);
        Task<IEnumerable<Driver>> GetTopPerformingDriversAsync(int companyId, int topN = 10);
    }
}
