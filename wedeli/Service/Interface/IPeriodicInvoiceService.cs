using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.COD;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Payment;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Periodic invoice service interface
    /// </summary>
    public interface IPeriodicInvoiceService
    {
        Task<PeriodicInvoiceResponseDto> GetInvoiceByIdAsync(int invoiceId);
        Task<IEnumerable<PeriodicInvoiceResponseDto>> GetCustomerInvoicesAsync(int customerId);
        Task<IEnumerable<PeriodicInvoiceResponseDto>> GetCompanyInvoicesAsync(int companyId);
        Task<IEnumerable<PeriodicInvoiceResponseDto>> GetOverdueInvoicesAsync(int? companyId = null);
        
        Task<PeriodicInvoiceResponseDto> GenerateInvoiceAsync(GenerateInvoiceDto dto);
        Task<bool> ProcessInvoicePaymentAsync(int invoiceId, decimal amount, string paymentMethod);
        Task<bool> MarkInvoiceOverdueAsync(int invoiceId);
        Task<bool> CancelInvoiceAsync(int invoiceId, string reason);
    }
}