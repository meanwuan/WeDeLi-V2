using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Rating
{
    // ============================================
    // RATING DTOs
    // ============================================

    public class RatingResponseDto
    {
        public int RatingId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public int RatingScore { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRatingDto
    {
        [Required] public int OrderId { get; set; }
        [Required] public int CustomerId { get; set; }
        public int? DriverId { get; set; }
        [Required][Range(1, 5)] public int RatingScore { get; set; }
        [MaxLength(1000)] public string ReviewText { get; set; }
    }

    public class UpdateRatingDto
    {
        [Range(1, 5)] public int? RatingScore { get; set; }
        [MaxLength(1000)] public string ReviewText { get; set; }
    }

    public class DriverRatingSummaryDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public List<RatingResponseDto> RecentRatings { get; set; }
    }
}

namespace wedeli.Models.DTO.Complaint
{
    // ============================================
    // COMPLAINT DTOs
    // ============================================

    public class ComplaintResponseDto
    {
        public int ComplaintId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ComplaintType { get; set; }
        public string Description { get; set; }
        public List<string> EvidencePhotos { get; set; }
        public string ComplaintStatus { get; set; }
        public string ResolutionNotes { get; set; }
        public int? ResolvedBy { get; set; }
        public string ResolvedByName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateComplaintDto
    {
        [Required] public int OrderId { get; set; }
        [Required] public int CustomerId { get; set; }
        [Required] public string ComplaintType { get; set; } // lost, damaged, late, wrong_address, other
        [Required][MaxLength(2000)] public string Description { get; set; }
        public List<string> EvidencePhotoUrls { get; set; }
    }

    public class UpdateComplaintDto
    {
        public string Description { get; set; }
        public List<string> EvidencePhotoUrls { get; set; }
    }

    public class ResolveComplaintDto
    {
        [Required] public int ComplaintId { get; set; }
        [Required][MaxLength(2000)] public string ResolutionNotes { get; set; }
        public decimal? RefundAmount { get; set; }
        public decimal? CompensationAmount { get; set; }
    }

    public class RejectComplaintDto
    {
        [Required] public int ComplaintId { get; set; }
        [Required][MaxLength(2000)] public string RejectionReason { get; set; }
    }

    public class ComplaintStatisticsDto
    {
        public int TotalComplaints { get; set; }
        public int PendingComplaints { get; set; }
        public int InvestigatingComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public int RejectedComplaints { get; set; }
        public Dictionary<string, int> ComplaintsByType { get; set; }
        public double ResolutionRate { get; set; }
        public double AverageResolutionTime { get; set; }
    }
}

namespace wedeli.Models.DTO.Report
{
    // ============================================
    // REPORT DTOs
    // ============================================

    public class DailySummaryDto
    {
        public DateTime SummaryDate { get; set; }
        public int TotalOrdersCreated { get; set; }
        public int TotalOrdersDelivered { get; set; }
        public int TotalOrdersCancelled { get; set; }
        public int TotalOrdersTransferred { get; set; }
        public int TotalVehiclesActive { get; set; }
        public int TotalVehiclesOverloaded { get; set; }
        public int TotalTripsCompleted { get; set; }
        public int TotalDriversActive { get; set; }
        public decimal AvgDeliveriesPerDriver { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCodCollected { get; set; }
        public decimal TotalCodSubmitted { get; set; }
        public decimal PendingCodAmount { get; set; }
        public int TotalComplaints { get; set; }
        public int ComplaintsResolved { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public class RevenueReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ShippingFeeRevenue { get; set; }
        public decimal CodRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}