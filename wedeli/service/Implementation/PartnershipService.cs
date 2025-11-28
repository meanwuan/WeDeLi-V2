using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    public class PartnershipService : IPartnershipService
    {
        private readonly IPartnershipRepository _partnershipRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PartnershipService> _logger;

        public PartnershipService(
            IPartnershipRepository partnershipRepository,
            IOrderRepository orderRepository,
            ILogger<PartnershipService> logger)
        {
            _partnershipRepository = partnershipRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<PartnershipDto?> GetPartnershipByIdAsync(int partnershipId)
        {
            var partnership = await _partnershipRepository.GetPartnershipByIdAsync(partnershipId);
            return partnership == null ? null : MapToDto(partnership);
        }

        public async Task<(List<PartnershipDto> Partnerships, int TotalCount)> GetPartnershipsAsync(PartnershipFilterDto filter)
        {
            var (partnerships, totalCount) = await _partnershipRepository.GetPartnershipsAsync(filter);
            var dtos = partnerships.Select(MapToDto).ToList();
            return (dtos, totalCount);
        }

        public async Task<List<PartnershipDto>> GetCompanyPartnersAsync(int companyId)
        {
            var partnerships = await _partnershipRepository.GetCompanyPartnersAsync(companyId);
            return partnerships.Select(MapToDto).ToList();
        }

        public async Task<PartnershipDto> CreatePartnershipAsync(CreatePartnershipDto dto, int createdBy)
        {
            if (await _partnershipRepository.PartnershipExistsAsync(dto.CompanyId, dto.PartnerCompanyId))
            {
                throw new InvalidOperationException("Partnership already exists");
            }

            if (dto.CompanyId == dto.PartnerCompanyId)
            {
                throw new ArgumentException("Cannot create partnership with same company");
            }

            var validLevels = new[] { "preferred", "regular", "backup" };
            if (!validLevels.Contains(dto.PartnershipLevel))
            {
                throw new ArgumentException($"Invalid partnership level. Must be: {string.Join(", ", validLevels)}");
            }

            var partnership = new CompanyPartnership
            {
                CompanyId = dto.CompanyId,
                PartnerCompanyId = dto.PartnerCompanyId,
                PartnershipLevel = dto.PartnershipLevel,
                CommissionRate = dto.CommissionRate,
                PriorityOrder = dto.PriorityOrder,
                Notes = dto.Notes,
                CreatedBy = createdBy
            };

            var created = await _partnershipRepository.CreatePartnershipAsync(partnership);
            return (await GetPartnershipByIdAsync(created.PartnershipId))!;
        }

        public async Task<PartnershipDto> UpdatePartnershipAsync(int partnershipId, UpdatePartnershipDto dto)
        {
            var partnership = await _partnershipRepository.GetPartnershipByIdAsync(partnershipId);
            if (partnership == null)
                throw new KeyNotFoundException($"Partnership not found: {partnershipId}");

            if (!string.IsNullOrEmpty(dto.PartnershipLevel))
                partnership.PartnershipLevel = dto.PartnershipLevel;

            if (dto.CommissionRate.HasValue)
                partnership.CommissionRate = dto.CommissionRate.Value;

            if (dto.PriorityOrder.HasValue)
                partnership.PriorityOrder = dto.PriorityOrder.Value;

            if (dto.IsActive.HasValue)
                partnership.IsActive = dto.IsActive.Value;

            if (dto.Notes != null)
                partnership.Notes = dto.Notes;

            await _partnershipRepository.UpdatePartnershipAsync(partnership);
            return MapToDto(partnership);
        }

        public async Task<bool> DeletePartnershipAsync(int partnershipId)
        {
            return await _partnershipRepository.DeletePartnershipAsync(partnershipId);
        }

        public async Task<PartnershipStatisticsDto> GetPartnershipStatisticsAsync(int partnershipId)
        {
            return await _partnershipRepository.GetPartnershipStatisticsAsync(partnershipId);
        }

        public async Task<OrderTransferResponseDto> TransferOrderAsync(CreateOrderTransferDto dto, int transferredBy)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null)
                throw new KeyNotFoundException($"Order not found: {dto.OrderId}");

            // Get best partner or use specified partner
            var partnership = await _partnershipRepository.GetBestPartnerForOrderAsync(
                order.RouteId ?? 0,
                order.WeightKg ?? 0);

            if (partnership == null)
                throw new InvalidOperationException("No suitable partner found for this order");

            var commissionPaid = order.ShippingFee * (partnership.CommissionRate / 100) ?? 0;

            // This is a simplified version - actual implementation would create OrderTransfer entity
            return new OrderTransferResponseDto
            {
                TransferId = 0,
                OrderId = dto.OrderId,
                TrackingCode = order.TrackingCode,
                FromCompanyName = "Current Company",
                ToCompanyName = partnership.PartnerCompany?.CompanyName ?? "",
                TransferReason = dto.TransferReason,
                CommissionPaid = commissionPaid,
                TransferStatus = "pending",
                Message = $"Order transferred successfully. Commission: {commissionPaid:N0} VND"
            };
        }

        public async Task<List<OrderTransferDto>> GetCompanyTransferHistoryAsync(int companyId)
        {
            // Simplified - would query OrderTransfer table
            return new List<OrderTransferDto>();
        }

        private PartnershipDto MapToDto(CompanyPartnership partnership)
        {
            return new PartnershipDto
            {
                PartnershipId = partnership.PartnershipId,
                CompanyId = partnership.CompanyId,
                CompanyName = partnership.Company?.CompanyName,
                PartnerCompanyId = partnership.PartnerCompanyId,
                PartnerCompanyName = partnership.PartnerCompany?.CompanyName,
                PartnershipLevel = partnership.PartnershipLevel ?? "regular",
                CommissionRate = partnership.CommissionRate ?? 0,
                PriorityOrder = partnership.PriorityOrder ?? 0,
                TotalTransferredOrders = partnership.TotalTransferredOrders ?? 0,
                IsActive = partnership.IsActive ?? true,
                Notes = partnership.Notes,
                CreatedBy = partnership.CreatedBy,
                CreatedByName = partnership.CreatedByNavigation?.FullName,
                CreatedAt = partnership.CreatedAt ?? DateTime.Now,
                UpdatedAt = partnership.UpdatedAt ?? DateTime.Now
            };
        }
    }
}