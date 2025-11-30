using AutoMapper;
using Microsoft.Extensions.Configuration;
using wedeli.Exceptions;
using wedeli.Infrastructure;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Auth;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Authentication service implementation
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IMapper mapper,
            IJwtService jwtService,
            IPasswordService passwordService,
            IEmailService emailService,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ICustomerRepository customerRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _mapper = mapper;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _emailService = emailService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _customerRepository = customerRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            // Check if username exists
            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                throw new ConflictException("User", "username", dto.Username);
            }

            // Check if email exists
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    throw new ConflictException("User", "email", dto.Email);
                }
            }

            // Check if phone exists
            var existingPhone = await _userRepository.GetByPhoneAsync(dto.Phone);
            if (existingPhone != null)
            {
                throw new ConflictException("User", "phone", dto.Phone);
            }

            // Check password strength
            if (!_passwordService.IsPasswordStrong(dto.Password))
            {
                throw new BadRequestException(
                    "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.");
            }

            // Map to entity
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = _passwordService.HashPassword(dto.Password);
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            // Add user
            var createdUser = await _userRepository.CreateAsync(user);

            // If role is Customer, create Customer record
            if (dto.RoleId == 5) // Assuming role_id 5 is Customer
            {
                var customer = new Customer
                {
                    UserId = createdUser.UserId,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    IsRegular = false,
                    TotalOrders = 0,
                    TotalRevenue = 0,
                    PaymentPrivilege = "prepay",
                    CreditLimit = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _customerRepository.AddAsync(customer);
            }

            // Map to response
            var response = _mapper.Map<RegisterResponseDto>(createdUser);
            response.Message = "Registration successful. Please login to continue.";
            return response;
        }

        /// <summary>
        /// Login user
        /// </summary>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            // Find user by email or username
            var user = await _userRepository.GetByEmailAsync(dto.EmailOrUsername);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email/username or password.");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email/username or password.");
            }

            // Check if user is active
            if (user.IsActive == false)
            {
                throw new ForbiddenException("Your account has been deactivated. Please contact support.");
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _jwtService.SaveRefreshTokenAsync(user.UserId, refreshToken);

            // Get role name
            var role = await _roleRepository.GetByIdAsync(user.RoleId);

            // Build response
            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = role?.RoleName ?? "Unknown",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "10080")),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(
                    int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "30"))
            };

            return response;
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            // Validate refresh token
            var isValid = await _jwtService.IsRefreshTokenValidAsync(dto.RefreshToken);
            if (!isValid)
            {
                throw new UnauthorizedException("Invalid or expired refresh token.");
            }

            // Get user from access token (even if expired)
            var userId = _jwtService.GetUserIdFromToken(dto.AccessToken);
            if (userId == null)
            {
                throw new UnauthorizedException("Invalid access token.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null || user.IsActive == false)
            {
                throw new UnauthorizedException("User not found or inactive.");
            }

            // Revoke old refresh token
            await _jwtService.RevokeRefreshTokenAsync(dto.RefreshToken);

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Save new refresh token
            await _jwtService.SaveRefreshTokenAsync(user.UserId, newRefreshToken);

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "10080")),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(
                    int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "30"))
            };
        }

        /// <summary>
        /// Logout user - revoke refresh token
        /// </summary>
        public async Task<bool> LogoutAsync(LogoutRequestDto dto)
        {
            try
            {
                // Revoke refresh token
                await _jwtService.RevokeRefreshTokenAsync(dto.RefreshToken);
                return true;
            }
            catch (Exception)
            {
                throw new BadRequestException("Failed to logout. Invalid refresh token.");
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            // Don't reveal that user doesn't exist (security best practice)
            if (user == null)
            {
                return true;
            }

            // Generate reset token (valid for 1 hour)
            var resetToken = _jwtService.GenerateRefreshToken();

            // Save reset token
            await _jwtService.SaveRefreshTokenAsync(user.UserId, resetToken);

            // Send email with reset link
            var resetLink = $"{_configuration["AppSettings:FrontendUrl"]}/reset-password?token={resetToken}";
            await _emailService.SendEmailAsync(user.Email, user.FullName, resetLink);

            return true;
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            // Verify reset token
            var isValid = await _jwtService.IsRefreshTokenValidAsync(dto.ResetToken);
            if (!isValid)
            {
                throw new UnauthorizedException("Invalid or expired reset token.");
            }

            // Get user ID from token
            var userId = _jwtService.GetUserIdFromToken(dto.ResetToken);
            if (userId == null)
            {
                throw new UnauthorizedException("Invalid reset token.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                throw new NotFoundException("User", userId.Value);
            }

            // Check password strength
            if (!_passwordService.IsPasswordStrong(dto.NewPassword))
            {
                throw new BadRequestException(
                    "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.");
            }

            // Update password
            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Revoke the reset token
            await _jwtService.RevokeRefreshTokenAsync(dto.ResetToken);

            // Revoke all existing refresh tokens for security
            await _jwtService.RevokeRefreshTokenAsync(user.UserId.ToString());

            return true;
        }

        /// <summary>
        /// Change password (for logged-in users)
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User", userId);
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            {
                throw new BadRequestException("Current password is incorrect.");
            }

            // Check if new password is same as current
            if (_passwordService.VerifyPassword(dto.NewPassword, user.PasswordHash))
            {
                throw new BadRequestException("New password must be different from current password.");
            }

            // Check password strength
            if (!_passwordService.IsPasswordStrong(dto.NewPassword))
            {
                throw new BadRequestException(
                    "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.");
            }

            // Update password
            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Optional: Revoke all refresh tokens to force re-login on all devices
            // await _jwtService.RevokeAllUserTokensAsync(userId);

            return true;
        }

        /// <summary>
        /// Validate JWT access token
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // Validate token format and signature
                var isValid = _jwtService.ValidateToken(token);
                if (isValid == null)
                {
                    return false;
                }

                // Get user ID from token
                var userId = _jwtService.GetUserIdFromToken(token);
                if (userId == null)
                {
                    return false;
                }

                // Check if user exists and is active
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null || user.IsActive == false)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}