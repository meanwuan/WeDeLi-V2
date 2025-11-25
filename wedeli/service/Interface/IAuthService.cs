using wedeli.Models.DTO;

namespace wedeli.service.Interface
{
    public interface IAuthService
    {
        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        Task<UserResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Refresh access token bằng refresh token
        /// </summary>
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);

        /// <summary>
        /// Quên mật khẩu - gửi OTP qua SMS
        /// </summary>
        Task<string> ForgotPasswordAsync(ForgotPasswordRequest request);

        /// <summary>
        /// Reset mật khẩu bằng token
        /// </summary>
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        /// <summary>
        /// Logout - revoke refresh token
        /// </summary>
        Task<bool> LogoutAsync(string refreshToken);

        /// <summary>
        /// Validate user credentials
        /// </summary>
        Task<bool> ValidateCredentialsAsync(string username, string password);
    }
}
