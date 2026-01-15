using System;
using System.Threading.Tasks;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Email service interface
    /// </summary>
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendOrderConfirmationAsync(int orderId);
        Task<bool> SendDeliveryNotificationAsync(int orderId);
        Task<bool> SendInvoiceAsync(int invoiceId);
        Task<bool> SendPasswordResetAsync(string email, string resetToken);
    }
}
