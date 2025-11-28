using wedeli.Models.DTO;

namespace wedeli.service.Interface
{
    public interface IOrderTransferService
    {
        Task<OrderTransferDto> CreateTransferAsync(CreateOrderTransferDto dto, int userId);
        Task<List<OrderTransferDto>> GetCompanyTransfersAsync(int companyId, string? status);
        Task<OrderTransferDto> AcceptTransferAsync(int transferId, int userId, int? vehicleId);
        Task<bool> RejectTransferAsync(int transferId, int userId, string reason);
        Task<List<OrderTransferDto>> GetPendingTransfersAsync(int companyId);
        Task<OrderTransferDto> GetTransferByIdAsync(int transferId);
    }
}