using FluentValidation;
using wedeli.Models.DTO.Route;
using wedeli.Models.DTO.Trip;
using wedeli.Models.DTO.COD;

namespace wedeli.Validators.Route
{
    public class CreateRouteValidator : AbstractValidator<CreateRouteDto>
    {
        public CreateRouteValidator()
        {
            RuleFor(x => x.CompanyId).GreaterThan(0).WithMessage("Company ID is required");
            RuleFor(x => x.RouteName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.OriginProvince).NotEmpty().MaximumLength(100);
            RuleFor(x => x.OriginDistrict).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DestinationProvince).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DestinationDistrict).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DistanceKm).GreaterThan(0).LessThanOrEqualTo(10000).When(x => x.DistanceKm.HasValue);
            RuleFor(x => x.EstimatedDurationHours).GreaterThan(0).LessThanOrEqualTo(1000).When(x => x.EstimatedDurationHours.HasValue);
            RuleFor(x => x.BasePrice).GreaterThan(0).LessThanOrEqualTo(999999999);
        }
    }

    public class UpdateRouteValidator : AbstractValidator<UpdateRouteDto>
    {
        public UpdateRouteValidator()
        {
            RuleFor(x => x.RouteName).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.RouteName));
            RuleFor(x => x.BasePrice).GreaterThan(0).When(x => x.BasePrice.HasValue);
        }
    }

    public class CalculateShippingFeeValidator : AbstractValidator<CalculateShippingFeeDto>
    {
        public CalculateShippingFeeValidator()
        {
            RuleFor(x => x.RouteId).GreaterThan(0);
            RuleFor(x => x.WeightKg).GreaterThan(0).LessThanOrEqualTo(10000);
        }
    }
}

namespace wedeli.Validators.Trip
{
    public class CreateTripValidator : AbstractValidator<CreateTripDto>
    {
        public CreateTripValidator()
        {
            RuleFor(x => x.RouteId).GreaterThan(0).WithMessage("Route ID is required");
            RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Vehicle ID is required");
            RuleFor(x => x.DriverId).GreaterThan(0).WithMessage("Driver ID is required");
            RuleFor(x => x.TripDate).GreaterThanOrEqualTo(DateTime.Today).WithMessage("Trip Date cannot be in the past");
            RuleFor(x => x.DepartureTime).GreaterThan(DateTime.Now).WithMessage("Departure Time must be in the future").When(x => x.DepartureTime.HasValue);
        }
    }

    public class UpdateTripValidator : AbstractValidator<UpdateTripDto>
    {
        public UpdateTripValidator()
        {
            RuleFor(x => x.VehicleId).GreaterThan(0).When(x => x.VehicleId.HasValue);
            RuleFor(x => x.DriverId).GreaterThan(0).When(x => x.DriverId.HasValue);
            RuleFor(x => x.TripStatus).Must(BeValidTripStatus).When(x => !string.IsNullOrEmpty(x.TripStatus));
        }

        private bool BeValidTripStatus(string status)
        {
            return new[] { "scheduled", "in_progress", "completed", "cancelled" }.Contains(status?.ToLower());
        }
    }

    public class AddOrderToTripValidator : AbstractValidator<AddOrderToTripDto>
    {
        public AddOrderToTripValidator()
        {
            RuleFor(x => x.TripId).GreaterThan(0).WithMessage("Trip ID is required");
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("Order ID is required");
            RuleFor(x => x.SequenceNumber).GreaterThan(0).When(x => x.SequenceNumber.HasValue);
        }
    }
}

namespace wedeli.Validators.COD
{
    public class CollectCodValidator : AbstractValidator<CollectCodDto>
    {
        public CollectCodValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("Order ID is required");
            RuleFor(x => x.CodAmount).GreaterThan(0).LessThanOrEqualTo(999999999).WithMessage("COD Amount must be valid");
            RuleFor(x => x.Notes).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }

    public class SubmitCodValidator : AbstractValidator<SubmitCodDto>
    {
        public SubmitCodValidator()
        {
            RuleFor(x => x.DriverId).GreaterThan(0).WithMessage("Driver ID is required");
            RuleFor(x => x.TransactionIds).NotEmpty().WithMessage("At least one transaction must be submitted");
            RuleFor(x => x.TotalAmount).GreaterThan(0).WithMessage("Total Amount must be greater than 0");
        }
    }

    public class TransferToSenderValidator : AbstractValidator<TransferToSenderDto>
    {
        public TransferToSenderValidator()
        {
            RuleFor(x => x.TransactionId).GreaterThan(0);
            RuleFor(x => x.TransferMethod).NotEmpty().Must(BeValidTransferMethod);
            RuleFor(x => x.CompanyFee).GreaterThanOrEqualTo(0).When(x => x.CompanyFee.HasValue);
        }

        private bool BeValidTransferMethod(string method)
        {
            return new[] { "cash", "bank_transfer", "e_wallet" }.Contains(method?.ToLower());
        }
    }

    public class ReconcileCodValidator : AbstractValidator<ReconcileCodDto>
    {
        public ReconcileCodValidator()
        {
            RuleFor(x => x.DriverId).GreaterThan(0).WithMessage("Driver ID is required");
            RuleFor(x => x.ReconciliationDate).NotEmpty().LessThanOrEqualTo(DateTime.Today).WithMessage("Reconciliation Date cannot be in the future");
        }
    }
}