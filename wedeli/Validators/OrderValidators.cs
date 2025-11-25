using FluentValidation;
using wedeli.Models.DTO;
using wedeli.Enums;

// File: Validators/OrderValidators.cs

namespace wedeli.Validators;

/// <summary>
/// Validator cho CreateOrderRequest
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        // === SENDER VALIDATION ===
        RuleFor(x => x.SenderName)
            .NotEmpty().WithMessage("Tên người gửi không được để trống")
            .MaximumLength(200).WithMessage("Tên người gửi tối đa 200 ký tự");

        RuleFor(x => x.SenderPhone)
            .NotEmpty().WithMessage("SĐT người gửi không được để trống")
            .MaximumLength(20).WithMessage("SĐT tối đa 20 ký tự")
            .Matches(@"^(0|\+84)[0-9]{9,10}$")
            .WithMessage("SĐT người gửi không hợp lệ (VD: 0901234567)");

        RuleFor(x => x.SenderAddress)
            .NotEmpty().WithMessage("Địa chỉ người gửi không được để trống");

        // === RECEIVER VALIDATION ===
        RuleFor(x => x.ReceiverName)
            .NotEmpty().WithMessage("Tên người nhận không được để trống")
            .MaximumLength(200).WithMessage("Tên người nhận tối đa 200 ký tự");

        RuleFor(x => x.ReceiverPhone)
            .NotEmpty().WithMessage("SĐT người nhận không được để trống")
            .MaximumLength(20).WithMessage("SĐT tối đa 20 ký tự")
            .Matches(@"^(0|\+84)[0-9]{9,10}$")
            .WithMessage("SĐT người nhận không hợp lệ");

        RuleFor(x => x.ReceiverAddress)
            .NotEmpty().WithMessage("Địa chỉ người nhận không được để trống");

        RuleFor(x => x.ReceiverProvince)
            .MaximumLength(100).WithMessage("Tỉnh/TP tối đa 100 ký tự");

        RuleFor(x => x.ReceiverDistrict)
            .MaximumLength(100).WithMessage("Quận/Huyện tối đa 100 ký tự");

        // === PACKAGE VALIDATION ===
        RuleFor(x => x.ParcelType)
            .IsInEnum().WithMessage("Loại hàng hóa không hợp lệ");

        RuleFor(x => x.WeightKg)
            .GreaterThan(0).When(x => x.WeightKg.HasValue)
            .WithMessage("Trọng lượng phải lớn hơn 0")
            .LessThanOrEqualTo(5000).When(x => x.WeightKg.HasValue)
            .WithMessage("Trọng lượng tối đa 5000 kg");

        RuleFor(x => x.DeclaredValue)
            .GreaterThanOrEqualTo(0).When(x => x.DeclaredValue.HasValue)
            .WithMessage("Giá trị khai báo không được âm");

        // === PRICING VALIDATION ===
        RuleFor(x => x.CodAmount)
            .GreaterThanOrEqualTo(0).When(x => x.CodAmount.HasValue)
            .WithMessage("Số tiền COD không được âm")
            .LessThanOrEqualTo(500_000_000).When(x => x.CodAmount.HasValue)
            .WithMessage("Số tiền COD tối đa 500 triệu");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Phương thức thanh toán không hợp lệ");

        // === OTHER ===
        RuleFor(x => x.PickupScheduledAt)
            .GreaterThan(DateTime.UtcNow.AddHours(-1))
            .When(x => x.PickupScheduledAt.HasValue)
            .WithMessage("Ngày lấy hàng phải là thời điểm trong tương lai");
    }
}

/// <summary>
/// Validator cho UpdateOrderRequest
/// </summary>
public class UpdateOrderValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.ReceiverName)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.ReceiverName))
            .WithMessage("Tên người nhận tối đa 200 ký tự");

        RuleFor(x => x.ReceiverPhone)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.ReceiverPhone))
            .Matches(@"^(0|\+84)[0-9]{9,10}$")
            .When(x => !string.IsNullOrEmpty(x.ReceiverPhone))
            .WithMessage("SĐT người nhận không hợp lệ");

        RuleFor(x => x.ParcelType)
            .IsInEnum().When(x => x.ParcelType.HasValue)
            .WithMessage("Loại hàng hóa không hợp lệ");

        RuleFor(x => x.WeightKg)
            .GreaterThan(0).When(x => x.WeightKg.HasValue)
            .WithMessage("Trọng lượng phải lớn hơn 0");

        RuleFor(x => x.CodAmount)
            .GreaterThanOrEqualTo(0).When(x => x.CodAmount.HasValue)
            .WithMessage("Số tiền COD không được âm");
    }
}

/// <summary>
/// Validator cho UpdateOrderStatusRequest
/// </summary>
public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Trạng thái không hợp lệ");

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => x.Notes != null)
            .WithMessage("Ghi chú tối đa 500 ký tự");

        RuleFor(x => x.Location)
            .MaximumLength(200).When(x => x.Location != null)
            .WithMessage("Vị trí tối đa 200 ký tự");

        RuleFor(x => x.PhotoUrl)
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.PhotoUrl))
            .WithMessage("URL ảnh không hợp lệ");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

/// <summary>
/// Validator cho AssignOrderRequest
/// </summary>
public class AssignOrderValidator : AbstractValidator<AssignOrderRequest>
{
    public AssignOrderValidator()
    {
        RuleFor(x => x)
            .Must(x => x.DriverId.HasValue || x.VehicleId.HasValue)
            .WithMessage("Phải chọn ít nhất tài xế hoặc xe");

        RuleFor(x => x.DriverId)
            .GreaterThan(0).When(x => x.DriverId.HasValue)
            .WithMessage("DriverId không hợp lệ");

        RuleFor(x => x.VehicleId)
            .GreaterThan(0).When(x => x.VehicleId.HasValue)
            .WithMessage("VehicleId không hợp lệ");

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => x.Notes != null)
            .WithMessage("Ghi chú tối đa 500 ký tự");
    }
}

/// <summary>
/// Validator cho BulkAssignRequest
/// </summary>
public class BulkAssignValidator : AbstractValidator<BulkAssignRequest>
{
    public BulkAssignValidator()
    {
        RuleFor(x => x.OrderIds)
            .NotEmpty().WithMessage("Danh sách đơn hàng không được trống")
            .Must(x => x.Count <= 100).WithMessage("Tối đa 100 đơn hàng mỗi lần");

        RuleFor(x => x.OrderIds)
            .Must(x => x.All(id => id > 0))
            .WithMessage("OrderId phải lớn hơn 0");

        RuleFor(x => x.DriverId)
            .GreaterThan(0).WithMessage("DriverId không hợp lệ");

        RuleFor(x => x.VehicleId)
            .GreaterThan(0).When(x => x.VehicleId.HasValue)
            .WithMessage("VehicleId không hợp lệ");
    }
}

/// <summary>
/// Validator cho UploadOrderPhotoRequest
/// </summary>
public class UploadOrderPhotoValidator : AbstractValidator<UploadOrderPhotoRequest>
{
    public UploadOrderPhotoValidator()
    {
        RuleFor(x => x.PhotoUrl)
            .NotEmpty().WithMessage("URL ảnh không được để trống")
            .MaximumLength(500).WithMessage("URL tối đa 500 ký tự")
            .Must(BeValidUrl).WithMessage("URL ảnh không hợp lệ");

        RuleFor(x => x.PhotoType)
            .IsInEnum().WithMessage("Loại ảnh không hợp lệ (before_delivery, after_delivery, parcel_condition, signature, damage_proof)");

        RuleFor(x => x.FileName)
            .MaximumLength(255).When(x => x.FileName != null)
            .WithMessage("Tên file tối đa 255 ký tự");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

/// <summary>
/// Validator cho CalculateShippingFeeRequest
/// </summary>
public class CalculateShippingFeeValidator : AbstractValidator<CalculateShippingFeeRequest>
{
    public CalculateShippingFeeValidator()
    {
        RuleFor(x => x.RouteId)
            .GreaterThan(0).WithMessage("RouteId không hợp lệ");

        RuleFor(x => x.WeightKg)
            .GreaterThan(0).When(x => x.WeightKg.HasValue)
            .WithMessage("Trọng lượng phải lớn hơn 0");

        RuleFor(x => x.CodAmount)
            .GreaterThanOrEqualTo(0).When(x => x.CodAmount.HasValue)
            .WithMessage("Số tiền COD không được âm");
    }
}