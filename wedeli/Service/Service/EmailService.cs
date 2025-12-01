using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using wedeli.Service.Interface;

namespace wedeli.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly SmtpClient _smtpClient;
        private readonly string _from;

        public EmailService(IConfiguration config)
        {
            _config = config;
            var smtpSettings = _config.GetSection("Smtp");

            _from = smtpSettings["From"];

            _smtpClient = new SmtpClient
            {
                Host = smtpSettings["Host"],
                Port = int.Parse(smtpSettings["Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    smtpSettings["Username"],
                    smtpSettings["Password"]
                )
            };
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            var mail = new MailMessage(_from, to, subject, body);
            mail.IsBodyHtml = true;

            try
            {
                await _smtpClient.SendMailAsync(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendOrderConfirmationAsync(int orderId)
        {
            string subject = $"Xác nhận đơn hàng #{orderId}";
            string body = $"<h3>Đơn hàng #{orderId} đã được xác nhận.</h3>";
            return await SendEmailAsync(_from, subject, body);
        }

        public async Task<bool> SendDeliveryNotificationAsync(int orderId)
        {
            string subject = $"Đơn hàng #{orderId} đang giao";
            string body = $"<p>Tài xế đang giao đơn hàng #{orderId}.</p>";
            return await SendEmailAsync(_from, subject, body);
        }

        public async Task<bool> SendInvoiceAsync(int invoiceId)
        {
            string subject = $"Hóa đơn #{invoiceId}";
            string body = $"<p>Hóa đơn #{invoiceId} đã sẵn sàng.</p>";
            return await SendEmailAsync(_from, subject, body);
        }

        public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
        {
            string subject = "Khôi phục mật khẩu";
            string body = $"Nhấn vào link để đặt lại mật khẩu:<br/><a href='https://yourdomain.com/reset?token={resetToken}'>Reset</a>";
            return await SendEmailAsync(email, subject, body);
        }
    }
}
