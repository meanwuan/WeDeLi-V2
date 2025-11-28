using wedeli.Models.DTO;

namespace wedeli.service.Interface
{
    public interface IPartnershipService
    {
        // Partnership CRUD
        Task<PartnershipDto?> GetPartnershipByIdAsync(int partnershipId);
        Task<(List<PartnershipDto> Partnerships, int TotalCount)> GetPartnershipsAsync(PartnershipFilterDto filter);
        Task<List<PartnershipDto>> GetCompanyPartnersAsync(int companyId);
        Task<PartnershipDto> CreatePartnershipAsync(CreatePartnershipDto dto, int createdBy);
        Task<PartnershipDto> UpdatePartnershipAsync(int partnershipId, UpdatePartnershipDto dto);
        Task<bool> DeletePartnershipAsync(int partnershipId);

        // Statistics
        Task<PartnershipStatisticsDto> GetPartnershipStatisticsAsync(int partnershipId);

        // Order Transfer
        Task<OrderTransferResponseDto> TransferOrderAsync(CreateOrderTransferDto dto, int transferredBy);
        Task<List<OrderTransferDto>> GetCompanyTransferHistoryAsync(int companyId);
    }
}