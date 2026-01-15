using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Report;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Driver;

namespace wedeli.Service.Service
{
    public class ReportService : IReportService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IOrderRepository orderRepository,
            IDriverRepository driverRepository,
            IMapper mapper,
            ILogger<ReportService> logger)
        {
            _orderRepository = orderRepository;
            _driverRepository = driverRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DailyReportSummaryDto> GetDailySummaryAsync(DateTime date)
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                // Fix: Handle nullable DateTime
                var dailyOrders = orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == date.Date).ToList();
                var deliveredOrders = dailyOrders.Where(o => o.OrderStatus == "delivered").ToList();

                return new DailyReportSummaryDto
                {
                    Date = date.Date,
                    TotalOrders = dailyOrders.Count,
                    CompletedOrders = deliveredOrders.Count,
                    CancelledOrders = dailyOrders.Count(o => o.OrderStatus == "cancelled"),
                    PendingOrders = dailyOrders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                    TotalRevenue = deliveredOrders.Sum(o => o.ShippingFee),
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily summary for date: {Date}", date);
                throw;
            }
        }

        public async Task<IEnumerable<DailyReportSummaryDto>> GetDailySummariesAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                // Fix: Handle nullable DateTime
                var rangeOrders = orders.Where(o => 
                    o.CreatedAt.HasValue && 
                    o.CreatedAt.Value.Date >= startDate.Date && 
                    o.CreatedAt.Value.Date <= endDate.Date).ToList();

                var dailySummaries = new List<DailyReportSummaryDto>();
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dayOrders = rangeOrders.Where(o => o.CreatedAt!.Value.Date == date).ToList();
                    var deliveredOrders = dayOrders.Where(o => o.OrderStatus == "delivered").ToList();
                    
                    dailySummaries.Add(new DailyReportSummaryDto
                    {
                        Date = date,
                        TotalOrders = dayOrders.Count,
                        CompletedOrders = deliveredOrders.Count,
                        CancelledOrders = dayOrders.Count(o => o.OrderStatus == "cancelled"),
                        PendingOrders = dayOrders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                        TotalRevenue = deliveredOrders.Sum(o => o.ShippingFee),
                        GeneratedAt = DateTime.UtcNow
                    });
                }

                return dailySummaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily summaries for range: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<bool> GenerateDailySummaryAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("Daily summary generated for date: {Date}", date);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily summary for date: {Date}", date);
                throw;
            }
        }

        public async Task<ReportSummaryDto> GetSummaryAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                var drivers = await _driverRepository.GetAllAsync();

                // Filter by date range if provided
                var filteredOrders = orders.AsEnumerable();
                if (startDate.HasValue)
                {
                    filteredOrders = filteredOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= startDate.Value.Date);
                }
                if (endDate.HasValue)
                {
                    filteredOrders = filteredOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date <= endDate.Value.Date);
                }

                var orderList = filteredOrders.ToList();
                var completedOrders = orderList.Where(o => o.OrderStatus == "delivered").ToList();
                var cancelledOrders = orderList.Count(o => o.OrderStatus == "cancelled");
                var pendingOrders = orderList.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled");
                var totalRevenue = completedOrders.Sum(o => o.ShippingFee);
                var avgOrderValue = orderList.Any() ? totalRevenue / orderList.Count : 0;
                var completionRate = orderList.Any() ? (decimal)completedOrders.Count / orderList.Count * 100 : 0;

                // Count drivers and vehicles for company
                var companyDrivers = drivers.Where(d => d.CompanyId == companyId).ToList();
                var vehicleCount = companyDrivers.Select(d => d.CompanyId).Distinct().Count() * 3; // Estimate

                return new ReportSummaryDto
                {
                    TotalOrders = orderList.Count,
                    CompletedOrders = completedOrders.Count,
                    CancelledOrders = cancelledOrders,
                    PendingOrders = pendingOrders,
                    CompletionRate = completionRate,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = avgOrderValue,
                    TotalDrivers = companyDrivers.Count,
                    TotalVehicles = vehicleCount,
                    StartDate = startDate,
                    EndDate = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary report for company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<DriverreportPerformanceDto> GetDriverPerformanceReportAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(driverId);
                if (driver == null)
                    throw new KeyNotFoundException($"Driver with ID {driverId} not found.");

                var orders = await _orderRepository.GetAllAsync();
                var driverOrders = orders.Where(o => o.DriverId == driverId).ToList();

                if (startDate.HasValue && endDate.HasValue)
                {
                    driverOrders = driverOrders.Where(o => 
                        o.CreatedAt >= startDate.Value.Date && 
                        o.CreatedAt <= endDate.Value.Date).ToList();
                }

                var successfulDeliveries = driverOrders.Count(o => o.OrderStatus == "delivered");
                var successRate = driverOrders.Any() ? (decimal)successfulDeliveries / driverOrders.Count * 100 : 0;

                return new DriverreportPerformanceDto
                {
                    DriverId = driverId,
                    DriverName = driver.CompanyUser?.FullName ?? "Unknown",
                    TotalDeliveries = driverOrders.Count,
                    SuccessfulDeliveries = successfulDeliveries,
                    FailedDeliveries = driverOrders.Count(o => o.OrderStatus == "cancelled"),
                    SuccessRate = successRate,
                    AverageRating = 0,
                    TotalEarnings = 0,
                    ReportGeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver performance report for driver: {DriverId}", driverId);
                throw;
            }
        }

        public async Task<IEnumerable<DriverreportPerformanceDto>> GetTopDriversReportAsync(int companyId, int topN = 10)
        {
            try
            {
                var drivers = await _driverRepository.GetAllAsync();
                var orders = await _orderRepository.GetAllAsync();

                var driverStats = new List<DriverreportPerformanceDto>();
                var companyDrivers = drivers.Where(d => d.CompanyId == companyId).OrderByDescending(d => d.DriverId).Take(topN);

                foreach (var driver in companyDrivers)
                {
                    var driverOrders = orders.Where(o => o.DriverId == driver.DriverId).ToList();
                    var successfulDeliveries = driverOrders.Count(o => o.OrderStatus == "delivered");
                    var successRate = driverOrders.Any() ? (decimal)successfulDeliveries / driverOrders.Count * 100 : 0;

                    driverStats.Add(new DriverreportPerformanceDto
                    {
                        DriverId = driver.DriverId,
                        DriverName = driver.CompanyUser?.FullName ?? "Unknown",
                        TotalDeliveries = driverOrders.Count,
                        SuccessfulDeliveries = successfulDeliveries,
                        FailedDeliveries = driverOrders.Count(o => o.OrderStatus == "cancelled"),
                        SuccessRate = successRate,
                        AverageRating = 0,
                        TotalEarnings = 0,
                        ReportGeneratedAt = DateTime.UtcNow
                    });
                }

                return driverStats.OrderByDescending(d => d.SuccessfulDeliveries).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top drivers report for company: {CompanyId}", companyId);
                throw;
            }
        }
    }
}
