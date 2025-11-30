using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IPeriodicInvoiceRepository : IBaseRepository<PeriodicInvoice>
    {
        Task<IEnumerable<PeriodicInvoice>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<PeriodicInvoice>> GetByCompanyIdAsync(int companyId);
        Task<IEnumerable<PeriodicInvoice>> GetByStatusAsync(string status, int? companyId = null);
        Task<IEnumerable<PeriodicInvoice>> GetOverdueInvoicesAsync(int? companyId = null);
        Task<bool> UpdatePaymentAsync(int invoiceId, decimal paidAmount);
        Task<bool> UpdateStatusAsync(int invoiceId, string status);
        Task<PeriodicInvoice> GenerateInvoiceAsync(int customerId, int companyId, DateTime startDate, DateTime endDate);
    }
}
