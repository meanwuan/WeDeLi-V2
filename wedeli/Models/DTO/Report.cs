using System;
using System.Collections.Generic;

namespace wedeli.Models.DTO.Report
{
    public class DailyReportSummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int TotalRevenue { get; internal set; }
    }

    public class DriverreportPerformanceDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageRating { get; set; }
        public decimal TotalEarnings { get; set; }
        public DateTime? ReportGeneratedAt { get; set; }
    }

    public class ComplianceReportDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int TotalViolations { get; set; }
        public List<string> ViolationDetails { get; set; } = new List<string>();
        public DateTime ReportDate { get; set; }
    }

    public class AnalyticsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public Dictionary<string, int> MetricsByCategory { get; set; } = new Dictionary<string, int>();
    }

    public class ExportReportDto
    {
        public string ReportName { get; set; }
        public string Format { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string FilePath { get; set; }
        public bool IsReady { get; set; }
    }
}
