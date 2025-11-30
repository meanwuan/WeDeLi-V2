using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Company;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Partnership;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Partnership service interface
    /// </summary>
    public interface IPartnershipService
    {
        Task<PartnershipResponseDto> GetPartnershipByIdAsync(int partnershipId);
        Task<IEnumerable<PartnershipResponseDto>> GetCompanyPartnershipsAsync(int companyId);
        Task<IEnumerable<PartnershipResponseDto>> GetPartnersByLevelAsync(int companyId, string level);
        Task<IEnumerable<PartnershipResponseDto>> GetPreferredPartnersAsync(int companyId);
        
        Task<PartnershipResponseDto> CreatePartnershipAsync(CreatePartnershipDto dto);
        Task<PartnershipResponseDto> UpdatePartnershipAsync(int partnershipId, UpdatePartnershipDto dto);
        Task<bool> DeletePartnershipAsync(int partnershipId);
        Task<bool> UpdatePriorityAsync(int partnershipId, int priority);
        Task<bool> UpdateCommissionRateAsync(int partnershipId, decimal rate);
    }
}
