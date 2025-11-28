namespace wedeli.Models.DTO
{
    /// <summary>
    /// DTO for creating partnership
    /// </summary>
    public class CreatePartnershipDto
    {
        public int CompanyId { get; set; }
        public int PartnerCompanyId { get; set; }
        public string PartnershipLevel { get; set; } = "regular"; // preferred, regular, backup
        public decimal CommissionRate { get; set; } = 0;
        public int PriorityOrder { get; set; } = 0;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating partnership
    /// </summary>
    public class UpdatePartnershipDto
    {
        public string? PartnershipLevel { get; set; }
        public decimal? CommissionRate { get; set; }
        public int? PriorityOrder { get; set; }
        public bool? IsActive { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response DTO for partnership details
    /// </summary>
    public class PartnershipDto
    {
        public int PartnershipId { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int PartnerCompanyId { get; set; }
        public string? PartnerCompanyName { get; set; }
        public string PartnershipLevel { get; set; } = null!;
        public decimal CommissionRate { get; set; }
        public int PriorityOrder { get; set; }
        public int TotalTransferredOrders { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for partnership statistics
    /// </summary>
    public class PartnershipStatisticsDto
    {
        public int PartnershipId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string PartnerCompanyName { get; set; } = null!;
        public int TotalOrdersTransferred { get; set; }
        public int OrdersCompleted { get; set; }
        public int OrdersCancelled { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public decimal AverageCommission { get; set; }
        public DateTime? LastTransferDate { get; set; }
        public int DaysSinceLastTransfer { get; set; }
    }

    /// <summary>
    /// Filter parameters for partnership queries
    /// </summary>
    public class PartnershipFilterDto
    {
        public int? CompanyId { get; set; }
        public int? PartnerCompanyId { get; set; }
        public string? PartnershipLevel { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for order transfer
    /// </summary>
    public class OrderTransferDto
    {
        public int TransferId { get; set; }
        public int OrderId { get; set; }
        public string? TrackingCode { get; set; }
        public int FromCompanyId { get; set; }
        public string? FromCompanyName { get; set; }
        public int ToCompanyId { get; set; }
        public string? ToCompanyName { get; set; }
        public string TransferReason { get; set; } = null!;
        public int? OriginalVehicleId { get; set; }
        public string? OriginalVehiclePlate { get; set; }
        public int? NewVehicleId { get; set; }
        public string? NewVehiclePlate { get; set; }
        public int TransferredBy { get; set; }
        public string? TransferredByName { get; set; }
        public decimal TransferFee { get; set; }
        public decimal CommissionPaid { get; set; }
        public string? AdminNotes { get; set; }
        public string TransferStatus { get; set; } = null!;
        public DateTime TransferredAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating order transfer
    /// </summary>
    public class CreateOrderTransferDto
    {
        public int OrderId { get; set; }
        public int ToCompanyId { get; set; }
        public string TransferReason { get; set; } = null!; // vehicle_full, route_unavailable, emergency, partnership, other
        public int? OriginalVehicleId { get; set; }
        public decimal TransferFee { get; set; } = 0;
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// DTO for order transfer response
    /// </summary>
    public class OrderTransferResponseDto
    {
        public int TransferId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; } = null!;
        public string FromCompanyName { get; set; } = null!;
        public string ToCompanyName { get; set; } = null!;
        public string TransferReason { get; set; } = null!;
        public decimal CommissionPaid { get; set; }
        public string TransferStatus { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}