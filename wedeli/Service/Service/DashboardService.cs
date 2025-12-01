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
                if (company == null)
                    throw new KeyNotFoundException($"Company with ID {companyId} not found.");

                var drivers = await _driverRepository.GetAllAsync();
                var companyDrivers = drivers.ToList();

                var vehicles = await _vehicleRepository.GetAllAsync();
                var companyVehicles = vehicles.ToList();

                var orders = await _orderRepository.GetAllAsync();
                var companyOrders = orders.ToList();

                return new DashboardStatsDto
                {
                    TotalOrders = companyOrders.Count,
                    PendingOrders = companyOrders.Count(o => o.OrderStatus != "delivered" && o.OrderStatus != "cancelled"),
                    InTransitOrders = companyOrders.Count(o => o.OrderStatus == "in_transit"),
                    DeliveredOrders = companyOrders.Count(o => o.OrderStatus == "delivered"),
                    TotalRevenue = 0,
                    TodayRevenue = 0,
                    ActiveVehicles = companyVehicles.Count(v => v.VehicleId > 0),
                    ActiveDrivers = companyDrivers.Count(d => d.IsActive == true),
                    PendingComplaints = 0,
                    PendingCodAmount = 0
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
