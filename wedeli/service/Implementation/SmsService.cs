using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly string _brandName;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();

            var smsSettings = _configuration.GetSection("SmsSettings");
            _apiUrl = smsSettings["ApiUrl"] ?? "";
            _apiKey = smsSettings["ApiKey"] ?? "";
            _brandName = smsSettings["BrandName"] ?? "WeDeLi";
        }

        /// <summary>
        /// Send SMS
        /// </summary>
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("SMS API key not configured");
                    return false;
                }

                // Format phone number (remove +84 and add 0)
                var formattedPhone = FormatPhoneNumber(phoneNumber);

                // Prepare SMS payload (example for Vietnamese SMS providers)
                var payload = new
                {
                    phone = formattedPhone,
                    message = message,
                    brandName = _brandName,
                    apiKey = _apiKey
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(_apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"SMS sent successfully to {formattedPhone}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send SMS to {formattedPhone}. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SMS to {phoneNumber}");
                return false;
            }
        }

        /// <summary>
        /// Send OTP SMS
        /// </summary>
        public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otp)
        {
            try
            {
                var message = $"Ma xac thuc WeDeLi cua ban la: {otp}. Ma co hieu luc trong 5 phut. Khong chia se ma nay voi bat ky ai.";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending OTP SMS to {phoneNumber}");
                return false;
            }
        }

        /// <summary>
        /// Send order confirmation SMS
        /// </summary>
        public async Task<bool> SendOrderConfirmationSmsAsync(string phoneNumber, string orderCode)
        {
            try
            {
                var message = $"WeDeLi: Don hang #{orderCode} da duoc tao thanh cong. Cam on quy khach da su dung dich vu cua chung toi!";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending order confirmation SMS to {phoneNumber}");
                return false;
            }
        }

        /// <summary>
        /// Send order status update SMS
        /// </summary>
        public async Task<bool> SendOrderStatusUpdateSmsAsync(string phoneNumber, string orderCode, string status)
        {
            try
            {
                var statusText = GetStatusText(status);
                var message = $"WeDeLi: Don hang #{orderCode} - {statusText}. Chi tiet: https://wedeli.com/tracking/{orderCode}";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending status update SMS to {phoneNumber}");
                return false;
            }
        }

        /// <summary>
        /// Send delivery notification SMS
        /// </summary>
        public async Task<bool> SendDeliveryNotificationSmsAsync(string phoneNumber, string orderCode, string driverName, string driverPhone)
        {
            try
            {
                var message = $"WeDeLi: Don hang #{orderCode} dang duoc giao boi {driverName} (SĐT: {driverPhone}). Vui long chuan bi nhan hang!";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending delivery notification SMS to {phoneNumber}");
                return false;
            }
        }

        /// <summary>
        /// Send COD payment reminder SMS
        /// </summary>
        public async Task<bool> SendCodPaymentReminderSmsAsync(string phoneNumber, string orderCode, decimal amount)
        {
            try
            {
                var message = $"WeDeLi: Nhac nho thanh toan COD cho don hang #{orderCode}. So tien: {amount:N0} VND. Vui long thanh toan khi nhan hang.";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending COD reminder SMS to {phoneNumber}");
                return false;
            }
        }

        /// <summary>
        /// Send promotional SMS
        /// </summary>
        public async Task<bool> SendPromotionalSmsAsync(string phoneNumber, string promotionMessage)
        {
            try
            {
                var message = $"WeDeLi: {promotionMessage}. Chi tiet tai https://wedeli.com/promotions";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending promotional SMS to {phoneNumber}");
                return false;
            }
        }

        /// <summary>
        /// Send bulk SMS to multiple recipients
        /// </summary>
        public async Task<Dictionary<string, bool>> SendBulkSmsAsync(List<string> phoneNumbers, string message)
        {
            var results = new Dictionary<string, bool>();

            foreach (var phoneNumber in phoneNumbers)
            {
                var success = await SendSmsAsync(phoneNumber, message);
                results[phoneNumber] = success;

                // Add delay to avoid rate limiting
                await Task.Delay(100);
            }

            var successCount = results.Count(r => r.Value);
            _logger.LogInformation($"Bulk SMS sent: {successCount}/{phoneNumbers.Count} successful");

            return results;
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        /// <summary>
        /// Format phone number to Vietnamese format
        /// </summary>
        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove spaces and special characters
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Convert +84 to 0
            if (cleaned.StartsWith("+84"))
            {
                cleaned = "0" + cleaned.Substring(3);
            }
            else if (cleaned.StartsWith("84"))
            {
                cleaned = "0" + cleaned.Substring(2);
            }

            // Ensure it starts with 0
            if (!cleaned.StartsWith("0"))
            {
                cleaned = "0" + cleaned;
            }

            return cleaned;
        }

        /// <summary>
        /// Get Vietnamese status text
        /// </summary>
        private string GetStatusText(string status)
        {
            return status switch
            {
                "pending_pickup" => "Cho lay hang",
                "picked_up" => "Da lay hang",
                "in_transit" => "Dang van chuyen",
                "at_warehouse" => "Dang tai kho",
                "out_for_delivery" => "Dang giao hang",
                "delivered" => "Da giao hang thanh cong",
                "failed_delivery" => "Giao hang that bai",
                "returned" => "Dang hoan tra",
                "cancelled" => "Da huy",
                _ => status
            };
        }

        /// <summary>
        /// Validate Vietnamese phone number
        /// </summary>
        public bool IsValidVietnamesePhone(string phoneNumber)
        {
            var formatted = FormatPhoneNumber(phoneNumber);

            // Vietnamese phone numbers start with 0 and have 10 digits
            // Mobile: 03, 05, 07, 08, 09
            // Landline: 02
            if (formatted.Length != 10 && formatted.Length != 11)
            {
                return false;
            }

            var validPrefixes = new[] { "03", "05", "07", "08", "09", "02" };
            return validPrefixes.Any(prefix => formatted.StartsWith(prefix));
        }

        /// <summary>
        /// Generate OTP code
        /// </summary>
        public string GenerateOtp(int length = 6)
        {
            var random = new Random();
            var otp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }

            return otp;
        }
    }
}