namespace wedeli.Models.DTO
{
    /// <summary>
    /// DTO for creating a new vehicle
    /// </summary>
    public class CreateVehicleDto
    {
        public int CompanyId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!; // truck, van, motorbike
        public decimal? MaxWeightKg { get; set; }
        public decimal? MaxVolumeM3 { get; set; }
        public decimal OverloadThreshold { get; set; } = 95.00M;
        public bool AllowOverload { get; set; } = false;
        public bool GpsEnabled { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating vehicle information
    /// </summary>
    public class UpdateVehicleDto
    {
        public string? LicensePlate { get; set; }
        public string? VehicleType { get; set; }
        public decimal? MaxWeightKg { get; set; }
        public decimal? MaxVolumeM3 { get; set; }
        public decimal? CurrentWeightKg { get; set; }
        public string? CurrentStatus { get; set; } // available, in_transit, maintenance, inactive, overloaded
        public decimal? OverloadThreshold { get; set; }
        public bool? AllowOverload { get; set; }
        public bool? GpsEnabled { get; set; }
    }

    /// <summary>
    /// Response DTO for vehicle details
    /// </summary>
    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public decimal? MaxWeightKg { get; set; }
        public decimal? MaxVolumeM3 { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal CapacityPercentage { get; set; }
        public decimal OverloadThreshold { get; set; }
        public bool AllowOverload { get; set; }
        public string CurrentStatus { get; set; } = null!;
        public bool GpsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
        public bool IsOverloaded => CapacityPercentage >= OverloadThreshold;
        public decimal RemainingCapacityKg => (MaxWeightKg ?? 0) - CurrentWeightKg;
    }

    /// <summary>
    /// DTO for updating vehicle load (weight)
    /// </summary>
    public class UpdateVehicleLoadDto
    {
        public int VehicleId { get; set; }
        public decimal WeightKg { get; set; }
        public bool IsAdding { get; set; } = true; // true = add weight, false = remove weight
    }

    /// <summary>
    /// Response DTO after updating load
    /// </summary>
    public class VehicleLoadResponseDto
    {
        public int VehicleId { get; set; }
        public decimal OldWeightKg { get; set; }
        public decimal NewWeightKg { get; set; }
        public decimal CapacityPercentage { get; set; }
        public bool IsOverloaded { get; set; }
        public string CurrentStatus { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool CanAddMoreOrders { get; set; }
    }

    /// <summary>
    /// DTO for vehicle statistics
    /// </summary>
    public class VehicleStatisticsDto
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public int TotalTrips { get; set; }
        public int TotalOrdersDelivered { get; set; }
        public decimal TotalDistanceKm { get; set; }
        public decimal AverageLoadPercentage { get; set; }
        public int DaysInService { get; set; }
        public DateOnly? LastTripDate { get; set; }
    }

    /// <summary>
    /// Filter parameters for vehicle queries
    /// </summary>
    public class VehicleFilterDto
    {
        public int? CompanyId { get; set; }
        public string? VehicleType { get; set; }
        public string? CurrentStatus { get; set; }
        public bool? GpsEnabled { get; set; }
        public bool? ShowOnlyOverloaded { get; set; }
        public bool? ShowOnlyAvailable { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}