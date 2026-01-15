using FluentValidation;
using wedeli.Models.DTO.Customer;

namespace wedeli.Validators.Customer
{
    // ============================================
    // CREATE CUSTOMER VALIDATOR
    // ============================================

    public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerValidator()
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
    // UPDATE CUSTOMER VALIDATOR
    // ============================================

    public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(200).WithMessage("Full Name cannot exceed 200 characters")
                .Matches(@"^[\p{L}\s]+$").WithMessage("Full Name can only contain letters and spaces")
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.Phone)
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Phone must be a valid Vietnamese number")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }

    // ============================================
    // CREATE CUSTOMER ADDRESS VALIDATOR
    // ============================================

    public class CreateCustomerAddressValidator : AbstractValidator<CreateCustomerAddressDto>
    {
        public CreateCustomerAddressValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Customer ID is required");

            RuleFor(x => x.AddressLabel)
                .MaximumLength(100).WithMessage("Address Label cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLabel));

            RuleFor(x => x.FullAddress)
                .NotEmpty().WithMessage("Full Address is required")
                .MinimumLength(10).WithMessage("Full Address must be at least 10 characters")
                .MaximumLength(500).WithMessage("Full Address cannot exceed 500 characters");

            RuleFor(x => x.Province)
                .NotEmpty().WithMessage("Province is required")
                .MaximumLength(100).WithMessage("Province cannot exceed 100 characters");

            RuleFor(x => x.District)
                .NotEmpty().WithMessage("District is required")
                .MaximumLength(100).WithMessage("District cannot exceed 100 characters");

            RuleFor(x => x.Ward)
                .MaximumLength(100).WithMessage("Ward cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Ward));

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
                .When(x => x.Longitude.HasValue);
        }
    }

    // ============================================
    // UPDATE CUSTOMER ADDRESS VALIDATOR
    // ============================================

    public class UpdateCustomerAddressValidator : AbstractValidator<UpdateCustomerAddressDto>
    {
        public UpdateCustomerAddressValidator()
        {
            RuleFor(x => x.AddressLabel)
                .MaximumLength(100).WithMessage("Address Label cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.AddressLabel));

            RuleFor(x => x.FullAddress)
                .MinimumLength(10).WithMessage("Full Address must be at least 10 characters")
                .MaximumLength(500).WithMessage("Full Address cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.FullAddress));

            RuleFor(x => x.Province)
                .MaximumLength(100).WithMessage("Province cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Province));

            RuleFor(x => x.District)
                .MaximumLength(100).WithMessage("District cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.District));

            RuleFor(x => x.Ward)
                .MaximumLength(100).WithMessage("Ward cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Ward));

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
                .When(x => x.Longitude.HasValue);
        }
    }

    // ============================================
    // UPDATE CUSTOMER CLASSIFICATION VALIDATOR
    // ============================================

    public class UpdateCustomerClassificationValidator : AbstractValidator<UpdateCustomerClassificationDto>
    {
        public UpdateCustomerClassificationValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Customer ID is required");

            RuleFor(x => x.PaymentPrivilege)
                .NotEmpty().WithMessage("Payment Privilege is required")
                .Must(BeValidPaymentPrivilege).WithMessage("Invalid Payment Privilege. Valid values: prepay, postpay, periodic");

            RuleFor(x => x.CreditLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Credit Limit must be positive")
                .LessThanOrEqualTo(999999999).WithMessage("Credit Limit is too large");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }

        private bool BeValidPaymentPrivilege(string privilege)
        {
            var validPrivileges = new[] { "prepay", "postpay", "periodic" };
            return validPrivileges.Contains(privilege?.ToLower());
        }
    }

    // ============================================
    // CUSTOMER SEARCH VALIDATOR
    // ============================================

    public class CustomerSearchValidator : AbstractValidator<CustomerSearchDto>
    {
        public CustomerSearchValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page Number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page Size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page Size cannot exceed 100");

            RuleFor(x => x.MinRevenue)
                .GreaterThanOrEqualTo(0).WithMessage("Min Revenue must be positive")
                .When(x => x.MinRevenue.HasValue);

            RuleFor(x => x.MaxRevenue)
                .GreaterThanOrEqualTo(x => x.MinRevenue ?? 0).WithMessage("Max Revenue must be greater than Min Revenue")
                .When(x => x.MaxRevenue.HasValue && x.MinRevenue.HasValue);

            RuleFor(x => x.MinOrders)
                .GreaterThanOrEqualTo(0).WithMessage("Min Orders must be positive")
                .When(x => x.MinOrders.HasValue);

            RuleFor(x => x.MaxOrders)
                .GreaterThanOrEqualTo(x => x.MinOrders ?? 0).WithMessage("Max Orders must be greater than Min Orders")
                .When(x => x.MaxOrders.HasValue && x.MinOrders.HasValue);

            RuleFor(x => x.CreatedTo)
                .GreaterThanOrEqualTo(x => x.CreatedFrom ?? DateTime.MinValue).WithMessage("Created To must be after Created From")
                .When(x => x.CreatedTo.HasValue && x.CreatedFrom.HasValue);

            RuleFor(x => x.SortOrder)
                .Must(BeValidSortOrder).WithMessage("Sort Order must be 'asc' or 'desc'")
                .When(x => !string.IsNullOrEmpty(x.SortOrder));
        }

        private bool BeValidSortOrder(string sortOrder)
        {
            return new[] { "asc", "desc" }.Contains(sortOrder?.ToLower());
        }
    }
}