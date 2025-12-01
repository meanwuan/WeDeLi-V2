using System;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Common
{
    // ============================================
    // COMMON REQUEST DTOs
    // ============================================

    /// <summary>
    /// Base DTO for entities with ID
    /// </summary>
    public class BaseIdDto
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// Date range filter
    /// </summary>
    public class DateRangeDto
    {
        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public void Validate()
        {
            if (FromDate > ToDate)
                throw new ArgumentException("FromDate cannot be greater than ToDate");
        }
    }

    /// <summary>
    /// Pagination and sorting parameters
    /// </summary>
    public class PaginationDto
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 20;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";

        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
        public bool IsSortDescending => SortOrder?.ToLower() == "desc";
    }

    /// <summary>
    /// Search with pagination
    /// </summary>
    public class SearchDto : PaginationDto
    {
        public string SearchTerm { get; set; }
    }

    /// <summary>
    /// ID with notes
    /// </summary>
    public class IdWithNotesDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Bulk operation request
    /// </summary>
    public class BulkOperationDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one ID is required")]
        public int[] Ids { get; set; }

        public string Operation { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// File upload DTO
    /// </summary>
    public class FileUploadDto
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileType { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        public long FileSizeBytes => FileData?.Length ?? 0;
        public string FileExtension => System.IO.Path.GetExtension(FileName);
    }

    /// <summary>
    /// Location DTO
    /// </summary>
    public class LocationDto
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        public string Address { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Notification DTO
    /// </summary>
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public string NotificationType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string SentVia { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Activity log DTO
    /// </summary>
    public class ActivityLogDto
    {
        public long LogId { get; set; }
        public DateTime LogDate { get; set; }
        public string LogType { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Action { get; set; }
        public string ChangedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Export request DTO
    /// </summary>
    public class ExportRequestDto
    {
        [Required]
        public string ExportType { get; set; } // excel, pdf, csv

        public DateRangeDto DateRange { get; set; }
        public string[] Columns { get; set; }
        public string[] Filters { get; set; }
    }

    /// <summary>
    /// Status change DTO
    /// </summary>
    public class StatusChangeDto
    {
        [Required]
        public int EntityId { get; set; }

        [Required]
        public string NewStatus { get; set; }

        public string Reason { get; set; }
    }

    /// <summary>
    /// Warehouse Staff DTO
    /// </summary>
    public class WarehouseStaffDto
    {
        public int StaffId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string WarehouseLocation { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class UpdateWarehouseStaffDto
    {
        public int StaffId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string WarehouseLocation { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateWarehouseStaffDto
    {
        [Required] public int UserId { get; set; }
        [Required] public int CompanyId { get; set; }
        [Required][MaxLength(200)] public string WarehouseLocation { get; set; }
    }

    /// <summary>
    /// Role DTO
    /// </summary>
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Dashboard stats DTO
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int InTransitOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int ActiveVehicles { get; set; }
        public int ActiveDrivers { get; set; }
        public int PendingComplaints { get; set; }
        public decimal PendingCodAmount { get; set; }
    }

    /// <summary>
    /// Key-Value DTO for dropdowns
    /// </summary>
    public class KeyValueDto
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class KeyValueDto<TKey>
    {
        public TKey Key { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Toggle user status DTO
    /// </summary>
    public class ToggleStatusRequestDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}