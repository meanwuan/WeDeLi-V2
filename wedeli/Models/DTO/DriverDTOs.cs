using wedeli.Models.DTO;
namespace wedeli.Models.DTO
{
    /// <summary>
    /// DTO for creating a new driver
    /// </summary>
    public class CreateDriverDto
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string? DriverLicense { get; set; }
        public DateTime? LicenseExpiry { get; set; }
    }

    /// <summary>
    /// DTO for updating driver information
    /// </summary>
    public class UpdateDriverDto
    {
        public string? DriverLicense { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public bool? IsActive { get; set; }
        public decimal? Rating { get; set; }
    }

    /// <summary>
    /// Response DTO for driver details
    /// </summary>
    public class DriverDto
    {
        public int DriverId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? DriverLicense { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public int TotalTrips { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal Rating { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
        public bool IsLicenseExpired => LicenseExpiry.HasValue && LicenseExpiry.Value < DateTime.Now;
        public int DaysUntilLicenseExpiry => LicenseExpiry.HasValue
            ? (int)(LicenseExpiry.Value - DateTime.Now).TotalDays
            : 0;
    }

    /// <summary>
    /// DTO for driver performance statistics
    /// </summary>
    public class DriverPerformanceDto
    {
        public int DriverId { get; set; }
        public string FullName { get; set; } = null!;
        public string? LicensePlate { get; set; }

        // Trip statistics
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int CancelledTrips { get; set; }

        // Order statistics
        public int TotalOrdersAssigned { get; set; }
        public int OrdersDelivered { get; set; }
        public int OrdersReturned { get; set; }
        public int OrdersCancelled { get; set; }

        // Performance metrics
        public decimal SuccessRate { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }

        // Financial
        public decimal TotalCodCollected { get; set; }
        public decimal TotalCodSubmitted { get; set; }
        public decimal PendingCodAmount { get; set; }

        // Time tracking
        public DateTime? LastDeliveryDate { get; set; }
        public int DaysActive { get; set; }
        public decimal AverageDeliveriesPerDay { get; set; }
    }

    /// <summary>
    /// DTO for driver's assigned orders
    /// </summary>
    public class DriverOrdersDto
    {
        public int DriverId { get; set; }
        public string FullName { get; set; } = null!;
        public int TotalOrders { get; set; }
        public List<OrderListItem> Orders { get; set; } = new();
    }

    /// <summary>
    /// DTO for driver daily summary
    /// </summary>
    public class DriverDailySummaryDto
    {
        public int DriverId { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime Date { get; set; }
        public int OrdersAssigned { get; set; }
        public int OrdersDelivered { get; set; }
        public int OrdersPending { get; set; }
        public decimal TotalCodCollected { get; set; }
        public decimal TotalDistanceKm { get; set; }
        public int TripCount { get; set; }
    }

    /// <summary>
    /// Filter parameters for driver queries
    /// </summary>
    public class DriverFilterDto
    {
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public bool? ShowOnlyAvailable { get; set; }
        public decimal? MinRating { get; set; }
        public bool? LicenseExpiringSoon { get; set; } // Within 30 days
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for driver location update (for real-time tracking)
    /// </summary>
    public class DriverLocationDto
    {
        public int DriverId { get; set; }
        public int? CurrentOrderId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }
}