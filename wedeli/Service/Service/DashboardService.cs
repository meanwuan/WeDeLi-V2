using System;
using System.Threading.Tasks;
using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Service.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ITransportCompanyRepository _transportCompanyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IDriverRepository driverRepository,
            IVehicleRepository vehicleRepository,
            ITransportCompanyRepository transportCompanyRepository,
            IMapper mapper,
            ILogger<DashboardService> logger)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _transportCompanyRepository = transportCompanyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetAdminDashboardAsync(int? companyId = null)
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                var customers = await _customerRepository.GetAllAsync();
                var drivers = await _driverRepository.GetAllAsync();

                return new DashboardStatsDto
                {
                    TotalOrders = orders.Count(),
                    PendingOrders = orders.Count(o => o.OrderStatus == "pending"),
                    InTransitOrders = orders.Count(o => o.OrderStatus == "in_transit"),
                    DeliveredOrders = orders.Count(o => o.OrderStatus == "delivered"),
                    TotalRevenue = 0,
                    TodayRevenue = 0,
                    ActiveVehicles = 0,
                    ActiveDrivers = drivers.Count(d => d.IsActive == true),
                    PendingComplaints = 0,
                    PendingCodAmount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard");
                throw;
            }
        }

        public async Task<DashboardStatsDto> GetDriverDashboardAsync(int driverId)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(driverId);
                if (driver == null)
                    throw new KeyNotFoundException($"Driver with ID {driverId} not found.");

                var orders = await _orderRepository.GetAllAsync();
                var driverOrders = orders.Where(o => o.DriverId == driverId).ToList();

                return new DashboardStatsDto
                {
                    TotalOrders = driverOrders.Count,
                    PendingOrders = driverOrders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                    InTransitOrders = driverOrders.Count(o => o.OrderStatus == "in_transit"),
                    DeliveredOrders = driverOrders.Count(o => o.OrderStatus == "delivered"),
                    TotalRevenue = 0,
                    TodayRevenue = 0,
                    ActiveVehicles = 0,
                    ActiveDrivers = 1,
                    PendingComplaints = 0,
                    PendingCodAmount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver dashboard: {DriverId}", driverId);
                throw;
            }
        }

        public async Task<DashboardStatsDto> GetCustomerDashboardAsync(int customerId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

                var orders = await _orderRepository.GetAllAsync();
                var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();

                return new DashboardStatsDto
                {
                    TotalOrders = customerOrders.Count,
                    PendingOrders = customerOrders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                    InTransitOrders = customerOrders.Count(o => o.OrderStatus == "in_transit"),
                    DeliveredOrders = customerOrders.Count(o => o.OrderStatus == "delivered"),
                    TotalRevenue = 0,
                    TodayRevenue = 0,
                    ActiveVehicles = 0,
                    ActiveDrivers = 0,
                    PendingComplaints = 0,
                    PendingCodAmount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer dashboard: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DashboardStatsDto> GetCompanyDashboardAsync(int companyId)
        {
            try
            {
                var company = await _transportCompanyRepository.GetByIdAsync(companyId);
                if (company == null || company.CompanyId == 0)
                    throw new KeyNotFoundException($"Company with ID {companyId} not found.");

                // Get drivers for this company
                var allDrivers = await _driverRepository.GetAllAsync();
                var companyDrivers = allDrivers.Where(d => d.CompanyId == companyId).ToList();
                var companyDriverIds = companyDrivers.Select(d => d.DriverId).ToHashSet();

                // Get vehicles for this company
                var allVehicles = await _vehicleRepository.GetAllAsync();
                var companyVehicles = allVehicles.Where(v => v.CompanyId == companyId).ToList();

                // Get orders assigned to company's drivers
                var allOrders = await _orderRepository.GetAllAsync();
                var companyOrders = allOrders.Where(o => o.DriverId.HasValue && companyDriverIds.Contains(o.DriverId.Value)).ToList();

                // Calculate revenue
                var deliveredOrders = companyOrders.Where(o => o.OrderStatus == "delivered").ToList();
                var totalRevenue = deliveredOrders.Sum(o => o.ShippingFee);
                var today = DateTime.UtcNow.Date;
                var todayRevenue = deliveredOrders
                    .Where(o => o.DeliveredAt.HasValue && o.DeliveredAt.Value.Date == today)
                    .Sum(o => o.ShippingFee);

                // Calculate pending COD (orders with COD that are not yet delivered)
                var pendingCodAmount = companyOrders
                    .Where(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled" && o.CodAmount.GetValueOrDefault() > 0)
                    .Sum(o => o.CodAmount.GetValueOrDefault());

                return new DashboardStatsDto
                {
                    TotalOrders = companyOrders.Count,
                    PendingOrders = companyOrders.Count(o => o.OrderStatus == "pending" || o.OrderStatus == "pending_pickup"),
                    InTransitOrders = companyOrders.Count(o => o.OrderStatus == "in_transit" || o.OrderStatus == "picked_up"),
                    DeliveredOrders = deliveredOrders.Count,
                    TotalRevenue = totalRevenue,
                    TodayRevenue = todayRevenue,
                    ActiveVehicles = companyVehicles.Count(v => v.CurrentStatus == "available" || v.CurrentStatus == "in_transit"),
                    ActiveDrivers = companyDrivers.Count(d => d.IsActive == true),
                    PendingComplaints = 0,
                    PendingCodAmount = pendingCodAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company dashboard: {CompanyId}", companyId);
                throw;
            }
        }
    }
}
