using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Vehicle
{
    // ============================================
    // VEHICLE RESPONSE DTO
    // ============================================

    public class VehicleResponseDto
    {
        public int VehicleId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleType { get; set; }
        public decimal MaxWeightKg { get; set; }
        public decimal? MaxVolumeM3 { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal CapacityPercentage { get; set; }
        public decimal OverloadThreshold { get; set; }
        public bool AllowOverload { get; set; }
        public string CurrentStatus { get; set; }
        public bool GpsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============================================
    // CREATE VEHICLE DTO
    // ============================================

    public class CreateVehicleDto
    {
        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "License Plate is required")]
        [MaxLength(20)]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{4,5}$", ErrorMessage = "Invalid license plate format")]
        public string LicensePlate { get; set; }

        [Required(ErrorMessage = "Vehicle Type is required")]
        public string VehicleType { get; set; } // truck, van, motorbike

        [Required(ErrorMessage = "Max Weight is required")]
        [Range(0.1, 100000, ErrorMessage = "Max Weight must be between 0.1 and 100000 kg")]
        public decimal MaxWeightKg { get; set; }

        [Range(0, 1000, ErrorMessage = "Max Volume must be positive")]
        public decimal? MaxVolumeM3 { get; set; }

        [Range(50, 100, ErrorMessage = "Overload Threshold must be between 50 and 100")]
        public decimal OverloadThreshold { get; set; } = 95;

        public bool GpsEnabled { get; set; } = true;
    }

    // ============================================
    // UPDATE VEHICLE DTO
    // ============================================

    public class UpdateVehicleDto
    {
        [MaxLength(20)]
        public string LicensePlate { get; set; }

        public string VehicleType { get; set; }

        [Range(0.1, 100000, ErrorMessage = "Max Weight must be positive")]
        public decimal? MaxWeightKg { get; set; }

        [Range(0, 1000, ErrorMessage = "Max Volume must be positive")]
        public decimal? MaxVolumeM3 { get; set; }

        [Range(50, 100, ErrorMessage = "Overload Threshold must be between 50 and 100")]
        public decimal? OverloadThreshold { get; set; }

        public bool? AllowOverload { get; set; }

        public string CurrentStatus { get; set; }

        public bool? GpsEnabled { get; set; }
    }

    // ============================================
    // VEHICLE DETAIL DTO
    // ============================================

    public class VehicleDetailDto : VehicleResponseDto
    {
        // Current Assignment
        public int? CurrentDriverId { get; set; }
        public string CurrentDriverName { get; set; }
        public int? CurrentTripId { get; set; }
        public string CurrentRouteName { get; set; }

        // Load Information
        public int CurrentOrderCount { get; set; }
        public decimal AvailableWeightKg { get; set; }
        public decimal AvailableVolumeM3 { get; set; }
        public bool IsOverloaded { get; set; }

        // Statistics
        public int TotalTripsCompleted { get; set; }
        public int TotalOrdersDelivered { get; set; }
        public decimal TotalDistanceKm { get; set; }

        // Maintenance
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public int DaysUntilMaintenance { get; set; }

        // Activity
        public DateTime? LastActiveDate { get; set; }
        public int TripsThisMonth { get; set; }
        public int TripsLastMonth { get; set; }
    }

    // ============================================
    // VEHICLE CAPACITY DTO
    // ============================================

    public class VehicleCapacityDto
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public decimal MaxWeightKg { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal AvailableWeightKg { get; set; }
        public decimal CapacityPercentage { get; set; }
        public decimal OverloadThreshold { get; set; }
        public bool IsOverloaded { get; set; }
        public bool AllowOverload { get; set; }
        public int CurrentOrderCount { get; set; }
        public bool CanAcceptMoreOrders { get; set; }
    }

    // ============================================
    // VEHICLE STATUS UPDATE DTO
    // ============================================

    public class UpdateVehicleStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } // available, in_transit, maintenance, inactive, overloaded

        public string Notes { get; set; }
    }

    // ============================================
    // VEHICLE LOAD UPDATE DTO
    // ============================================

    public class UpdateVehicleLoadDto
    {
        [Required(ErrorMessage = "Weight change is required")]
        public decimal WeightChangeKg { get; set; } // Positive for adding, negative for removing

        public string Reason { get; set; }
    }

    // ============================================
    // OVERLOAD APPROVAL DTO
    // ============================================

    public class ApproveOverloadDto
    {
        [Required(ErrorMessage = "Vehicle ID is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Allow Overload flag is required")]
        public bool AllowOverload { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; }

        public decimal? TemporaryMaxWeight { get; set; }
    }

    // ============================================
    // VEHICLE ASSIGNMENT DTO
    // ============================================

    public class VehicleAssignmentDto
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleType { get; set; }
        public string CurrentStatus { get; set; }
        public int? CurrentDriverId { get; set; }
        public string CurrentDriverName { get; set; }
        public decimal CapacityPercentage { get; set; }
        public decimal AvailableWeightKg { get; set; }
        public bool IsAvailable { get; set; }
    }

    // ============================================
    // VEHICLE STATISTICS DTO
    // ============================================

    public class VehicleStatisticsDto
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; }

        // Trip Statistics
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int CancelledTrips { get; set; }

        // Delivery Statistics
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public double SuccessRate { get; set; }

        // Utilization
        public decimal TotalDistanceKm { get; set; }
        public decimal AverageLoadPercentage { get; set; }
        public int DaysInOperation { get; set; }
        public int DaysIdle { get; set; }
        public double UtilizationRate { get; set; }

        // Maintenance
        public int MaintenanceCount { get; set; }
        public int DaysInMaintenance { get; set; }

        // Time Period
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    // ============================================
    // VEHICLE OVERLOAD REPORT DTO
    // ============================================

    public class VehicleOverloadReportDto
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public decimal MaxWeightKg { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal CapacityPercentage { get; set; }
        public decimal OverloadThreshold { get; set; }
        public bool IsOverloaded { get; set; }
        public bool AllowOverload { get; set; }
        public int OrderCount { get; set; }
        public string CurrentDriverName { get; set; }
        public string CurrentTripStatus { get; set; }
        public List<OrderOnVehicleDto> Orders { get; set; }
    }

    public class OrderOnVehicleDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public decimal WeightKg { get; set; }
        public string ReceiverAddress { get; set; }
        public string OrderStatus { get; set; }
    }

    // ============================================
    // VEHICLE SEARCH/FILTER DTO
    // ============================================

    public class VehicleSearchDto
    {
        public int? CompanyId { get; set; }
        public string SearchTerm { get; set; }
        public string VehicleType { get; set; }
        public string CurrentStatus { get; set; }
        public bool? IsOverloaded { get; set; }
        public bool? GpsEnabled { get; set; }
        public decimal? MinCapacity { get; set; }
        public decimal? MaxCapacity { get; set; }
        public string SortBy { get; set; } = "licensePlate";
        public string SortOrder { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}