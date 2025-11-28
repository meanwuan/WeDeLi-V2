namespace wedeli.service.Interface
{
    public interface IEmailService
    {
        /// <summary>
        /// Send basic email
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Send email with attachments
        /// </summary>
        Task<bool> SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            List<string> attachmentPaths,
            bool isHtml = true);

        /// <summary>
        /// Send order confirmation email
        /// </summary>
        Task<bool> SendOrderConfirmationEmailAsync(string toEmail, string orderCode, string trackingUrl);

        /// <summary>
        /// Send order status update email
        /// </summary>
        Task<bool> SendOrderStatusUpdateEmailAsync(string toEmail, string orderCode, string status);

        /// <summary>
        /// Send password reset email
        /// </summary>
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl);

        /// <summary>
        /// Send welcome email to new users
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
    }
}