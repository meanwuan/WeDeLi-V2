namespace wedeli.Models.DTO
{
    /// <summary>
    /// DTO for creating a new customer
    /// </summary>
    public class CreateCustomerDto
    {
        public int? UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
    }

    /// <summary>
    /// DTO for updating customer information
    /// </summary>
    public class UpdateCustomerDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool? IsRegular { get; set; }
        public string? PaymentPrivilege { get; set; } // prepay, postpay, periodic
        public decimal? CreditLimit { get; set; }
    }

    /// <summary>
    /// Response DTO for customer details
    /// </summary>
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public int? UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public bool IsRegular { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public string PaymentPrivilege { get; set; } = null!;
        public decimal CreditLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed properties
        public string CustomerType => IsRegular ? "Regular" : "Guest";
        public bool HasCreditLimit => CreditLimit > 0;
    }

    /// <summary>
    /// DTO for customer address
    /// </summary>
    public class CustomerAddressDto
    {
        public int AddressId { get; set; }
        public int CustomerId { get; set; }
        public string? AddressLabel { get; set; }
        public string FullAddress { get; set; } = null!;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating customer address
    /// </summary>
    public class CreateCustomerAddressDto
    {
        public int CustomerId { get; set; }
        public string? AddressLabel { get; set; }
        public string FullAddress { get; set; } = null!;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating customer address
    /// </summary>
    public class UpdateCustomerAddressDto
    {
        public string? AddressLabel { get; set; }
        public string? FullAddress { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool? IsDefault { get; set; }
    }

    /// <summary>
    /// DTO for customer statistics
    /// </summary>
    public class CustomerStatisticsDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public bool IsRegular { get; set; }

        // Order statistics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }

        // Financial statistics
        public decimal TotalRevenue { get; set; }
        public decimal TotalShippingFees { get; set; }
        public decimal TotalCodAmount { get; set; }
        public decimal AverageOrderValue { get; set; }

        // Usage statistics
        public int TotalAddresses { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int DaysSinceLastOrder { get; set; }
        public DateTime MemberSince { get; set; }
        public int DaysAsMember { get; set; }
    }

    /// <summary>
    /// DTO for customer order history
    /// </summary>
    public class CustomerOrderHistoryDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public int TotalOrders { get; set; }
        public List<OrderListItem> Orders { get; set; } = new();
    }

    /// <summary>
    /// Filter parameters for customer queries
    /// </summary>
    public class CustomerFilterDto
    {
        public bool? IsRegular { get; set; }
        public string? PaymentPrivilege { get; set; }
        public decimal? MinRevenue { get; set; }
        public int? MinOrders { get; set; }
        public string? SearchTerm { get; set; } // Search by name or phone
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}