using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Rating;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Complaint;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Complaint service interface
    /// </summary>
    public interface IComplaintService
    {
        Task<ComplaintResponseDto> GetComplaintByIdAsync(int complaintId);
        Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByOrderAsync(int orderId);
        Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByCustomerAsync(int customerId);
        Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByStatusAsync(string status, int? companyId = null);
        Task<IEnumerable<ComplaintResponseDto>> GetPendingComplaintsAsync(int? companyId = null);
        
        Task<ComplaintResponseDto> CreateComplaintAsync(CreateComplaintDto dto);
        Task<bool> UpdateComplaintStatusAsync(int complaintId, string status, string resolutionNotes = null, int? resolvedBy = null);
        Task<bool> ResolveComplaintAsync(ResolveComplaintDto dto);
        Task<bool> RejectComplaintAsync(int complaintId, string reason, int rejectedBy);
    }
}
