using FluentValidation;
using wedeli.Models.DTO.Payment;
using wedeli.Models.DTO.Company;
using wedeli.Models.DTO.Partnership;

namespace wedeli.Validators.Payment
{
    public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
    {
        public CreatePaymentValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(999999999);
            RuleFor(x => x.PaymentMethod).NotEmpty().Must(BeValidPaymentMethod);
            RuleFor(x => x.PaymentType).NotEmpty().Must(BeValidPaymentType);
        }

        private bool BeValidPaymentMethod(string method)
        {
            return new[] { "cash", "bank_transfer", "e_wallet" }.Contains(method?.ToLower());
        }

        private bool BeValidPaymentType(string type)
        {
            return new[] { "shipping_fee", "cod_collection", "refund" }.Contains(type?.ToLower());
        }
    }

    public class ConfirmPaymentValidator : AbstractValidator<ConfirmPaymentDto>
    {
        public ConfirmPaymentValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.TransactionRef).NotEmpty().MaximumLength(100);
        }
    }

    public class GenerateInvoiceValidator : AbstractValidator<GenerateInvoiceDto>
    {
        public GenerateInvoiceValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.CompanyId).GreaterThan(0);
            RuleFor(x => x.StartDate).NotEmpty().LessThan(x => x.EndDate);
            RuleFor(x => x.EndDate).NotEmpty().LessThanOrEqualTo(DateTime.Today);
            RuleFor(x => x.DueDate).GreaterThan(DateTime.Today).When(x => x.DueDate.HasValue);
        }
    }

    public class PayInvoiceValidator : AbstractValidator<PayInvoiceDto>
    {
        public PayInvoiceValidator()
        {
            RuleFor(x => x.InvoiceId).GreaterThan(0);
            RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(999999999);
            RuleFor(x => x.PaymentMethod).NotEmpty();
        }
    }
}

namespace wedeli.Validators.Company
{
    public class CreateCompanyValidator : AbstractValidator<CreateCompanyDto>
    {
        public CreateCompanyValidator()
        {
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BusinessLicense).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Address).NotEmpty();
            RuleFor(x => x.Phone).NotEmpty().Matches(@"^(\+84|0)[0-9]{9,10}$");
            RuleFor(x => x.Email).EmailAddress().MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Email));
        }
    }

    public class UpdateCompanyValidator : AbstractValidator<UpdateCompanyDto>
    {
        public UpdateCompanyValidator()
        {
            RuleFor(x => x.CompanyName).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.CompanyName));
            RuleFor(x => x.Phone).Matches(@"^(\+84|0)[0-9]{9,10}$").When(x => !string.IsNullOrEmpty(x.Phone));
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}

namespace wedeli.Validators.Partnership
{
    public class CreatePartnershipValidator : AbstractValidator<CreatePartnershipDto>
    {
        public CreatePartnershipValidator()
        {
            RuleFor(x => x.CompanyId).GreaterThan(0).WithMessage("Company ID is required");
            RuleFor(x => x.PartnerCompanyId).GreaterThan(0).WithMessage("Partner Company ID is required")
                .NotEqual(x => x.CompanyId).WithMessage("Cannot create partnership with itself");
            RuleFor(x => x.PartnershipLevel).NotEmpty().Must(BeValidPartnershipLevel);
            RuleFor(x => x.CommissionRate).InclusiveBetween(0, 100);
            RuleFor(x => x.PriorityOrder).GreaterThanOrEqualTo(0);
        }

        private bool BeValidPartnershipLevel(string level)
        {
            return new[] { "preferred", "regular", "backup" }.Contains(level?.ToLower());
        }
    }

    public class UpdatePartnershipValidator : AbstractValidator<UpdatePartnershipDto>
    {
        public UpdatePartnershipValidator()
        {
            RuleFor(x => x.CommissionRate).InclusiveBetween(0, 100).When(x => x.CommissionRate.HasValue);
            RuleFor(x => x.PriorityOrder).GreaterThanOrEqualTo(0).When(x => x.PriorityOrder.HasValue);
        }
    }

    public class TransferOrderValidator : AbstractValidator<TransferOrderDto>
    {
        public TransferOrderValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.ToCompanyId).GreaterThan(0);
            RuleFor(x => x.TransferReason).NotEmpty().Must(BeValidTransferReason);
            RuleFor(x => x.AdminNotes).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.AdminNotes));
        }

        private bool BeValidTransferReason(string reason)
        {
            return new[] { "vehicle_full", "route_unavailable", "emergency", "partnership", "other" }.Contains(reason?.ToLower());
        }
    }

    public class AcceptTransferValidator : AbstractValidator<AcceptTransferDto>
    {
        public AcceptTransferValidator()
        {
            RuleFor(x => x.TransferId).GreaterThan(0);
            RuleFor(x => x.NewVehicleId).GreaterThan(0);
        }
    }

    public class RejectTransferValidator : AbstractValidator<RejectTransferDto>
    {
        public RejectTransferValidator()
        {
            RuleFor(x => x.TransferId).GreaterThan(0);
            RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(500);
        }
    }
}