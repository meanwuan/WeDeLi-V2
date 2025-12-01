using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Driver
{
    // ============================================
    // DRIVER RESPONSE DTO
    // ============================================

    public class DriverResponseDto
    {
        public int DriverId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string DriverLicense { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public int TotalTrips { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal Rating { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============================================
    // CREATE DRIVER DTO
    // ============================================

    public class CreateDriverDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Driver License is required")]
        [MaxLength(50)]
        public string DriverLicense { get; set; }

        [Required(ErrorMessage = "License Expiry is required")]
        public DateTime LicenseExpiry { get; set; }
    }

    // ============================================
    // UPDATE DRIVER DTO
    // ============================================

    public class UpdateDriverDto
    {
        [MaxLength(50)]
        public string DriverLicense { get; set; }

        public DateTime? LicenseExpiry { get; set; }

        public bool? IsActive { get; set; }
    }

    // ============================================
    // DRIVER DETAIL DTO
    // ============================================

    public class DriverDetailDto : DriverResponseDto
    {
        // Current Assignment
        public int? CurrentTripId { get; set; }
        public int? CurrentVehicleId { get; set; }
        public string CurrentVehicleLicensePlate { get; set; }
        public string CurrentTripStatus { get; set; }

        // Statistics
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public int PendingDeliveries { get; set; }

        // Financial
        public decimal TotalCodCollected { get; set; }
        public decimal PendingCodAmount { get; set; }
        public decimal TotalEarnings { get; set; }

        // Performance
        public decimal AverageDeliveryTime { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }

        // Activity
        public DateTime? LastActiveDate { get; set; }
        public int TripsThisMonth { get; set; }
        public int TripsLastMonth { get; set; }
    }

    // ============================================
    // DRIVER STATISTICS DTO
    // ============================================

    public class DriverStatisticsDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }

        // Trip Statistics
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int InProgressTrips { get; set; }
        public int CancelledTrips { get; set; }

        // Delivery Statistics
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }

        // Performance Metrics
        public decimal AverageDeliveryTime { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
        public double OnTimeRate { get; set; }

        // COD Management
        public decimal TotalCodCollected { get; set; }
        public decimal TotalCodSubmitted { get; set; }
        public decimal PendingCodAmount { get; set; }

        // Rating
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }

        // Time Period
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    // ============================================
    // DRIVER ASSIGNMENT DTO
    // ============================================

    public class DriverAssignmentDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int CurrentVehicleId { get; set; }
        public string VehicleLicensePlate { get; set; }
        public int CurrentOrderCount { get; set; }
        public decimal CurrentLoadWeight { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? LastDeliveryTime { get; set; }
    }

    // ============================================
    // DRIVER COD SUMMARY DTO
    // ============================================

    public class DriverCodSummaryDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public DateTime SummaryDate { get; set; }
        public decimal TotalCodCollected { get; set; }
        public decimal TotalCodSubmitted { get; set; }
        public decimal PendingAmount { get; set; }
        public string ReconciliationStatus { get; set; }
        public DateTime? ReconciledAt { get; set; }
        public string ReconciledByName { get; set; }
        public List<CodTransactionItemDto> Transactions { get; set; }
    }

    public class CodTransactionItemDto
    {
        public int TransactionId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public decimal CodAmount { get; set; }
        public string CollectionStatus { get; set; }
        public DateTime? CollectedAt { get; set; }
        public bool SubmittedToCompany { get; set; }
    }

    // ============================================
    // DRIVER SCHEDULE DTO
    // ============================================

    public class DriverScheduleDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public DateTime ScheduleDate { get; set; }
        public List<ScheduledTripDto> Trips { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalWeight { get; set; }
    }

    public class ScheduledTripDto
    {
        public int TripId { get; set; }
        public string RouteName { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string TripStatus { get; set; }
        public int OrderCount { get; set; }
        public List<string> DeliveryAddresses { get; set; }
    }

    // ============================================
    // DRIVER PERFORMANCE DTO
    // ============================================

    public class DriverPerformanceDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public string Period { get; set; }

        // Efficiency Metrics
        public int TotalTrips { get; set; }
        public int TotalDeliveries { get; set; }
        public double AverageDeliveriesPerTrip { get; set; }
        public decimal AverageDeliveryTime { get; set; }

        // Quality Metrics
        public double SuccessRate { get; set; }
        public double OnTimeRate { get; set; }
        public double AverageRating { get; set; }
        public int ComplaintCount { get; set; }

        // Financial Metrics
        public decimal TotalRevenue { get; set; }
        public decimal TotalCodCollected { get; set; }
        public decimal CodReconciliationRate { get; set; }

        // Ranking
        public int RankAmongDrivers { get; set; }
        public int TotalDriversInCompany { get; set; }
        public decimal Rating { get; internal set; }
    }
}