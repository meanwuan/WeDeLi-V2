using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.COD;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Payment;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Payment service interface
    /// </summary>
    public interface IPaymentService
    {
        Task<PaymentResponseDto> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByOrderAsync(int orderId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByCustomerAsync(int customerId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByStatusAsync(string status, int? companyId = null);
        
        Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto);
        Task<bool> ProcessPaymentAsync(int paymentId, string transactionRef = null);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status);
        Task<bool> RefundPaymentAsync(int paymentId, string reason);
    }
}
