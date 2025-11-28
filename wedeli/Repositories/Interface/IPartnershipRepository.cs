using wedeli.Models.Domain;
using wedeli.Models.DTO;

namespace wedeli.Repositories.Interface
{
    public interface IPartnershipRepository
    {
        // ===== Partnership CRUD =====
        Task<CompanyPartnership?> GetPartnershipByIdAsync(int partnershipId);
        Task<CompanyPartnership?> GetPartnershipAsync(int companyId, int partnerCompanyId);
        Task<(List<CompanyPartnership> Partnerships, int TotalCount)> GetPartnershipsAsync(PartnershipFilterDto filter);
        Task<List<CompanyPartnership>> GetCompanyPartnersAsync(int companyId);
        Task<List<CompanyPartnership>> GetPreferredPartnersAsync(int companyId);
        Task<CompanyPartnership> CreatePartnershipAsync(CompanyPartnership partnership);
        Task<CompanyPartnership> UpdatePartnershipAsync(CompanyPartnership partnership);
        Task<bool> DeletePartnershipAsync(int partnershipId);

        // ===== Partnership Statistics =====
        Task<PartnershipStatisticsDto> GetPartnershipStatisticsAsync(int partnershipId);
        Task UpdatePartnershipStatisticsAsync(int partnershipId);

        // ===== Helper Methods =====
        Task<bool> PartnershipExistsAsync(int companyId, int partnerCompanyId);
        Task<CompanyPartnership?> GetBestPartnerForOrderAsync(int companyId, decimal orderWeight);
    }
}