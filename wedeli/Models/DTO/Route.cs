using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Route
{
    // ============================================
    // ROUTE DTOs
    // ============================================

    public class RouteResponseDto
    {
        public int RouteId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string RouteName { get; set; }
        public string OriginProvince { get; set; }
        public string OriginDistrict { get; set; }
        public string DestinationProvince { get; set; }
        public string DestinationDistrict { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? EstimatedDurationHours { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRouteDto
    {
        [Required] public int CompanyId { get; set; }
        [Required][MaxLength(200)] public string RouteName { get; set; }
        [Required][MaxLength(100)] public string OriginProvince { get; set; }
        [Required][MaxLength(100)] public string OriginDistrict { get; set; }
        [Required][MaxLength(100)] public string DestinationProvince { get; set; }
        [Required][MaxLength(100)] public string DestinationDistrict { get; set; }
        [Range(0, 10000)] public decimal? DistanceKm { get; set; }
        [Range(0, 1000)] public decimal? EstimatedDurationHours { get; set; }
        [Required][Range(0, 999999999)] public decimal BasePrice { get; set; }
    }

    public class UpdateRouteDto
    {
        public string RouteName { get; set; }
        public string OriginProvince { get; set; }
        public string OriginDistrict { get; set; }
        public string DestinationProvince { get; set; }
        public string DestinationDistrict { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? EstimatedDurationHours { get; set; }
        public decimal? BasePrice { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CalculateShippingFeeDto
    {
        [Required] public int RouteId { get; set; }
        [Required][Range(0.1, 10000)] public decimal WeightKg { get; set; }
        public string ParcelType { get; set; }
        public decimal? DeclaredValue { get; set; }
    }

    public class ShippingFeeResponseDto
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal WeightFee { get; set; }
        public decimal ParcelTypeFee { get; set; }
        public decimal InsuranceFee { get; set; }
        public decimal TotalFee { get; set; }
        public decimal? EstimatedDurationHours { get; set; }
    }

    /// <summary>
    /// DTO for recommending transport companies based on route
    /// </summary>
    public class CompanyRouteRecommendationDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public decimal? Rating { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        
        // Best route for this company matching the request
        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? EstimatedDurationHours { get; set; }
        
        // Calculated score (higher is better)
        public decimal RecommendationScore { get; set; }
    }
}

namespace wedeli.Models.DTO.Trip
{
    // ============================================
    // TRIP DTOs
    // ============================================

    public class TripResponseDto
    {
        public int TripId { get; set; }
        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleLicensePlate { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public DateTime TripDate { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string TripStatus { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalWeightKg { get; set; }
        public bool IsReturnTrip { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTripDto
    {
        [Required] public int RouteId { get; set; }
        [Required] public int VehicleId { get; set; }
        [Required] public int DriverId { get; set; }
        [Required] public DateTime TripDate { get; set; }
        public DateTime? DepartureTime { get; set; }
        public bool IsReturnTrip { get; set; } = false;
    }

    public class UpdateTripDto
    {
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public DateTime? TripDate { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string TripStatus { get; set; }
    }

    public class TripDetailDto : TripResponseDto
    {
        public string OriginProvince { get; set; }
        public string DestinationProvince { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal VehicleCapacityPercentage { get; set; }
        public List<TripOrderDto> Orders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TripOrderDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }
        public decimal WeightKg { get; set; }
        public string OrderStatus { get; set; }
        public int? SequenceNumber { get; set; }
        public bool PickupConfirmed { get; set; }
        public bool DeliveryConfirmed { get; set; }
    }

    public class AddOrderToTripDto
    {
        [Required] public int TripId { get; set; }
        [Required] public int OrderId { get; set; }
        public int? SequenceNumber { get; set; }
    }

    public class UpdateTripStatusDto
    {
        [Required] public string Status { get; set; }
        public string Notes { get; set; }
    }
}