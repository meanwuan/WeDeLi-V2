using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Order service for managing orders
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IDriverRepository driverRepository,
            IVehicleRepository vehicleRepository,
            IOrderStatusHistoryRepository statusHistoryRepository,
            INotificationService notificationService,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        public async Task<OrderDetailDto> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                var orderDto = _mapper.Map<OrderDetailDto>(order);

                // Enrich with customer info from Platform DB
                if (order.CustomerId > 0)
                {
                    try
                    {
                        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
                        if (customer != null)
                        {
                            orderDto.CustomerName = customer.FullName;
                            orderDto.CustomerPhone = customer.Phone;
                            orderDto.IsRegularCustomer = customer.IsRegular ?? false;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        // Customer not found in Platform DB - continue with null values
                        _logger.LogWarning("Customer {CustomerId} not found for order {OrderId}", order.CustomerId, orderId);
                    }
                }
                
                _logger.LogInformation("Retrieved order details: {OrderId}", orderId);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Get order by tracking code
        /// </summary>
        public async Task<OrderResponseDto> GetOrderByTrackingCodeAsync(string trackingCode)
        {
            try
            {
                if (string.IsNullOrEmpty(trackingCode))
                    throw new ArgumentNullException(nameof(trackingCode));

                var order = await _orderRepository.GetByTrackingCodeAsync(trackingCode);
                
                if (order == null)
                    throw new KeyNotFoundException($"Order with tracking code {trackingCode} not found");

                var orderDto = _mapper.Map<OrderResponseDto>(order);
                
                _logger.LogInformation("Retrieved order by tracking code: {TrackingCode}", trackingCode);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by tracking code: {TrackingCode}", trackingCode);
                throw;
            }
        }

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                
                var paginatedOrders = orders
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(paginatedOrders);
                
                _logger.LogInformation("Retrieved all orders - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                throw;
            }
        }

        /// <summary>
        /// Get orders by customer
        /// </summary>
        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(int customerId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                
                if (customer == null)
                    throw new KeyNotFoundException($"Customer {customerId} not found");

                var orders = await _orderRepository.GetByCustomerIdAsync(customerId, pageNumber, pageSize);
                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
                
                _logger.LogInformation("Retrieved orders for customer: {CustomerId}", customerId);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for customer: {CustomerId}", customerId);
                throw;
            }
        }

        /// <summary>
        /// Get orders by driver
        /// </summary>
        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByDriverAsync(int driverId, string? status = null)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(driverId);
                
                if (driver == null)
                    throw new KeyNotFoundException($"Driver {driverId} not found");

                var orders = await _orderRepository.GetByDriverIdAsync(driverId, status ?? "");
                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
                
                _logger.LogInformation("Retrieved orders for driver: {DriverId}", driverId);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    throw new ArgumentNullException(nameof(status));

                var orders = await _orderRepository.GetByStatusAsync(status, companyId);
                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
                
                _logger.LogInformation("Retrieved orders by status: {Status}", status);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get pending orders
        /// </summary>
        public async Task<IEnumerable<OrderResponseDto>> GetPendingOrdersAsync(int? companyId = null)
        {
            try
            {
                var orders = await _orderRepository.GetPendingOrdersAsync(companyId);
                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
                
                _logger.LogInformation("Retrieved pending orders");
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending orders");
                throw;
            }
        }

        /// <summary>
        /// Get orders by company ID with pagination
        /// </summary>
        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var orders = await _orderRepository.GetByCompanyIdAsync(companyId, pageNumber, pageSize);
                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
                
                _logger.LogInformation("Retrieved orders for company: {CompanyId}", companyId);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for company: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Create new order
        /// Supports both existing customers (CustomerId > 0) and walk-in orders (CustomerId = 0)
        /// For walk-in orders, a new customer is auto-created from sender info
        /// </summary>
        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                int customerId = dto.CustomerId;

                // If CustomerId is 0, auto-create a new customer from sender info
                if (customerId <= 0)
                {
                    // Check if customer already exists with this phone
                    Customer? existingCustomer = null;
                    try
                    {
                        existingCustomer = await _customerRepository.GetByPhoneAsync(dto.SenderPhone);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Customer not found by phone - this is expected for new customers
                        existingCustomer = null;
                    }
                    
                    if (existingCustomer != null)
                    {
                        customerId = existingCustomer.CustomerId;
                        _logger.LogInformation("Found existing customer by phone: {Phone}, CustomerId: {CustomerId}", 
                            dto.SenderPhone, customerId);
                    }
                    else
                    {
                        // Create new customer from sender info
                        var newCustomer = new Customer
                        {
                            FullName = dto.SenderName,
                            Phone = dto.SenderPhone,
                            PaymentPrivilege = null, // Let database use default or null
                            IsRegular = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        // CreateCustomerAsync saves to DB and returns entity with generated ID
                        var createdCustomer = await _customerRepository.CreateCustomerAsync(newCustomer);
                        customerId = createdCustomer.CustomerId;
                        _logger.LogInformation("Created new walk-in customer: {CustomerId} ({CustomerName})", 
                            customerId, dto.SenderName);
                    }
                }
                else
                {
                    // Verify existing customer
                    var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
                    if (customer == null)
                        throw new KeyNotFoundException($"Customer {dto.CustomerId} not found");
                }

                var order = _mapper.Map<Order>(dto);
                order.CustomerId = customerId;
                order.TrackingCode = GenerateTrackingCode();
                order.OrderStatus = "pending_pickup";
                order.PaymentStatus = "unpaid";
                order.CreatedAt = DateTime.UtcNow;

                var createdOrder = await _orderRepository.AddAsync(order);
                var orderDto = _mapper.Map<OrderResponseDto>(createdOrder);
                
                // Send notification to admin/company users about new order
                await SendNewOrderNotificationAsync(createdOrder);
                
                _logger.LogInformation("Created new order: {OrderId} ({TrackingCode})", createdOrder.OrderId, createdOrder.TrackingCode);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                throw;
            }
        }

        /// <summary>
        /// Send notification to transport company owner when new order is created
        /// Only sends to the specific company that the order is assigned to via RouteId
        /// </summary>
        private async Task SendNewOrderNotificationAsync(Order order)
        {
            try
            {
                // Only send notification if order has a RouteId (assigned to a company)
                if (order.RouteId == null || order.RouteId <= 0)
                {
                    _logger.LogInformation("Order {OrderId} has no RouteId, skipping notification", order.OrderId);
                    return;
                }

                // Get the Route to find the CompanyId
                var route = order.Route;
                if (route == null)
                {
                    _logger.LogWarning("Route {RouteId} not found for order {OrderId}", order.RouteId, order.OrderId);
                    return;
                }

                // Get the Company to find the owner UserId
                var company = route.Company;
                if (company == null || company.UserId == null)
                {
                    _logger.LogWarning("Company not found or has no owner for Route {RouteId}", order.RouteId);
                    return;
                }

                // Create notification for the company owner
                var notification = new NotificationDto
                {
                    UserId = company.UserId.Value,
                    OrderId = order.OrderId,
                    NotificationType = "order_status",
                    Title = "Đơn hàng mới",
                    Message = $"Đơn hàng mới #{order.TrackingCode} từ {order.SenderName} đến {order.ReceiverName} ({order.ReceiverProvince})",
                    SentVia = "push",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                    
                await _notificationService.CreateNotificationAsync(notification);
                
                _logger.LogInformation("Sent new order notification to company {CompanyName} (UserId: {UserId}) for order {OrderId}", 
                    company.CompanyName, company.UserId, order.OrderId);
            }
            catch (Exception ex)
            {
                // Log but don't fail the order creation if notification fails
                _logger.LogWarning(ex, "Failed to send notification for new order {OrderId}", order.OrderId);
            }
        }

        /// <summary>
        /// Update order
        /// </summary>
        public async Task<OrderResponseDto> UpdateOrderAsync(int orderId, UpdateOrderDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                _mapper.Map(dto, order);
                order.UpdatedAt = DateTime.UtcNow;

                var updatedOrder = await _orderRepository.UpdateAsync(order);
                var orderDto = _mapper.Map<OrderResponseDto>(updatedOrder);
                
                _logger.LogInformation("Updated order: {OrderId}", orderId);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        public async Task<bool> CancelOrderAsync(int orderId, string reason, int? cancelledBy = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                order.OrderStatus = "cancelled";
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
                
                _logger.LogInformation("Cancelled order: {OrderId}, Reason: {Reason}", orderId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Assign driver and vehicle to order
        /// </summary>
        public async Task<bool> AssignDriverAndVehicleAsync(int orderId, int driverId, int vehicleId, int? assignedBy = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                var driver = await _driverRepository.GetByIdAsync(driverId);
                if (driver == null)
                    throw new KeyNotFoundException($"Driver {driverId} not found");

                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle {vehicleId} not found");

                await _orderRepository.AssignDriverAndVehicleAsync(orderId, driverId, vehicleId);
                
                _logger.LogInformation("Assigned driver and vehicle to order - OrderId: {OrderId}, DriverId: {DriverId}, VehicleId: {VehicleId}", 
                    orderId, driverId, vehicleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver and vehicle: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, int? updatedBy = null, string? notes = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                var oldStatus = order.OrderStatus;
                await _orderRepository.UpdateStatusAsync(orderId, newStatus, updatedBy);

                _logger.LogInformation("Updated order status - OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}", 
                    orderId, oldStatus, newStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Confirm pickup
        /// </summary>
        public async Task<bool> ConfirmPickupAsync(int orderId, int? confirmedBy = null)
        {
            return await UpdateOrderStatusAsync(orderId, "picked_up", confirmedBy);
        }

        /// <summary>
        /// Mark order as in transit
        /// </summary>
        public async Task<bool> MarkInTransitAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "in_transit");
        }

        /// <summary>
        /// Mark order as out for delivery
        /// </summary>
        public async Task<bool> MarkOutForDeliveryAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "out_for_delivery");
        }

        /// <summary>
        /// Complete delivery
        /// </summary>
        public async Task<bool> CompleteDeliveryAsync(int orderId, int? deliveredBy = null, string? signature = null)
        {
            return await UpdateOrderStatusAsync(orderId, "delivered", deliveredBy);
        }

        /// <summary>
        /// Mark order as returned
        /// </summary>
        public async Task<bool> MarkAsReturnedAsync(int orderId, string reason)
        {
            return await UpdateOrderStatusAsync(orderId, "returned");
        }

        /// <summary>
        /// Track order by tracking code
        /// </summary>
        public async Task<OrderTrackingDto> TrackOrderAsync(string trackingCode)
        {
            try
            {
                var order = await _orderRepository.GetByTrackingCodeAsync(trackingCode);
                if (order == null)
                    throw new KeyNotFoundException($"Order with tracking code {trackingCode} not found");

                var trackingDto = _mapper.Map<OrderTrackingDto>(order);
                
                _logger.LogInformation("Tracked order: {TrackingCode}", trackingCode);
                return trackingDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking order: {TrackingCode}", trackingCode);
                throw;
            }
        }

        /// <summary>
        /// Get order history
        /// </summary>
        public async Task<IEnumerable<OrderStatusDto>> GetOrderHistoryAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                var statusHistory = order.OrderStatusHistories
                    ?.OrderByDescending(h => h.CreatedAt)
                    .ToList() ?? new List<OrderStatusHistory>();

                var historyDtos = _mapper.Map<IEnumerable<OrderStatusDto>>(statusHistory);
                
                _logger.LogInformation("Retrieved order history: {OrderId}", orderId);
                return historyDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order history: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Search orders
        /// </summary>
        public async Task<IEnumerable<OrderResponseDto>> SearchOrdersAsync(string searchTerm, int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    throw new ArgumentNullException(nameof(searchTerm));

                var orders = await _orderRepository.SearchOrdersAsync(searchTerm, companyId);
                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
                
                _logger.LogInformation("Searched orders: {SearchTerm}", searchTerm);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders: {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Generate unique tracking code
        /// </summary>
        private string GenerateTrackingCode()
        {
            return $"WDL{DateTime.UtcNow.Ticks}{new Random().Next(1000, 9999)}";
        }
    }
}
