using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using wedeli.Infrastructure;

namespace wedeli.Models.DTO.Report
{
    public class DailyReportSummaryDto
    {
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime GeneratedAt { get; set; }
        public decimal TotalRevenue { get; set; }
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
        [JsonConverter(typeof(NullableDateOnlyJsonConverter))]
        public DateTime? ReportGeneratedAt { get; set; }
    }

    public class ComplianceReportDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int TotalViolations { get; set; }
        public List<string> ViolationDetails { get; set; } = new List<string>();
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime ReportDate { get; set; }
    }

    public class AnalyticsDto
    {
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime PeriodStart { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
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
        [JsonConverter(typeof(NullableDateOnlyJsonConverter))]
        public DateTime? StartDate { get; set; }
        [JsonConverter(typeof(NullableDateOnlyJsonConverter))]
        public DateTime? EndDate { get; set; }
        public string FilePath { get; set; }
        public bool IsReady { get; set; }
    }

    public class ReportSummaryDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalVehicles { get; set; }
        [JsonConverter(typeof(NullableDateOnlyJsonConverter))]
        public DateTime? StartDate { get; set; }
        [JsonConverter(typeof(NullableDateOnlyJsonConverter))]
        public DateTime? EndDate { get; set; }
    }
}
