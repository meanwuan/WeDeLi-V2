namespace wedeli.service.Interface
{
    public interface ISmsService
    {
        /// <summary>
        /// Send basic SMS
        /// </summary>
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        /// <summary>
        /// Send OTP SMS for verification
        /// </summary>
        Task<bool> SendOtpSmsAsync(string phoneNumber, string otp);

        /// <summary>
        /// Send order confirmation SMS
        /// </summary>
        Task<bool> SendOrderConfirmationSmsAsync(string phoneNumber, string orderCode);

        /// <summary>
        /// Send order status update SMS
        /// </summary>
        Task<bool> SendOrderStatusUpdateSmsAsync(string phoneNumber, string orderCode, string status);

        /// <summary>
        /// Send delivery notification SMS (with driver info)
        /// </summary>
        Task<bool> SendDeliveryNotificationSmsAsync(string phoneNumber, string orderCode, string driverName, string driverPhone);

        /// <summary>
        /// Send COD payment reminder SMS
        /// </summary>
        Task<bool> SendCodPaymentReminderSmsAsync(string phoneNumber, string orderCode, decimal amount);

        /// <summary>
        /// Send promotional SMS
        /// </summary>
        Task<bool> SendPromotionalSmsAsync(string phoneNumber, string promotionMessage);

        /// <summary>
        /// Send SMS to multiple recipients
        /// </summary>
        Task<Dictionary<string, bool>> SendBulkSmsAsync(List<string> phoneNumbers, string message);

        /// <summary>
        /// Validate Vietnamese phone number format
        /// </summary>
        bool IsValidVietnamesePhone(string phoneNumber);

        /// <summary>
        /// Generate random OTP code
        /// </summary>
        string GenerateOtp(int length = 6);
    }
}