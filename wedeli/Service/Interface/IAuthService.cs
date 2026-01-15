using System;
using System.Threading.Tasks;
using wedeli.Models.DTO.Auth;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Authentication service interface for user login, registration, and token management
    /// </summary>
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<bool> LogoutAsync(LogoutRequestDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto dto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task<bool> ValidateTokenAsync(string token);
    }
}
