using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("transport_companies")]
public partial class TransportCompany
{
    [Key]
    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("company_name")]
    [StringLength(200)]
    public string CompanyName { get; set; } = null!;

    [Column("business_license")]
    [StringLength(50)]
    public string? BusinessLicense { get; set; }

    [Column("address", TypeName = "text")]
    public string? Address { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("rating")]
    [Precision(3, 2)]
    public decimal? Rating { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<CompanyPartnership> CompanyPartnershipCompanies { get; set; } = new List<CompanyPartnership>();

    [InverseProperty("PartnerCompany")]
    public virtual ICollection<CompanyPartnership> CompanyPartnershipPartnerCompanies { get; set; } = new List<CompanyPartnership>();

    [InverseProperty("Company")]
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    [InverseProperty("FromCompany")]
    public virtual ICollection<OrderTransfer> OrderTransferFromCompanies { get; set; } = new List<OrderTransfer>();

    [InverseProperty("ToCompany")]
    public virtual ICollection<OrderTransfer> OrderTransferToCompanies { get; set; } = new List<OrderTransfer>();

    [InverseProperty("Company")]
    public virtual ICollection<PeriodicInvoice> PeriodicInvoices { get; set; } = new List<PeriodicInvoice>();

    [InverseProperty("Company")]
    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();

    [InverseProperty("Company")]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    [InverseProperty("Company")]
    public virtual ICollection<WarehouseStaff> WarehouseStaffs { get; set; } = new List<WarehouseStaff>();
}
