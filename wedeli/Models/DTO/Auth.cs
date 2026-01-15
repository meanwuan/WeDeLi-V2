using System;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Auth
{
    // ============================================
    // LOGIN
    // ============================================

    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email or Username is required")]
        public string? EmailOrUsername { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? RoleName { get; set; }
        public int RoleId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }

    // ============================================
    // REGISTER
    // ============================================

    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(200, ErrorMessage = "Full Name cannot exceed 200 characters")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^(\+84|0)[0-9]{9,10}$", ErrorMessage = "Phone must be a valid Vietnamese number")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }

        // Optional for specific roles
        public int? CompanyId { get; set; }
    }

    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? RoleName { get; set; }
        public string? Message { get; set; }
    }

    // ============================================
    // REFRESH TOKEN
    // ============================================

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Access Token is required")]
        public string? AccessToken { get; set; }

        [Required(ErrorMessage = "Refresh Token is required")]
        public string? RefreshToken { get; set; }
    }

    public class RefreshTokenResponseDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }

    // ============================================
    // PASSWORD MANAGEMENT
    // ============================================

    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Reset token is required")]
        public string? ResetToken { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "Current Password is required")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }

    // ============================================
    // USER PROFILE
    // ============================================

    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(200, ErrorMessage = "Full Name cannot exceed 200 characters")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }
    }

    // ============================================
    // LOGOUT
    // ============================================

    public class LogoutRequestDto
    {
        [Required(ErrorMessage = "Refresh Token is required")]
        public string? RefreshToken { get; set; }
    }
}