using FluentValidation;
using wedeli.Models.DTO.Rating;
using wedeli.Models.DTO.Complaint;
using wedeli.Models.DTO.Common;

namespace wedeli.Validators.Rating
{
    public class CreateRatingValidator : AbstractValidator<CreateRatingDto>
    {
        public CreateRatingValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("Order ID is required");
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer ID is required");
            RuleFor(x => x.RatingScore).InclusiveBetween(1, 5).WithMessage("Rating Score must be between 1 and 5");
            RuleFor(x => x.ReviewText).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.ReviewText));
        }
    }

    public class UpdateRatingValidator : AbstractValidator<UpdateRatingDto>
    {
        public UpdateRatingValidator()
        {
            RuleFor(x => x.RatingScore).InclusiveBetween(1, 5).When(x => x.RatingScore.HasValue);
            RuleFor(x => x.ReviewText).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.ReviewText));
        }
    }
}

namespace wedeli.Validators.Complaint
{
    public class CreateComplaintValidator : AbstractValidator<CreateComplaintDto>
    {
        public CreateComplaintValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("Order ID is required");
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer ID is required");

            RuleFor(x => x.ComplaintType)
                .NotEmpty().WithMessage("Complaint Type is required")
                .Must(BeValidComplaintType).WithMessage("Invalid Complaint Type");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MinimumLength(20).WithMessage("Description must be at least 20 characters")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.EvidencePhotoUrls)
                .Must(HaveValidPhotoUrls).WithMessage("All evidence photo URLs must be valid URLs")
                .When(x => x.EvidencePhotoUrls != null && x.EvidencePhotoUrls.Any());
        }

        private bool BeValidComplaintType(string type)
        {
            var validTypes = new[] { "lost", "damaged", "late", "wrong_address", "other" };
            return validTypes.Contains(type?.ToLower());
        }

        private bool HaveValidPhotoUrls(List<string> urls)
        {
            if (urls == null || !urls.Any()) return true;
            return urls.All(url => Uri.TryCreate(url, UriKind.Absolute, out _));
        }
    }

    public class UpdateComplaintValidator : AbstractValidator<UpdateComplaintDto>
    {
        public UpdateComplaintValidator()
        {
            RuleFor(x => x.Description)
                .MinimumLength(20).WithMessage("Description must be at least 20 characters")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }

    public class ResolveComplaintValidator : AbstractValidator<ResolveComplaintDto>
    {
        public ResolveComplaintValidator()
        {
            RuleFor(x => x.ComplaintId).GreaterThan(0);

            RuleFor(x => x.ResolutionNotes)
                .NotEmpty().WithMessage("Resolution Notes are required")
                .MinimumLength(20).WithMessage("Resolution Notes must be at least 20 characters")
                .MaximumLength(2000).WithMessage("Resolution Notes cannot exceed 2000 characters");

            RuleFor(x => x.RefundAmount)
                .GreaterThan(0).WithMessage("Refund Amount must be positive")
                .When(x => x.RefundAmount.HasValue);

            RuleFor(x => x.CompensationAmount)
                .GreaterThan(0).WithMessage("Compensation Amount must be positive")
                .When(x => x.CompensationAmount.HasValue);
        }
    }

    public class RejectComplaintValidator : AbstractValidator<RejectComplaintDto>
    {
        public RejectComplaintValidator()
        {
            RuleFor(x => x.ComplaintId).GreaterThan(0);

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection Reason is required")
                .MinimumLength(20).WithMessage("Rejection Reason must be at least 20 characters")
                .MaximumLength(2000).WithMessage("Rejection Reason cannot exceed 2000 characters");
        }
    }
}

namespace wedeli.Validators.Common
{
    // ============================================
    // DATE RANGE VALIDATOR
    // ============================================

    public class DateRangeValidator : AbstractValidator<DateRangeDto>
    {
        public DateRangeValidator()
        {
            RuleFor(x => x.FromDate)
                .NotEmpty().WithMessage("From Date is required")
                .LessThanOrEqualTo(x => x.ToDate).WithMessage("From Date must be before or equal to To Date");

            RuleFor(x => x.ToDate)
                .NotEmpty().WithMessage("To Date is required")
                .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("To Date cannot be in the future");
        }
    }

    // ============================================
    // FILE UPLOAD VALIDATOR
    // ============================================

    public class FileUploadValidator : AbstractValidator<FileUploadDto>
    {
        private const int MaxFileSizeMB = 10;
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };

        public FileUploadValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required")
                .Must(HaveValidExtension).WithMessage($"Invalid file extension. Allowed: {string.Join(", ", AllowedExtensions)}");

            RuleFor(x => x.FileType)
                .NotEmpty().WithMessage("File type is required");

            RuleFor(x => x.FileData)
                .NotNull().WithMessage("File data is required")
                .Must(BeValidFileSize).WithMessage($"File size cannot exceed {MaxFileSizeMB} MB");
        }

        private bool HaveValidExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            var extension = System.IO.Path.GetExtension(fileName).ToLower();
            return AllowedExtensions.Contains(extension);
        }

        private bool BeValidFileSize(byte[] fileData)
        {
            if (fileData == null) return false;
            return fileData.Length <= MaxFileSizeMB * 1024 * 1024;
        }
    }

    // ============================================
    // LOCATION VALIDATOR
    // ============================================

    public class LocationValidator : AbstractValidator<LocationDto>
    {
        public LocationValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }

    // ============================================
    // WAREHOUSE STAFF VALIDATOR
    // ============================================

    public class CreateWarehouseStaffValidator : AbstractValidator<CreateWarehouseStaffDto>
    {
        public CreateWarehouseStaffValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("User ID is required");
            RuleFor(x => x.CompanyId).GreaterThan(0).WithMessage("Company ID is required");

            RuleFor(x => x.WarehouseLocation)
                .NotEmpty().WithMessage("Warehouse Location is required")
                .MaximumLength(200).WithMessage("Warehouse Location cannot exceed 200 characters");
        }
    }

    // ============================================
    // BULK OPERATION VALIDATOR
    // ============================================

    public class BulkOperationValidator : AbstractValidator<BulkOperationDto>
    {
        public BulkOperationValidator()
        {
            RuleFor(x => x.Ids)
                .NotEmpty().WithMessage("At least one ID is required")
                .Must(HaveUniqueIds).WithMessage("IDs must be unique");

            RuleFor(x => x.Operation)
                .NotEmpty().WithMessage("Operation is required")
                .When(x => !string.IsNullOrEmpty(x.Operation));

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }

        private bool HaveUniqueIds(int[] ids)
        {
            return ids.Distinct().Count() == ids.Length;
        }
    }

    // ============================================
    // EXPORT REQUEST VALIDATOR
    // ============================================

    public class ExportRequestValidator : AbstractValidator<ExportRequestDto>
    {
        public ExportRequestValidator()
        {
            RuleFor(x => x.ExportType)
                .NotEmpty().WithMessage("Export Type is required")
                .Must(BeValidExportType).WithMessage("Invalid Export Type. Valid types: excel, pdf, csv");
        }

        private bool BeValidExportType(string exportType)
        {
            var validTypes = new[] { "excel", "pdf", "csv" };
            return validTypes.Contains(exportType?.ToLower());
        }
    }

    // ============================================
    // PAGINATION VALIDATOR
    // ============================================

    public class PaginationValidator : AbstractValidator<PaginationDto>
    {
        public PaginationValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page Number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page Size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page Size cannot exceed 100");

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