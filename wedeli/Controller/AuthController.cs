using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Exceptions;
using wedeli.Models.DTO.Auth;
using wedeli.Models.Response;
using wedeli.Repositories;
using wedeli.Service;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    /// <summary>
    /// Authentication Controller - Handles user authentication and authorization
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/register
        ///     {
        ///         "username": "john_doe",
        ///         "email": "john@example.com",
        ///         "phone": "0123456789",
        ///         "fullName": "John Doe",
        ///         "password": "Password@123",
        ///         "roleId": 5
        ///     }
        /// </remarks>
        /// <param name="registerRequest">Register request details</param>
        /// <returns>Register response with user details</returns>
        /// <response code="201">User registered successfully</response>
        /// <response code="400">Invalid input or password not strong enough</response>
        /// <response code="409">Username, email, or phone already exists</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            try
            {
                _logger.LogInformation($"Register attempt for username: {registerRequest.Username}");

                // Validate request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    _logger.LogWarning($"Register validation failed for {registerRequest.Username}: {string.Join(", ", errors)}");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data: " + string.Join(", ", errors),
                        Errors = errors
                    });
                }

                // Call service
                var result = await _authService.RegisterAsync(registerRequest);

                _logger.LogInformation($"User registered successfully: {registerRequest.Username}");

                return CreatedAtAction(nameof(Register), new ApiResponse<RegisterResponseDto>
                {
                    Success = true,
                    Message = "User registered successfully. Please verify your email.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Register error: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Login user and get access token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/login
        ///     {
        ///         "emailOrUsername": "john@example.com",
        ///         "password": "Password@123"
        ///     }
        /// </remarks>
        /// <param name="loginRequest">Login credentials</param>
        /// <returns>Login response with tokens</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="401">Unauthorized - invalid email/username or password</response>
        /// <response code="403">Account deactivated</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                _logger.LogInformation($"Login attempt for: {loginRequest.EmailOrUsername}");

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Call service
                var result = await _authService.LoginAsync(loginRequest);

                _logger.LogInformation($"Login successful for user: {result.Username}");

                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/refresh-token
        ///     {
        ///         "accessToken": "eyJhbGciOiJIUzI1NiIs...",
        ///         "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
        ///     }
        /// </remarks>
        /// <param name="refreshRequest">Tokens for refresh</param>
        /// <returns>New access and refresh tokens</returns>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="401">Invalid or expired refresh token</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshRequest)
        {
            try
            {
                _logger.LogInformation("Refresh token attempt");

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Call service
                var result = await _authService.RefreshTokenAsync(refreshRequest);

                _logger.LogInformation("Token refreshed successfully");

                return Ok(new ApiResponse<RefreshTokenResponseDto>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Refresh token failed: {ex.Message}");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Refresh token error: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/forgot-password
        ///     {
        ///         "email": "john@example.com"
        ///     }
        /// </remarks>
        /// <param name="forgotPasswordRequest">Email address</param>
        /// <returns>Confirmation message</returns>
        /// <response code="200">Reset email sent successfully</response>
        /// <response code="400">Invalid email address</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordRequest)
        {
            try
            {
                _logger.LogInformation($"Forgot password request for: {forgotPasswordRequest.Email}");

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Call service
                var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);

                _logger.LogInformation($"Forgot password email sent for: {forgotPasswordRequest.Email}");

                // Always return success for security (don't reveal if email exists)
                return Ok(new ApiResponse<object?>
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Forgot password error: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Reset password with reset token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/reset-password
        ///     {
        ///         "resetToken": "eyJhbGciOiJIUzI1NiIs...",
        ///         "newPassword": "NewPassword@123",
        ///         "confirmPassword": "NewPassword@123"
        ///     }
        /// </remarks>
        /// <param name="resetPasswordRequest">Reset token and new password</param>
        /// <returns>Confirmation message</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid password or token expired</response>
        /// <response code="401">Invalid or expired reset token</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequest)
        {
            try
            {
                _logger.LogInformation("Reset password attempt");

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Validate password match
                if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmPassword)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Passwords do not match"
                    });
                }

                // Call service
                var result = await _authService.ResetPasswordAsync(resetPasswordRequest);

                _logger.LogInformation("Password reset successfully");

                return Ok(new ApiResponse<object?>
                {
                    Success = true,
                    Message = "Password reset successfully. Please login with your new password.",
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Reset password failed: {ex.Message}");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Verify email with verification token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/verify-email
        ///     {
        ///         "email": "john@example.com",
        ///         "verificationCode": "123456"
        ///     }
        /// </remarks>
        /// <param name="verifyEmailRequest">Email and verification code</param>
        /// <returns>Confirmation message</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid verification code</response>
        /// <response code="404">User not found</response>

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/logout
        ///     {
        ///         "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
        ///     }
        /// </remarks>
        /// <param name="logoutRequest">Refresh token to revoke</param>
        /// <returns>Confirmation message</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="400">Invalid refresh token</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto logoutRequest)
        {
            try
            {
                _logger.LogInformation("Logout attempt");

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Call service
                var result = await _authService.LogoutAsync(logoutRequest);

                if (!result)
                {
                    _logger.LogWarning("Logout failed");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to logout"
                    });
                }

                _logger.LogInformation("Logout successful");

                return Ok(new ApiResponse<object?>
                {
                    Success = true,
                    Message = "Logout successful",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Change password for logged-in user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/change-password
        ///     {
        ///         "currentPassword": "OldPassword@123",
        ///         "newPassword": "NewPassword@456",
        ///         "confirmPassword": "NewPassword@456"
        ///     }
        /// </remarks>
        /// <param name="changePasswordRequest">Current and new passwords</param>
        /// <returns>Confirmation message</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid current password or weak new password</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequest)
        {
            try
            {
                _logger.LogInformation("Change password attempt");

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Validate password match
                if (changePasswordRequest.NewPassword != changePasswordRequest.ConfirmPassword)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "New password and confirm password do not match"
                    });
                }

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in JWT token");
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                // Call service
                var result = await _authService.ChangePasswordAsync(userId, changePasswordRequest);

                if (!result)
                {
                    _logger.LogWarning($"Change password failed for user: {userId}");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to change password"
                    });
                }

                _logger.LogInformation($"Password changed successfully for user: {userId}");

                return Ok(new ApiResponse<object?>
                {
                    Success = true,
                    Message = "Password changed successfully",
                    Data = null
                });
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning($"Change password validation error: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Change password error: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
