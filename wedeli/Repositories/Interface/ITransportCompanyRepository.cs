using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ITransportCompanyRepository : IBaseRepository<TransportCompany>
    {
        Task<IEnumerable<TransportCompany>> GetActiveCompaniesAsync();
        Task<bool> UpdateRatingAsync(int companyId, decimal rating);
        Task<TransportCompany> GetByNameAsync(string companyName);
        Task<TransportCompany?> GetByUserIdAsync(int userId);
        Task SaveChangesAsync();
    }
}
