using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Models.DTO.Payment;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Service.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            try
            {
                var payment = _mapper.Map<Payment>(dto);
                payment.CreatedAt = DateTime.UtcNow;
                payment.PaymentStatus = "pending";

                var createdPayment = await _paymentRepository.AddAsync(payment);

                return _mapper.Map<PaymentResponseDto>(createdPayment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                throw;
            }
        }

        public async Task<PaymentResponseDto> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                return _mapper.Map<PaymentResponseDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByOrderAsync(int orderId)
        {
            try
            {
                var payments = await _paymentRepository.GetAllAsync();
                var filtered = payments.Where(p => p.OrderId == orderId);
                return _mapper.Map<IEnumerable<PaymentResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByCustomerAsync(int customerId)
        {
            try
            {
                var payments = await _paymentRepository.GetAllAsync();
                var filtered = payments.Where(p => p.CustomerId == customerId);
                return _mapper.Map<IEnumerable<PaymentResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByStatusAsync(string status, int? companyId)
        {
            try
            {
                var payments = await _paymentRepository.GetAllAsync();
                var filtered = payments.Where(p => p.PaymentStatus == status);

                return _mapper.Map<IEnumerable<PaymentResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by status: {Status}", status);
                throw;
            }
        }

        public async Task<bool> ProcessPaymentAsync(int paymentId, string transactionReference)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                    return false;

                payment.PaymentStatus = "completed";
                payment.TransactionRef = transactionReference;
                payment.PaidAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment);

                _logger.LogInformation("Payment processed: {PaymentId}", paymentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                    return false;

                payment.PaymentStatus = status;
                payment.CreatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<bool> RefundPaymentAsync(int paymentId, string reason)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                    return false;

                payment.PaymentStatus = "completed";
                payment.Notes = reason;

                await _paymentRepository.UpdateAsync(payment);

                _logger.LogInformation("Payment refunded: {PaymentId}, Reason: {Reason}", paymentId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment: {PaymentId}", paymentId);
                throw;
            }
        }
    }
}
