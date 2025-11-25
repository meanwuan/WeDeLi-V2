using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Infrastructure;
using wedeli.Models.DTO;
using wedeli.service.Interface;

namespace wedeli.Controllers;

/// <summary>
/// Authentication Controller
/// Xử lý tất cả API endpoints liên quan đến authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Đăng nhập với username và password
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "username": "john_driver",
    ///         "password": "Password123!"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">Login credentials</param>
    /// <returns>Access token, refresh token và thông tin user</returns>
    /// <response code="200">Login thành công</response>
    /// <response code="400">Request không hợp lệ</response>
    /// <response code="401">Username hoặc password sai</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt from IP: {IP}", HttpContext.Connection.RemoteIpAddress);

            var response = await _authService.LoginAsync(request);

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Login validation error");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login unauthorized");
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///         "username": "john_driver",
    ///         "password": "Password123!",
    ///         "fullName": "John Doe",
    ///         "phone": "0987654321",
    ///         "email": "john@example.com",
    ///         "roleId": 2
    ///     }
    /// 
    /// Roles:
    /// - 1: admin
    /// - 2: driver
    /// - 3: warehouse_staff
    /// - 4: multi_role
    /// - 5: customer
    /// </remarks>
    /// <param name="request">Registration information</param>
    /// <returns>Thông tin user đã tạo</returns>
    /// <response code="201">Đăng ký thành công</response>
    /// <response code="400">Request không hợp lệ hoặc username/phone đã tồn tại</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for username: {Username}", request.Username);

            var user = await _authService.RegisterAsync(request);

            _logger.LogInformation("User {Username} registered successfully", request.Username);

            return CreatedAtAction(
                nameof(Register),
                new ApiResponse<UserResponse>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = user
                });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Registration validation error");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration business rule error");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during registration"
            });
        }
    }

    /// <summary>
    /// Refresh access token bằng refresh token
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/refresh-token
    ///     {
    ///         "refreshToken": "your_refresh_token_here"
    ///     }
    /// 
    /// Khi access token hết hạn (60 phút), dùng endpoint này để lấy access token mới
    /// mà không cần user login lại.
    /// </remarks>
    /// <param name="request">Refresh token</param>
    /// <returns>Access token mới và refresh token mới</returns>
    /// <response code="200">Refresh thành công</response>
    /// <response code="400">Refresh token không hợp lệ</response>
    /// <response code="401">Refresh token hết hạn hoặc đã bị revoke</response>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Refresh token attempt");

            var response = await _authService.RefreshTokenAsync(request);

            _logger.LogInformation("Token refreshed successfully");

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = response
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Refresh token validation error");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Refresh token unauthorized");
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token error");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during token refresh"
            });
        }
    }

    /// <summary>
    /// Đổi mật khẩu (yêu cầu đăng nhập)
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/change-password
    ///     {
    ///         "currentPassword": "OldPassword123!",
    ///         "newPassword": "NewPassword123!",
    ///         "confirmPassword": "NewPassword123!"
    ///     }
    /// 
    /// Yêu cầu: 
    /// - User phải đăng nhập (có JWT token)
    /// - Current password phải đúng
    /// - New password phải khác current password
    /// - New password và confirm password phải giống nhau
    /// - New password phải có ít nhất 6 ký tự
    /// </remarks>
    /// <param name="request">Password change information</param>
    /// <returns>Kết quả đổi mật khẩu</returns>
    /// <response code="200">Đổi mật khẩu thành công</response>
    /// <response code="400">Request không hợp lệ</response>
    /// <response code="401">Chưa đăng nhập hoặc current password sai</response>
    [HttpPost("change-password")]
    [Authorize] // YÊU CẦU ĐĂNG NHẬP
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Lấy userId từ JWT token
            var userId = User.GetUserId();
            
            if (!userId.HasValue)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            _logger.LogInformation("Change password attempt for user: {UserId}", userId);

            await _authService.ChangePasswordAsync(userId.Value, request);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Password changed successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Change password validation error");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Change password unauthorized");
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change password error");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during password change"
            });
        }
    }

    /// <summary>
    /// Quên mật khẩu - Gửi OTP qua SMS
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/forgot-password
    ///     {
    ///         "username": "john_driver",
    ///         "phone": "0987654321"
    ///     }
    /// 
    /// Hệ thống sẽ:
    /// 1. Kiểm tra username và phone có khớp không
    /// 2. Tạo OTP 6 chữ số
    /// 3. Gửi OTP qua SMS
    /// 4. OTP có hiệu lực trong 5 phút
    /// </remarks>
    /// <param name="request">Forgot password information</param>
    /// <returns>Thông báo đã gửi OTP</returns>
    /// <response code="200">OTP đã được gửi</response>
    /// <response code="400">Username hoặc phone không đúng</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Forgot password attempt for username: {Username}", request.Username);

            var message = await _authService.ForgotPasswordAsync(request);

            _logger.LogInformation("OTP sent successfully for username: {Username}", request.Username);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = message,
                Data = "OTP has been sent to your phone number"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Forgot password error");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password error");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while processing forgot password request"
            });
        }
    }

    /// <summary>
    /// Reset mật khẩu bằng OTP
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/reset-password
    ///     {
    ///         "token": "123456",
    ///         "newPassword": "NewPassword123!",
    ///         "confirmPassword": "NewPassword123!"
    ///     }
    /// 
    /// Sử dụng OTP nhận được từ SMS (endpoint forgot-password)
    /// OTP chỉ có hiệu lực trong 5 phút
    /// </remarks>
    /// <param name="request">Reset password information</param>
    /// <returns>Kết quả reset password</returns>
    /// <response code="200">Reset password thành công</response>
    /// <response code="400">Request không hợp lệ</response>
    /// <response code="401">OTP không đúng hoặc đã hết hạn</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Reset password attempt");

            await _authService.ResetPasswordAsync(request);

            _logger.LogInformation("Password reset successfully");

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Password reset successfully. You can now login with your new password."
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Reset password validation error");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Reset password unauthorized");
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reset password error");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during password reset"
            });
        }
    }

    /// <summary>
    /// Đăng xuất - Revoke refresh token
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/logout
    ///     {
    ///         "refreshToken": "your_refresh_token_here"
    ///     }
    /// 
    /// Hệ thống sẽ revoke refresh token để không thể dùng lại.
    /// Access token vẫn có hiệu lực cho đến khi hết hạn (60 phút).
    /// </remarks>
    /// <param name="request">Refresh token to revoke</param>
    /// <returns>Kết quả logout</returns>
    /// <response code="200">Logout thành công</response>
    [HttpPost("logout")]
    [Authorize] // YÊU CẦU ĐĂNG NHẬP
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Logout attempt for user: {UserId}", userId);

            await _authService.LogoutAsync(request.RefreshToken);

            _logger.LogInformation("User {UserId} logged out successfully", userId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logout successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during logout"
            });
        }
    }

    /// <summary>
    /// Lấy thông tin user hiện tại (từ JWT token)
    /// </summary>
    /// <returns>Thông tin user</returns>
    /// <response code="200">Thành công</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userId = User.GetUserId();
            var username = User.GetUsername();
            var fullName = User.GetFullName();
            var role = User.GetRole();
            var phone = User.GetPhone();
            var email = User.GetEmail();

            if (!userId.HasValue)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            return Ok(new ApiResponse<CurrentUserResponse>
            {
                Success = true,
                Message = "User information retrieved successfully",
                Data = new CurrentUserResponse
                {
                    UserId = userId.Value,
                    Username = username,
                    FullName = fullName,
                    Phone = phone,
                    Email = email,
                    Role = role
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving user information"
            });
        }
    }

    /// <summary>
    /// Health check endpoint - kiểm tra API có hoạt động không
    /// </summary>
    /// <returns>Status message</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Auth API is running",
            Data = $"Server time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
        });
    }
}