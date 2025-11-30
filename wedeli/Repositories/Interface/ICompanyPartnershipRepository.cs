using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ICompanyPartnershipRepository : IBaseRepository<CompanyPartnership>
    {
        Task<IEnumerable<CompanyPartnership>> GetByCompanyIdAsync(int companyId);
        Task<IEnumerable<CompanyPartnership>> GetPartnersByLevelAsync(int companyId, string level);
        Task<CompanyPartnership> GetPartnershipAsync(int companyId, int partnerCompanyId);
        Task<bool> UpdatePriorityAsync(int partnershipId, int priority);
        Task<bool> UpdateCommissionRateAsync(int partnershipId, decimal rate);
        Task<bool> IncrementTransferredOrdersAsync(int partnershipId);
        Task<IEnumerable<CompanyPartnership>> GetPreferredPartnersAsync(int companyId);
    }
}
