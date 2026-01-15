using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IComplaintRepository : IBaseRepository<Complaint>
    {
        Task<IEnumerable<Complaint>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Complaint>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Complaint>> GetByStatusAsync(string status, int? companyId = null);
        Task<IEnumerable<Complaint>> GetByTypeAsync(string complaintType, int? companyId = null);
        Task<bool> UpdateStatusAsync(int complaintId, string status, string resolutionNotes = null, int? resolvedBy = null);
        Task<IEnumerable<Complaint>> GetPendingComplaintsAsync(int? companyId = null);
        Task SaveChangesAsync();
    }
}
