using FluentValidation;
using wedeli.Models.DTO.Auth;

namespace wedeli.Validators.Auth
{
    // ============================================
    // LOGIN VALIDATOR
    // ============================================

    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.EmailOrUsername)
                .NotEmpty().WithMessage("Email or Username is required")
                .MaximumLength(100).WithMessage("Email or Username cannot exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters");
        }
    }

    // ============================================
    // REGISTER VALIDATOR
    // ============================================

    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required")
                .MaximumLength(200).WithMessage("Full Name cannot exceed 200 characters")
                .Matches(@"^[\p{L}\s]+$").WithMessage("Full Name can only contain letters and spaces");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Phone must be a valid Vietnamese number (e.g., 0901234567 or +84901234567)");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("Role is required");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID is required")
                .When(x => x.RoleId != 1); // Not required for SuperAdmin (assuming RoleId 1 is SuperAdmin)
        }
    }

    // ============================================
    // REFRESH TOKEN VALIDATOR
    // ============================================

    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access Token is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh Token is required");
        }
    }

    // ============================================
    // FORGOT PASSWORD VALIDATOR
    // ============================================

    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        }
    }

    // ============================================
    // RESET PASSWORD VALIDATOR
    // ============================================

    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.ResetToken)
                .NotEmpty().WithMessage("Reset token is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required")
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        }
    }

    // ============================================
    // CHANGE PASSWORD VALIDATOR
    // ============================================

    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current Password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number")
                .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required")
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        }
    }

    // ============================================
    // UPDATE PROFILE VALIDATOR
    // ============================================

    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required")
                .MaximumLength(200).WithMessage("Full Name cannot exceed 200 characters")
                .Matches(@"^[\p{L}\s]+$").WithMessage("Full Name can only contain letters and spaces");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Phone must be a valid Vietnamese number");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }

    // ============================================
    // LOGOUT VALIDATOR
    // ============================================

    public class LogoutRequestValidator : AbstractValidator<LogoutRequestDto>
    {
        public LogoutRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh Token is required");
        }
    }
}