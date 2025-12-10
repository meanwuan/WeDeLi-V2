using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wedeli.Models.DTO.Company
{
    // ============================================
    // COMPANY DTOs
    // ============================================

    public class CompanyResponseDto
    {
        public int CompanyId { get; set; }
        public int? UserId { get; set; }
        public string CompanyName { get; set; }
        public string BusinessLicense { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public decimal Rating { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCompanyDto
    {
        public int? UserId { get; set; }
        [Required][MaxLength(200)] public string CompanyName { get; set; }
        [Required][MaxLength(50)] public string BusinessLicense { get; set; }
        [Required] public string Address { get; set; }
        [Required][Phone] public string Phone { get; set; }
        [EmailAddress] public string Email { get; set; }
    }

    public class UpdateCompanyDto
    {
        public string CompanyName { get; set; }
        public string BusinessLicense { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CompanyDetailDto : CompanyResponseDto
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int ActiveDrivers { get; set; }
        public int TotalRoutes { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPartners { get; set; }
    }

    public class CompanyStatisticsDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveVehicles { get; set; }
        public int ActiveDrivers { get; set; }
        public double SuccessRate { get; set; }
        public double AverageRating { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}

namespace wedeli.Models.DTO.Partnership
{
    // ============================================
    // PARTNERSHIP DTOs
    // ============================================

    public class PartnershipResponseDto
    {
        public int PartnershipId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int PartnerCompanyId { get; set; }
        public string PartnerCompanyName { get; set; }
        public string PartnershipLevel { get; set; }
        public decimal CommissionRate { get; set; }
        public int PriorityOrder { get; set; }
        public int TotalTransferredOrders { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePartnershipDto
    {
        [Required] public int CompanyId { get; set; }
        [Required] public int PartnerCompanyId { get; set; }
        [Required] public string PartnershipLevel { get; set; }
        [Range(0, 100)] public decimal CommissionRate { get; set; } = 0;
        public int PriorityOrder { get; set; } = 0;
        public string Notes { get; set; }
    }

    public class UpdatePartnershipDto
    {
        public string PartnershipLevel { get; set; }
        public decimal? CommissionRate { get; set; }
        public int? PriorityOrder { get; set; }
        public bool? IsActive { get; set; }
        public string Notes { get; set; }
    }

    public class OrderTransferResponseDto
    {
        public int TransferId { get; set; }
        public int OrderId { get; set; }
        public string TrackingCode { get; set; }
        public int FromCompanyId { get; set; }
        public string FromCompanyName { get; set; }
        public int ToCompanyId { get; set; }
        public string ToCompanyName { get; set; }
        public string TransferReason { get; set; }
        public int? OriginalVehicleId { get; set; }
        public int? NewVehicleId { get; set; }
        public decimal TransferFee { get; set; }
        public decimal CommissionPaid { get; set; }
        public string TransferStatus { get; set; }
        public DateTime TransferredAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    public class TransferOrderDto
    {
        [Required] public int OrderId { get; set; }
        [Required] public int ToCompanyId { get; set; }
        [Required] public string TransferReason { get; set; }
        public decimal? TransferFee { get; set; }
        public string AdminNotes { get; set; }
    }

    public class AcceptTransferDto
    {
        [Required] public int TransferId { get; set; }
        [Required] public int NewVehicleId { get; set; }
        public string Notes { get; set; }
    }

    public class RejectTransferDto
    {
        [Required] public int TransferId { get; set; }
        [Required] public string Reason { get; set; }
    }
}