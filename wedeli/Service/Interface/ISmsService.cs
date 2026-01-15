using System;
using System.Threading.Tasks;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// SMS service interface
    /// </summary>
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> SendOrderStatusSmsAsync(int orderId, string status);
        Task<bool> SendOtpAsync(string phoneNumber, string otp);
    }
}
