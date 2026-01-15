using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("order_transfers")]
[Index("FromCompanyId", Name = "from_company_id")]
[Index("NewVehicleId", Name = "new_vehicle_id")]
[Index("OrderId", Name = "order_id")]
[Index("OriginalVehicleId", Name = "original_vehicle_id")]
[Index("ToCompanyId", Name = "to_company_id")]
[Index("TransferredBy", Name = "transferred_by")]
public partial class OrderTransfer
{
    [Key]
    [Column("transfer_id")]
    public int TransferId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("from_company_id")]
    public int FromCompanyId { get; set; }

    [Column("to_company_id")]
    public int ToCompanyId { get; set; }

    [Column("transfer_reason", TypeName = "enum('vehicle_full','route_unavailable','emergency','partnership','other')")]
    public string TransferReason { get; set; } = null!;

    /// <summary>
    /// Xe ban đầu bị đầy
    /// </summary>
    [Column("original_vehicle_id")]
    public int? OriginalVehicleId { get; set; }

    [Column("new_vehicle_id")]
    public int? NewVehicleId { get; set; }

    /// <summary>
    /// Admin thực hiện chuyển
    /// </summary>
    [Column("transferred_by")]
    public int TransferredBy { get; set; }

    /// <summary>
    /// Phí chuyển đổi (nếu có)
    /// </summary>
    [Column("transfer_fee")]
    [Precision(12, 2)]
    public decimal? TransferFee { get; set; }

    /// <summary>
    /// Hoa hồng trả cho đối tác
    /// </summary>
    [Column("commission_paid")]
    [Precision(12, 2)]
    public decimal? CommissionPaid { get; set; }

    [Column("admin_notes", TypeName = "text")]
    public string? AdminNotes { get; set; }

    [Column("transfer_status", TypeName = "enum('pending','accepted','rejected','completed')")]
    public string? TransferStatus { get; set; }

    [Column("transferred_at", TypeName = "timestamp")]
    public DateTime? TransferredAt { get; set; }

    [Column("accepted_at", TypeName = "timestamp")]
    public DateTime? AcceptedAt { get; set; }

    [NotMapped]
    public virtual TransportCompany FromCompany { get; set; } = null!;

    [ForeignKey("NewVehicleId")]
    [InverseProperty("OrderTransferNewVehicles")]
    public virtual Vehicle? NewVehicle { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderTransfers")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("OriginalVehicleId")]
    [InverseProperty("OrderTransferOriginalVehicles")]
    public virtual Vehicle? OriginalVehicle { get; set; }

    [NotMapped]
    public virtual TransportCompany ToCompany { get; set; } = null!;

    [NotMapped]
    public virtual User TransferredByNavigation { get; set; } = null!;
}
