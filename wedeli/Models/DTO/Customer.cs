using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Customer
{
    // ============================================
    // CUSTOMER RESPONSE DTO
    // ============================================

    public class CustomerResponseDto
    {
        public int CustomerId { get; set; }
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // Customer Status
        public bool IsRegular { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        // Payment Settings
        public string PaymentPrivilege { get; set; } // prepay, postpay, periodic
        public decimal CreditLimit { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ============================================
    // CREATE CUSTOMER DTO
    // ============================================

    public class CreateCustomerDto
    {
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(200)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone format")]
        [RegularExpression(@"^(\+84|0)[0-9]{9,10}$", ErrorMessage = "Phone must be a valid Vietnamese number")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email { get; set; }
    }

    // ============================================
    // UPDATE CUSTOMER DTO
    // ============================================

    public class UpdateCustomerDto
    {
        [MaxLength(200)]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email { get; set; }
    }

    // ============================================
    // CUSTOMER DETAIL DTO
    // ============================================

    public class CustomerDetailDto : CustomerResponseDto
    {
        // Statistics
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime? LastOrderDate { get; set; }

        // Payment Info
        public decimal OutstandingBalance { get; set; }
        public int PendingInvoices { get; set; }

        // Addresses
        public List<CustomerAddressDto> Addresses { get; set; }

        // Recent Orders
        public List<RecentOrderDto> RecentOrders { get; set; }
    }

    // ============================================
    // CUSTOMER ADDRESS DTO
    // ============================================

    public class CustomerAddressDto
    {
        public int AddressId { get; set; }
        public int CustomerId { get; set; }
        public string AddressLabel { get; set; } // Home, Office, etc
        public string FullAddress { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCustomerAddressDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        [MaxLength(100)]
        public string AddressLabel { get; set; }

        [Required(ErrorMessage = "Full Address is required")]
        public string FullAddress { get; set; }

        [Required(ErrorMessage = "Province is required")]
        [MaxLength(100)]
        public string Province { get; set; }

        [Required(ErrorMessage = "District is required")]
        [MaxLength(100)]
        public string District { get; set; }

        [MaxLength(100)]
        public string Ward { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateCustomerAddressDto
    {
        [MaxLength(100)]
        public string AddressLabel { get; set; }

        public string FullAddress { get; set; }

        [MaxLength(100)]
        public string Province { get; set; }

        [MaxLength(100)]
        public string District { get; set; }

        [MaxLength(100)]
        public string Ward { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool? IsDefault { get; set; }
    }

    // ============================================
    // CUSTOMER CLASSIFICATION DTO
    // ============================================

    public class UpdateCustomerClassificationDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        public bool IsRegular { get; set; }

        [Required(ErrorMessage = "Payment Privilege is required")]
        public string PaymentPrivilege { get; set; } // prepay, postpay, periodic

        [Range(0, 999999999, ErrorMessage = "Credit Limit must be positive")]
        public decimal CreditLimit { get; set; } = 0;

        public string Notes { get; set; }
    }

    // ============================================
    // CUSTOMER STATISTICS DTO
    // ============================================

    public class CustomerStatisticsDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool IsRegular { get; set; }

        // Order Statistics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public double SuccessRate { get; set; }

        // Financial Statistics
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal OutstandingBalance { get; set; }

        // Activity
        public DateTime? FirstOrderDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int DaysSinceLastOrder { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersLastMonth { get; set; }

        // Loyalty Metrics
        public int LoyaltyPoints { get; set; }
        public string LoyaltyTier { get; set; }
    }

    // ============================================
    // RECENT ORDER DTO (for customer detail)
    // ============================================

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }
        public string OrderStatus { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal CodAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    // ============================================
    // CUSTOMER SEARCH/FILTER DTO
    // ============================================

    public class CustomerSearchDto
    {
        public string SearchTerm { get; set; }
        public bool? IsRegular { get; set; }
        public string PaymentPrivilege { get; set; }
        public decimal? MinRevenue { get; set; }
        public decimal? MaxRevenue { get; set; }
        public int? MinOrders { get; set; }
        public int? MaxOrders { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } // name, totalOrders, totalRevenue, createdAt
        public string SortOrder { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ============================================
    // CUSTOMER ORDER HISTORY DTO
    // ============================================

    public class CustomerOrderHistoryDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<OrderHistoryItemDto> Orders { get; set; }
    }

    public class OrderHistoryItemDto
    {
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }
        public string OrderStatus { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal CodAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public bool HasRating { get; set; }
        public int? RatingScore { get; set; }
    }

    // ============================================
    // CUSTOMER LIST ITEM DTO
    // ============================================

    public class CustomerListItemDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsRegular { get; set; }
        public string PaymentPrivilege { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}