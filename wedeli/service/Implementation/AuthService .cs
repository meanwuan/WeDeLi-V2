using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using wedeli.Infrastructure;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            IMapper mapper,
            ILogger<AuthService> logger,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _smsService = smsService;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }
        /// <summary>
        /// LOGIN - Xác thực username/password và tạo tokens
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {Username}", request.Username);

                // 1. Validate input
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Login failed: Empty username or password");
                    throw new ArgumentException("Username and password are required");
                }

                // 2. Get user from database
                var user = await _userRepository.GetByUsernameAsync(request.Username);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
                    throw new UnauthorizedAccessException("Invalid username or password");
                }

                // 3. Check if user is active
                if ((bool)!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User is inactive - {Username}", request.Username);
                    throw new UnauthorizedAccessException("Account is inactive. Please contact administrator.");
                }

                // 4. Verify password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
                    throw new UnauthorizedAccessException("Invalid username or password");
                }

                // 5. Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // 6. Save refresh token to cache (trong production nên lưu vào database)
                var refreshTokenKey = $"refresh_token:{user.UserId}:{refreshToken}";
                var refreshTokenExpiry = TimeSpan.FromDays(
                    int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7")
                );

                _cache.Set(refreshTokenKey, new
                {
                    UserId = user.UserId,
                    Token = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(refreshTokenExpiry)
                }, refreshTokenExpiry);

                // 7. Calculate token expiry
                var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
                var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

                _logger.LogInformation("Login successful for user: {UserId} ({Username})", user.UserId, user.Username);

                // 8. Return response
                return new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = _mapper.Map<UserResponse>(user),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                throw;
            }
        }

        /// <summary>
        /// REGISTER - Tạo user mới
        /// </summary>
        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for username: {Username}", request.Username);

                // 1. Validate input
                ValidateRegisterRequest(request);

                // 2. Check if username exists
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    _logger.LogWarning("Registration failed: Username already exists - {Username}", request.Username);
                    throw new InvalidOperationException($"Username '{request.Username}' is already taken");
                }

                // 3. Check if phone exists
                if (await _userRepository.PhoneExistsAsync(request.Phone))
                {
                    _logger.LogWarning("Registration failed: Phone already exists - {Phone}", request.Phone);
                    throw new InvalidOperationException($"Phone number '{request.Phone}' is already registered");
                }

                // 4. Check if email exists (nếu có)
                if (!string.IsNullOrEmpty(request.Email) && await _userRepository.EmailExistsAsync(request.Email))
                {
                    _logger.LogWarning("Registration failed: Email already exists - {Email}", request.Email);
                    throw new InvalidOperationException($"Email '{request.Email}' is already registered");
                }

                // 5. Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 6. Create user entity
                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = passwordHash,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Email = request.Email,
                    RoleId = request.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 7. Save to database
                var createdUser = await _userRepository.CreateAsync(user);

                _logger.LogInformation("User registered successfully: {UserId} ({Username})",
                    createdUser.UserId, createdUser.Username);

                // 8. Send welcome email (optional)
                if (!string.IsNullOrEmpty(createdUser.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            createdUser.Email,
                            "Welcome to WeDeli",
                            $"Hello {createdUser.FullName}, welcome to WeDeli delivery system!"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send welcome email to {Email}", createdUser.Email);
                        // Không throw exception vì email không quan trọng bằng việc tạo user
                    }
                }

                // 9. Return user info
                return _mapper.Map<UserResponse>(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
                throw;
            }
        }

        /// <summary>
        /// REFRESH TOKEN - Lấy access token mới
        /// </summary>
        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Refresh token attempt");

                // 1. Validate input
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    throw new ArgumentException("Refresh token is required");

                // 2. Find refresh token in cache
                // Trong production, nên dùng database query
                int? userId = null;
                foreach (var key in GetAllCacheKeys().Where(k => k.StartsWith("refresh_token:")))
                {
                    if (_cache.TryGetValue(key, out dynamic tokenData))
                    {
                        if (tokenData.Token == request.RefreshToken)
                        {
                            userId = tokenData.UserId;
                            break;
                        }
                    }
                }

                if (!userId.HasValue)
                {
                    _logger.LogWarning("Refresh token not found or expired");
                    throw new UnauthorizedAccessException("Invalid or expired refresh token");
                }

                // 3. Get user
                var user = await _userRepository.GetByIdAsync(userId.Value);

                if (user == null || (bool)!user.IsActive)
                {
                    _logger.LogWarning("User not found or inactive during refresh: {UserId}", userId);
                    throw new UnauthorizedAccessException("User not found or inactive");
                }

                // 4. Generate new tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // 5. Revoke old refresh token & save new one
                var oldTokenKey = $"refresh_token:{userId}:{request.RefreshToken}";
                _cache.Remove(oldTokenKey);

                var newTokenKey = $"refresh_token:{userId}:{newRefreshToken}";
                var refreshTokenExpiry = TimeSpan.FromDays(
                    int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7")
                );

                _cache.Set(newTokenKey, new
                {
                    UserId = userId.Value,
                    Token = newRefreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(refreshTokenExpiry)
                }, refreshTokenExpiry);

                var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
                var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);

                return new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    User = _mapper.Map<UserResponse>(user),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                throw;
            }
        }

        /// <summary>
        /// CHANGE PASSWORD - Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Change password attempt for user: {UserId}", userId);

                // 1. Validate input
                if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    throw new ArgumentException("All password fields are required");
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    throw new ArgumentException("New password and confirm password do not match");
                }

                if (request.NewPassword.Length < 6)
                {
                    throw new ArgumentException("New password must be at least 6 characters long");
                }

                // 2. Get user
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    throw new KeyNotFoundException($"User {userId} not found");

                // 3. Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Change password failed: Invalid current password for user {UserId}", userId);
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }

                // 4. Hash new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // 5. Update in database
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// FORGOT PASSWORD - Gửi OTP qua SMS
        /// </summary>
        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Forgot password attempt for username: {Username}", request.Username);

                // 1. Get user
                var user = await _userRepository.GetByUsernameAsync(request.Username);

                if (user == null || user.Phone != request.Phone)
                {
                    _logger.LogWarning("Forgot password failed: User not found or phone mismatch");
                    // Không tiết lộ thông tin cụ thể
                    throw new InvalidOperationException("Invalid username or phone number");
                }

                // 2. Generate OTP
                var otp = GenerateOTP();
                var otpKey = $"reset_otp:{user.UserId}";

                // 3. Save OTP to cache (5 phút)
                _cache.Set(otpKey, otp, TimeSpan.FromMinutes(5));

                // 4. Send OTP via SMS
                await _smsService.SendSmsAsync(
                    user.Phone,
                    $"Your WeDeli password reset OTP is: {otp}. Valid for 5 minutes."
                );

                _logger.LogInformation("OTP sent successfully to user: {UserId}", user.UserId);

                return "OTP has been sent to your phone number";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for username: {Username}", request.Username);
                throw;
            }
        }

        /// <summary>
        /// RESET PASSWORD - Reset mật khẩu bằng OTP
        /// </summary>
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Reset password attempt with token");

                // 1. Validate input
                if (string.IsNullOrWhiteSpace(request.Token) ||
                    string.IsNullOrWhiteSpace(request.NewPassword) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    throw new ArgumentException("All fields are required");
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    throw new ArgumentException("Passwords do not match");
                }

                // 2. Find OTP in cache
                // Format: reset_otp:{userId}
                int? userId = null;
                foreach (var key in GetAllCacheKeys().Where(k => k.StartsWith("reset_otp:")))
                {
                    if (_cache.TryGetValue(key, out string otp) && otp == request.Token)
                    {
                        userId = int.Parse(key.Split(':')[1]);
                        _cache.Remove(key); // Remove used OTP
                        break;
                    }
                }

                if (!userId.HasValue)
                {
                    _logger.LogWarning("Reset password failed: Invalid or expired OTP");
                    throw new UnauthorizedAccessException("Invalid or expired OTP");
                }

                // 3. Get user
                var user = await _userRepository.GetByIdAsync(userId.Value);

                if (user == null)
                    throw new KeyNotFoundException("User not found");

                // 4. Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Password reset successfully for user: {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                throw;
            }
        }

        /// <summary>
        /// LOGOUT - Revoke refresh token
        /// </summary>
        public async Task<bool> LogoutAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return false;

                // Remove refresh token from cache
                foreach (var key in GetAllCacheKeys().Where(k => k.StartsWith("refresh_token:")))
                {
                    if (_cache.TryGetValue(key, out dynamic tokenData))
                    {
                        if (tokenData.Token == refreshToken)
                        {
                            _cache.Remove(key);
                            _logger.LogInformation("User logged out, refresh token revoked");
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                throw;
            }
        }

        /// <summary>
        /// Validate user credentials
        /// </summary>
        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);

                if (user == null || (bool)!user.IsActive)
                    return false;

                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials");
                return false;
            }
        }

        // ========== PRIVATE HELPER METHODS ==========

        private void ValidateRegisterRequest(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username is required");

            if (request.Username.Length < 3)
                throw new ArgumentException("Username must be at least 3 characters long");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required");

            if (request.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Full name is required");

            if (string.IsNullOrWhiteSpace(request.Phone))
                throw new ArgumentException("Phone is required");

            if (request.RoleId <= 0)
                throw new ArgumentException("Valid role is required");
        }

        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private IEnumerable<string> GetAllCacheKeys()
        {
            // Note: IMemoryCache không có method lấy all keys
            // Trong production, nên dùng distributed cache (Redis) hoặc database
            // Đây là workaround cho development
            var field = typeof(MemoryCache).GetField("_entries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null && _cache is MemoryCache memoryCache)
            {
                var entries = field.GetValue(memoryCache) as dynamic;
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        yield return entry.Key.ToString();
                    }
                }
            }
        }
    }
}
