using FluentValidation;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Vehicle;

namespace wedeli.Validators.Driver
{
    // ============================================
    // CREATE DRIVER VALIDATOR
    // ============================================

    public class CreateDriverValidator : AbstractValidator<CreateDriverDto>
    {
        public CreateDriverValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID is required");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID is required");

            RuleFor(x => x.DriverLicense)
                .NotEmpty().WithMessage("Driver License is required")
                .MaximumLength(50).WithMessage("Driver License cannot exceed 50 characters")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Driver License must contain only uppercase letters and numbers");

            RuleFor(x => x.LicenseExpiry)
                .NotEmpty().WithMessage("License Expiry Date is required")
                .GreaterThan(DateTime.Now).WithMessage("License must not be expired");
        }
    }

    // ============================================
    // UPDATE DRIVER VALIDATOR
    // ============================================

    public class UpdateDriverValidator : AbstractValidator<UpdateDriverDto>
    {
        public UpdateDriverValidator()
        {
            RuleFor(x => x.DriverLicense)
                .MaximumLength(50).WithMessage("Driver License cannot exceed 50 characters")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Driver License must contain only uppercase letters and numbers")
                .When(x => !string.IsNullOrEmpty(x.DriverLicense));

            RuleFor(x => x.LicenseExpiry)
                .GreaterThan(DateTime.Now).WithMessage("License must not be expired")
                .When(x => x.LicenseExpiry.HasValue);
        }
    }
}

namespace wedeli.Validators.Vehicle
{
    // ============================================
    // CREATE VEHICLE VALIDATOR
    // ============================================

    public class CreateVehicleValidator : AbstractValidator<CreateVehicleDto>
    {
        public CreateVehicleValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID is required");

            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage("License Plate is required")
                .MaximumLength(20).WithMessage("License Plate cannot exceed 20 characters")
                .Matches(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{4,5}$").WithMessage("Invalid license plate format (e.g., 29A-12345)");

            RuleFor(x => x.VehicleType)
                .NotEmpty().WithMessage("Vehicle Type is required")
                .Must(BeValidVehicleType).WithMessage("Invalid Vehicle Type. Valid types: truck, van, motorbike");

            RuleFor(x => x.MaxWeightKg)
                .GreaterThan(0).WithMessage("Max Weight must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Max Weight cannot exceed 100,000 kg");

            RuleFor(x => x.MaxVolumeM3)
                .GreaterThan(0).WithMessage("Max Volume must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Max Volume cannot exceed 1,000 m³")
                .When(x => x.MaxVolumeM3.HasValue);

            RuleFor(x => x.OverloadThreshold)
                .InclusiveBetween(50, 100).WithMessage("Overload Threshold must be between 50% and 100%");
        }

        private bool BeValidVehicleType(string vehicleType)
        {
            var validTypes = new[] { "truck", "van", "motorbike" };
            return validTypes.Contains(vehicleType?.ToLower());
        }
    }

    // ============================================
    // UPDATE VEHICLE VALIDATOR
    // ============================================

    public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleDto>
    {
        public UpdateVehicleValidator()
        {
            RuleFor(x => x.LicensePlate)
                .MaximumLength(20).WithMessage("License Plate cannot exceed 20 characters")
                .Matches(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{4,5}$").WithMessage("Invalid license plate format")
                .When(x => !string.IsNullOrEmpty(x.LicensePlate));

            RuleFor(x => x.VehicleType)
                .Must(BeValidVehicleType).WithMessage("Invalid Vehicle Type")
                .When(x => !string.IsNullOrEmpty(x.VehicleType));

            RuleFor(x => x.MaxWeightKg)
                .GreaterThan(0).WithMessage("Max Weight must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Max Weight cannot exceed 100,000 kg")
                .When(x => x.MaxWeightKg.HasValue);

            RuleFor(x => x.OverloadThreshold)
                .InclusiveBetween(50, 100).WithMessage("Overload Threshold must be between 50% and 100%")
                .When(x => x.OverloadThreshold.HasValue);

            RuleFor(x => x.CurrentStatus)
                .Must(BeValidVehicleStatus).WithMessage("Invalid Vehicle Status")
                .When(x => !string.IsNullOrEmpty(x.CurrentStatus));
        }

        private bool BeValidVehicleType(string vehicleType)
        {
            var validTypes = new[] { "truck", "van", "motorbike" };
            return validTypes.Contains(vehicleType?.ToLower());
        }

        private bool BeValidVehicleStatus(string status)
        {
            var validStatuses = new[] { "available", "in_transit", "maintenance", "inactive", "overloaded" };
            return validStatuses.Contains(status?.ToLower());
        }
    }

    // ============================================
    // UPDATE VEHICLE STATUS VALIDATOR
    // ============================================

    public class UpdateVehicleStatusValidator : AbstractValidator<UpdateVehicleStatusDto>
    {
        public UpdateVehicleStatusValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(BeValidVehicleStatus).WithMessage("Invalid Vehicle Status");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }

        private bool BeValidVehicleStatus(string status)
        {
            var validStatuses = new[] { "available", "in_transit", "maintenance", "inactive", "overloaded" };
            return validStatuses.Contains(status?.ToLower());
        }
    }

    // ============================================
    // APPROVE OVERLOAD VALIDATOR
    // ============================================

    public class ApproveOverloadValidator : AbstractValidator<ApproveOverloadDto>
    {
        public ApproveOverloadValidator()
        {
            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("Vehicle ID is required");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
                .When(x => x.AllowOverload);

            RuleFor(x => x.TemporaryMaxWeight)
                .GreaterThan(0).WithMessage("Temporary Max Weight must be greater than 0")
                .When(x => x.TemporaryMaxWeight.HasValue);
        }
    }

    // ============================================
    // VEHICLE SEARCH VALIDATOR
    // ============================================

    public class VehicleSearchValidator : AbstractValidator<VehicleSearchDto>
    {
        public VehicleSearchValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page Number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page Size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page Size cannot exceed 100");

            RuleFor(x => x.MinCapacity)
                .GreaterThanOrEqualTo(0).WithMessage("Min Capacity must be positive")
                .When(x => x.MinCapacity.HasValue);

            RuleFor(x => x.MaxCapacity)
                .GreaterThanOrEqualTo(x => x.MinCapacity ?? 0).WithMessage("Max Capacity must be greater than Min Capacity")
                .When(x => x.MaxCapacity.HasValue && x.MinCapacity.HasValue);
        }
    }
}