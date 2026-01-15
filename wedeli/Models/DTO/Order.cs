using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Order
{
    // ============================================
    // CREATE ORDER
    // ============================================

    public class CreateOrderDto
    {
        // CustomerId: 0 = walk-in customer (auto-create from sender info), >0 = existing customer
        public int CustomerId { get; set; }

        // Sender Information
        [Required(ErrorMessage = "Sender Name is required")]
        [MaxLength(200)]
        public string SenderName { get; set; }

        [Required(ErrorMessage = "Sender Phone is required")]
        public string SenderPhone { get; set; }

        [Required(ErrorMessage = "Sender Address is required")]
        public string SenderAddress { get; set; }

        // Receiver Information
        [Required(ErrorMessage = "Receiver Name is required")]
        [MaxLength(200)]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Receiver Phone is required")]
        public string ReceiverPhone { get; set; }

        [Required(ErrorMessage = "Receiver Address is required")]
        public string ReceiverAddress { get; set; }

        [Required(ErrorMessage = "Receiver Province is required")]
        public string ReceiverProvince { get; set; }

        [Required(ErrorMessage = "Receiver District is required")]
        public string ReceiverDistrict { get; set; }

        public string ReceiverWard { get; set; }

        // Parcel Information
        [Required(ErrorMessage = "Parcel Type is required")]
        public string ParcelType { get; set; } // fragile, electronics, food, cold, document, other

        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 10000, ErrorMessage = "Weight must be between 0.1 and 10000 kg")]
        public decimal WeightKg { get; set; }

        [Range(typeof(decimal), "0", "9999999999", ErrorMessage = "Declared value must be positive")]
        public decimal? DeclaredValue { get; set; }

        public string SpecialInstructions { get; set; }

        // Route & Assignment
        public int? RouteId { get; set; }
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }

        // Payment
        [Required(ErrorMessage = "Payment Method is required")]
        public string PaymentMethod { get; set; } // cash, bank_transfer, e_wallet, periodic

        [Range(typeof(decimal), "0", "9999999999", ErrorMessage = "COD Amount must be positive")]
        public decimal CodAmount { get; set; } = 0;

        // Schedule
        public DateTime? PickupScheduledAt { get; set; }
    }

    // ============================================
    // UPDATE ORDER
    // ============================================

    public class UpdateOrderDto
    {
        public string? SenderName { get; set; }
        public string? SenderPhone { get; set; }
        public string? SenderAddress { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? ReceiverAddress { get; set; }
        public string? ReceiverProvince { get; set; }
        public string? ReceiverDistrict { get; set; }
        public string? ParcelType { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? DeclaredValue { get; set; }
        public string? SpecialInstructions { get; set; }
        public int? RouteId { get; set; }
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public decimal? CodAmount { get; set; }
        public DateTime? PickupScheduledAt { get; set; }
        
        // Status fields for admin updates
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
    }

    // ============================================
    // ORDER RESPONSE
    // ============================================

    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }

        // Customer
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public bool IsRegularCustomer { get; set; }

        // Sender
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string SenderAddress { get; set; }

        // Receiver
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverProvince { get; set; }
        public string ReceiverDistrict { get; set; }

        // Parcel
        public string ParcelType { get; set; }
        public decimal WeightKg { get; set; }
        public decimal? DeclaredValue { get; set; }
        public string SpecialInstructions { get; set; }

        // Assignment
        public int? RouteId { get; set; }
        public string RouteName { get; set; }
        public int? VehicleId { get; set; }
        public string VehicleLicensePlate { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }

        // Financial
        public decimal ShippingFee { get; set; }
        public decimal CodAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? PaidAt { get; set; }

        // Status
        public string OrderStatus { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? PickupScheduledAt { get; set; }
        public DateTime? PickupConfirmedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Photos
        public List<OrderPhotoDto> Photos { get; set; }

        // Status History
        public List<OrderStatusDto> StatusHistory { get; set; }
    }

    // ============================================
    // ORDER DETAIL DTO
    // ============================================

    public class OrderDetailDto : OrderResponseDto
    {
        // Company Information
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }

        // Extended Information
        public int? TripId { get; set; }
        public bool IsReturnTrip { get; set; }

        // Transfer Information
        public bool IsTransferred { get; set; }
        public int? TransferredFromCompanyId { get; set; }
        public string TransferredFromCompanyName { get; set; }
        public string TransferReason { get; set; }

        // COD Information
        public bool HasCodTransaction { get; set; }
        public string CodStatus { get; set; }
        public DateTime? CodCollectedAt { get; set; }
        public DateTime? CodSubmittedAt { get; set; }

        // Ratings & Complaints
        public bool HasRating { get; set; }
        public int? RatingScore { get; set; }
        public string ReviewText { get; set; }
        public bool HasComplaint { get; set; }
        public string ComplaintType { get; set; }
        public string ComplaintStatus { get; set; }
    }

    // ============================================
    // ORDER LIST ITEM DTO (for list views)
    // ============================================

    public class OrderListItemDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public string CustomerName { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddress { get; set; }
        public string OrderStatus { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal CodAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string DriverName { get; set; }
        public string VehicleLicensePlate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    // ============================================
    // ORDER STATUS DTO
    // ============================================

    public class OrderStatusDto
    {
        public int HistoryId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public int? UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "New Status is required")]
        public string NewStatus { get; set; } // pending_pickup, picked_up, in_transit, out_for_delivery, delivered, returned, cancelled

        public string Location { get; set; }
        public string Notes { get; set; }
    }

    // ============================================
    // ORDER PHOTO DTO
    // ============================================

    public class OrderPhotoDto
    {
        public int PhotoId { get; set; }
        public string PhotoType { get; set; } // before_delivery, after_delivery, parcel_condition, signature, damage_proof
        public string PhotoUrl { get; set; }
        public string FileName { get; set; }
        public int? FileSizeKb { get; set; }
        public int? UploadedBy { get; set; }
        public string UploadedByName { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class UploadOrderPhotoDto
    {
        [Required(ErrorMessage = "Order ID is required")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Photo Type is required")]
        public string PhotoType { get; set; } // before_delivery, after_delivery, parcel_condition, signature, damage_proof

        [Required(ErrorMessage = "Photo file is required")]
        public byte[] PhotoFile { get; set; }

        [Required(ErrorMessage = "File name is required")]
        public string FileName { get; set; }
        public int? UploadedBy { get; internal set; }
        public string PhotoUrl { get; internal set; }
    }

    // ============================================
    // ORDER TRACKING DTO
    // ============================================

    public class OrderTrackingDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public string OrderStatus { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }
        public decimal ShippingFee { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedDeliveryAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Current Location
        public string CurrentLocation { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }

        // Driver Information
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }

        // Timeline
        public List<OrderStatusDto> Timeline { get; set; }

        // Photos
        public string BeforeDeliveryPhotoUrl { get; set; }
        public string AfterDeliveryPhotoUrl { get; set; }
    }

    // ============================================
    // ORDER ASSIGNMENT DTO
    // ============================================

    public class AssignOrderDto
    {
        [Required(ErrorMessage = "Order ID is required")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Driver ID is required")]
        public int DriverId { get; set; }

        [Required(ErrorMessage = "Vehicle ID is required")]
        public int VehicleId { get; set; }

        public int? RouteId { get; set; }
        public int? TripId { get; set; }
        public DateTime? ScheduledPickupTime { get; set; }
        public string Notes { get; set; }
    }

    // ============================================
    // CANCEL ORDER DTO
    // ============================================

    public class CancelOrderDto
    {
        [Required(ErrorMessage = "Cancellation Reason is required")]
        [MaxLength(500)]
        public string CancellationReason { get; set; }

        public bool RefundRequested { get; set; } = false;
    }

    // ============================================
    // ORDER STATISTICS DTO
    // ============================================

    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int InTransitOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCodAmount { get; set; }
        public decimal AverageShippingFee { get; set; }
        public double SuccessRate { get; set; }
    }
}