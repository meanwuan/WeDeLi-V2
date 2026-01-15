using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Company;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Partnership;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Order transfer service interface for partner company transfers
    /// </summary>
    public interface IOrderTransferService
    {
        Task<OrderTransferResponseDto> GetTransferByIdAsync(int transferId);
        Task<IEnumerable<OrderTransferResponseDto>> GetTransfersByOrderAsync(int orderId);
        Task<IEnumerable<OrderTransferResponseDto>> GetCompanyTransfersAsync(int companyId, bool outgoing = true);
        Task<IEnumerable<OrderTransferResponseDto>> GetPendingTransfersAsync(int companyId);
        
        Task<OrderTransferResponseDto> TransferOrderAsync(TransferOrderDto dto);
        Task<bool> AcceptTransferAsync(int transferId, int? newVehicleId = null);
        Task<bool> RejectTransferAsync(int transferId, string reason);
        
    }

}