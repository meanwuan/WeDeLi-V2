using FluentValidation;
using wedeli.Models.DTO.Order;

namespace wedeli.Validators.Order
{
    // ============================================
    // CREATE ORDER VALIDATOR
    // ============================================

    public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderValidator()
        {
            // Customer - 0 means walk-in (auto-create), positive means existing customer
            RuleFor(x => x.CustomerId)
                .GreaterThanOrEqualTo(0).WithMessage("Customer ID must be 0 (walk-in) or a valid customer ID");

            // Sender Information
            RuleFor(x => x.SenderName)
                .NotEmpty().WithMessage("Sender Name is required")
                .MaximumLength(200).WithMessage("Sender Name cannot exceed 200 characters");

            RuleFor(x => x.SenderPhone)
                .NotEmpty().WithMessage("Sender Phone is required")
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Sender Phone must be a valid Vietnamese number");

            RuleFor(x => x.SenderAddress)
                .NotEmpty().WithMessage("Sender Address is required")
                .MaximumLength(500).WithMessage("Sender Address cannot exceed 500 characters");

            // Receiver Information
            RuleFor(x => x.ReceiverName)
                .NotEmpty().WithMessage("Receiver Name is required")
                .MaximumLength(200).WithMessage("Receiver Name cannot exceed 200 characters");

            RuleFor(x => x.ReceiverPhone)
                .NotEmpty().WithMessage("Receiver Phone is required")
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Receiver Phone must be a valid Vietnamese number");

            RuleFor(x => x.ReceiverAddress)
                .NotEmpty().WithMessage("Receiver Address is required")
                .MaximumLength(500).WithMessage("Receiver Address cannot exceed 500 characters");

            RuleFor(x => x.ReceiverProvince)
                .NotEmpty().WithMessage("Receiver Province is required")
                .MaximumLength(100).WithMessage("Receiver Province cannot exceed 100 characters");

            RuleFor(x => x.ReceiverDistrict)
                .NotEmpty().WithMessage("Receiver District is required")
                .MaximumLength(100).WithMessage("Receiver District cannot exceed 100 characters");

            // Parcel Information
            RuleFor(x => x.ParcelType)
                .NotEmpty().WithMessage("Parcel Type is required")
                .Must(BeValidParcelType).WithMessage("Invalid Parcel Type. Valid types: fragile, electronics, food, cold, document, other");

            RuleFor(x => x.WeightKg)
                .GreaterThan(0).WithMessage("Weight must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Weight cannot exceed 10,000 kg");

            RuleFor(x => x.DeclaredValue)
                .GreaterThanOrEqualTo(0).WithMessage("Declared Value must be positive")
                .When(x => x.DeclaredValue.HasValue);

            RuleFor(x => x.SpecialInstructions)
                .MaximumLength(1000).WithMessage("Special Instructions cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.SpecialInstructions));

            // Payment
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment Method is required")
                .Must(BeValidPaymentMethod).WithMessage("Invalid Payment Method. Valid methods: cash, bank_transfer, e_wallet, periodic");

            RuleFor(x => x.CodAmount)
                .GreaterThanOrEqualTo(0).WithMessage("COD Amount must be positive")
                .LessThanOrEqualTo(999999999).WithMessage("COD Amount is too large");

            // Schedule
            RuleFor(x => x.PickupScheduledAt)
                .GreaterThan(DateTime.Now).WithMessage("Pickup schedule must be in the future")
                .When(x => x.PickupScheduledAt.HasValue);
        }

        private bool BeValidParcelType(string parcelType)
        {
            var validTypes = new[] { "fragile", "electronics", "food", "cold", "document", "other" };
            return validTypes.Contains(parcelType?.ToLower());
        }

        private bool BeValidPaymentMethod(string paymentMethod)
        {
            var validMethods = new[] { "cash", "bank_transfer", "e_wallet", "periodic" };
            return validMethods.Contains(paymentMethod?.ToLower());
        }
    }

    // ============================================
    // UPDATE ORDER VALIDATOR
    // ============================================

    public class UpdateOrderValidator : AbstractValidator<UpdateOrderDto>
    {
        public UpdateOrderValidator()
        {
            RuleFor(x => x.SenderName)
                .MaximumLength(200).WithMessage("Sender Name cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.SenderName));

            RuleFor(x => x.SenderPhone)
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Sender Phone must be a valid Vietnamese number")
                .When(x => !string.IsNullOrEmpty(x.SenderPhone));

            RuleFor(x => x.ReceiverName)
                .MaximumLength(200).WithMessage("Receiver Name cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.ReceiverName));

            RuleFor(x => x.ReceiverPhone)
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Receiver Phone must be a valid Vietnamese number")
                .When(x => !string.IsNullOrEmpty(x.ReceiverPhone));

            RuleFor(x => x.WeightKg)
                .GreaterThan(0).WithMessage("Weight must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Weight cannot exceed 10,000 kg")
                .When(x => x.WeightKg.HasValue);

            RuleFor(x => x.DeclaredValue)
                .GreaterThanOrEqualTo(0).WithMessage("Declared Value must be positive")
                .When(x => x.DeclaredValue.HasValue);

            // Order Status validation
            RuleFor(x => x.OrderStatus)
                .Must(BeValidOrderStatus).WithMessage("Invalid Order Status")
                .When(x => !string.IsNullOrEmpty(x.OrderStatus));

            // Payment Status validation
            RuleFor(x => x.PaymentStatus)
                .Must(BeValidPaymentStatus).WithMessage("Invalid Payment Status")
                .When(x => !string.IsNullOrEmpty(x.PaymentStatus));
        }

        private bool BeValidOrderStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return true;
            var validStatuses = new[] { "pending_pickup", "picked_up", "in_transit", "out_for_delivery", "delivered", "returned", "cancelled" };
            return validStatuses.Contains(status.ToLower());
        }

        private bool BeValidPaymentStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return true;
            var validStatuses = new[] { "unpaid", "paid", "pending" };
            return validStatuses.Contains(status.ToLower());
        }
    }

    // ============================================
    // UPDATE ORDER STATUS VALIDATOR
    // ============================================

    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.NewStatus)
                .NotEmpty().WithMessage("New Status is required")
                .Must(BeValidOrderStatus).WithMessage("Invalid Order Status");

            RuleFor(x => x.Location)
                .MaximumLength(200).WithMessage("Location cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }

        private bool BeValidOrderStatus(string status)
        {
            var validStatuses = new[] { "pending_pickup", "picked_up", "in_transit", "out_for_delivery", "delivered", "returned", "cancelled" };
            return validStatuses.Contains(status?.ToLower());
        }
    }

    // ============================================
    // UPLOAD ORDER PHOTO VALIDATOR
    // ============================================

    public class UploadOrderPhotoValidator : AbstractValidator<UploadOrderPhotoDto>
    {
        private const int MaxFileSizeMB = 10;
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public UploadOrderPhotoValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("Order ID is required");

            RuleFor(x => x.PhotoType)
                .NotEmpty().WithMessage("Photo Type is required")
                .Must(BeValidPhotoType).WithMessage("Invalid Photo Type");

            RuleFor(x => x.PhotoFile)
                .NotNull().WithMessage("Photo file is required")
                .Must(BeValidFileSize).WithMessage($"File size cannot exceed {MaxFileSizeMB} MB");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required")
                .Must(HaveValidExtension).WithMessage($"Invalid file extension. Allowed: {string.Join(", ", AllowedExtensions)}");
        }

        private bool BeValidPhotoType(string photoType)
        {
            var validTypes = new[] { "before_delivery", "after_delivery", "parcel_condition", "signature", "damage_proof" };
            return validTypes.Contains(photoType?.ToLower());
        }

        private bool BeValidFileSize(byte[] file)
        {
            if (file == null) return false;
            return file.Length <= MaxFileSizeMB * 1024 * 1024;
        }

        private bool HaveValidExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            var extension = System.IO.Path.GetExtension(fileName).ToLower();
            return AllowedExtensions.Contains(extension);
        }
    }

    // ============================================
    // ASSIGN ORDER VALIDATOR
    // ============================================

    public class AssignOrderValidator : AbstractValidator<AssignOrderDto>
    {
        public AssignOrderValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("Order ID is required");

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage("Driver ID is required");

            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("Vehicle ID is required");

            RuleFor(x => x.RouteId)
                .GreaterThan(0).WithMessage("Route ID is required")
                .When(x => x.RouteId.HasValue);

            RuleFor(x => x.ScheduledPickupTime)
                .GreaterThan(DateTime.Now).WithMessage("Scheduled Pickup Time must be in the future")
                .When(x => x.ScheduledPickupTime.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }

    // ============================================
    // CANCEL ORDER VALIDATOR
    // ============================================

    public class CancelOrderValidator : AbstractValidator<CancelOrderDto>
    {
        public CancelOrderValidator()
        {
            RuleFor(x => x.CancellationReason)
                .NotEmpty().WithMessage("Cancellation Reason is required")
                .MinimumLength(10).WithMessage("Cancellation Reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Cancellation Reason cannot exceed 500 characters");
        }
    }
}