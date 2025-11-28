using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var emailSettings = _configuration.GetSection("EmailSettings");
            _smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            _smtpUsername = emailSettings["SmtpUsername"] ?? "";
            _smtpPassword = emailSettings["SmtpPassword"] ?? "";
            _fromEmail = emailSettings["FromEmail"] ?? "";
            _fromName = emailSettings["FromName"] ?? "WeDeLi";
            _enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
        }

        /// <summary>
        /// Send email
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("SMTP credentials not configured");
                    return false;
                }

                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = _enableSsl;

                await smtpClient.SendMailAsync(message);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Send email with attachments
        /// </summary>
        public async Task<bool> SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            List<string> attachmentPaths,
            bool isHtml = true)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("SMTP credentials not configured");
                    return false;
                }

                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                // Add attachments
                foreach (var attachmentPath in attachmentPaths)
                {
                    if (File.Exists(attachmentPath))
                    {
                        var attachment = new Attachment(attachmentPath);
                        message.Attachments.Add(attachment);
                    }
                    else
                    {
                        _logger.LogWarning($"Attachment not found: {attachmentPath}");
                    }
                }

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = _enableSsl;

                await smtpClient.SendMailAsync(message);

                _logger.LogInformation($"Email with attachments sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email with attachments to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Send order confirmation email
        /// </summary>
        public async Task<bool> SendOrderConfirmationEmailAsync(string toEmail, string orderCode, string trackingUrl)
        {
            try
            {
                var subject = $"Xác nhận đơn hàng #{orderCode} - WeDeLi";
                var body = GetOrderConfirmationEmailTemplate(orderCode, trackingUrl);

                return await SendEmailAsync(toEmail, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending order confirmation email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Send order status update email
        /// </summary>
        public async Task<bool> SendOrderStatusUpdateEmailAsync(string toEmail, string orderCode, string status)
        {
            try
            {
                var subject = $"Cập nhật trạng thái đơn hàng #{orderCode} - WeDeLi";
                var body = GetOrderStatusUpdateEmailTemplate(orderCode, status);

                return await SendEmailAsync(toEmail, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending order status update email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl)
        {
            try
            {
                var subject = "Đặt lại mật khẩu - WeDeLi";
                var body = GetPasswordResetEmailTemplate(resetToken, resetUrl);

                return await SendEmailAsync(toEmail, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending password reset email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Send welcome email
        /// </summary>
        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                var subject = "Chào mừng bạn đến với WeDeLi!";
                var body = GetWelcomeEmailTemplate(userName);

                return await SendEmailAsync(toEmail, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending welcome email to {toEmail}");
                return false;
            }
        }

        // ============================================
        // EMAIL TEMPLATES
        // ============================================

        private string GetOrderConfirmationEmailTemplate(string orderCode, string trackingUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff; }}
        .header {{ background-color: #4CAF50; color: white; padding: 30px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .order-info {{ background-color: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Xác nhận đơn hàng</h1>
        </div>
        <div class='content'>
            <h2>Cảm ơn bạn đã sử dụng dịch vụ WeDeLi!</h2>
            <p>Đơn hàng của bạn đã được tiếp nhận thành công.</p>
            
            <div class='order-info'>
                <h3>Thông tin đơn hàng:</h3>
                <p><strong>Mã đơn hàng:</strong> {orderCode}</p>
                <p><strong>Trạng thái:</strong> Đang xử lý</p>
            </div>
            
            <p>Bạn có thể theo dõi đơn hàng của mình tại:</p>
            <a href='{trackingUrl}' class='button'>Theo dõi đơn hàng</a>
            
            <p style='margin-top: 30px;'>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc hotline.</p>
        </div>
        <div class='footer'>
            <p>© 2025 WeDeLi - Hệ thống quản lý giao hàng</p>
            <p>Email: support@wedeli.com | Hotline: 1900-xxxx</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetOrderStatusUpdateEmailTemplate(string orderCode, string status)
        {
            var statusText = status switch
            {
                "pending_pickup" => "Chờ lấy hàng",
                "picked_up" => "Đã lấy hàng",
                "in_transit" => "Đang vận chuyển",
                "out_for_delivery" => "Đang giao hàng",
                "delivered" => "Đã giao hàng",
                "cancelled" => "Đã hủy",
                _ => status
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff; }}
        .header {{ background-color: #2196F3; color: white; padding: 30px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .status-badge {{ display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; border-radius: 20px; font-weight: bold; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📦 Cập nhật đơn hàng</h1>
        </div>
        <div class='content'>
            <h2>Đơn hàng #{orderCode}</h2>
            <p>Trạng thái đơn hàng của bạn đã được cập nhật:</p>
            
            <p style='text-align: center; margin: 30px 0;'>
                <span class='status-badge'>{statusText}</span>
            </p>
            
            <p>Cảm ơn bạn đã tin tưởng sử dụng dịch vụ WeDeLi!</p>
        </div>
        <div class='footer'>
            <p>© 2025 WeDeLi - Hệ thống quản lý giao hàng</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetEmailTemplate(string resetToken, string resetUrl)
        {
            var fullResetUrl = $"{resetUrl}?token={resetToken}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff; }}
        .header {{ background-color: #FF5722; color: white; padding: 30px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #FF5722; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .warning {{ background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔑 Đặt lại mật khẩu</h1>
        </div>
        <div class='content'>
            <h2>Yêu cầu đặt lại mật khẩu</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            
            <p>Nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
            <a href='{fullResetUrl}' class='button'>Đặt lại mật khẩu</a>
            
            <div class='warning'>
                <strong>⚠️ Lưu ý:</strong> Link này chỉ có hiệu lực trong 1 giờ.
            </div>
            
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        </div>
        <div class='footer'>
            <p>© 2025 WeDeLi - Hệ thống quản lý giao hàng</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff; }}
        .header {{ background-color: #9C27B0; color: white; padding: 30px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .features {{ background-color: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .feature-item {{ padding: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Chào mừng đến với WeDeLi!</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {userName}!</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại WeDeLi - hệ thống quản lý giao hàng hiện đại.</p>
            
            <div class='features'>
                <h3>✨ Tính năng nổi bật:</h3>
                <div class='feature-item'>📦 Quản lý đơn hàng dễ dàng</div>
                <div class='feature-item'>🚚 Theo dõi hành trình real-time</div>
                <div class='feature-item'>💰 Thanh toán linh hoạt (COD, chuyển khoản)</div>
                <div class='feature-item'>📱 Thông báo tức thời qua SMS & Email</div>
                <div class='feature-item'>⭐ Đánh giá và phản hồi dịch vụ</div>
            </div>
            
            <p>Hãy bắt đầu trải nghiệm dịch vụ của chúng tôi ngay hôm nay!</p>
        </div>
        <div class='footer'>
            <p>© 2025 WeDeLi - Hệ thống quản lý giao hàng</p>
            <p>Email: support@wedeli.com | Hotline: 1900-xxxx</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}