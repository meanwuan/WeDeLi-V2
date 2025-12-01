using System.Threading.Tasks;
using wedeli.Service.Interface;

namespace wedeli.Service.Implementation
{
    public class SmsService : ISmsService
    {
        public Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            Console.WriteLine($"[MOCK SMS] To: {phoneNumber} - {message}");
            return Task.FromResult(true);
        }

        public Task<bool> SendOrderStatusSmsAsync(int orderId, string status)
        {
            return SendSmsAsync("USER_PHONE", $"Đơn hàng #{orderId}: {status}");
        }

        public Task<bool> SendOtpAsync(string phoneNumber, string otp)
        {
            return SendSmsAsync(phoneNumber, $"Mã OTP của bạn là: {otp}");
        }
    }
}
