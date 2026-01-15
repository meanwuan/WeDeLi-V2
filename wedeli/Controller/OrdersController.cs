using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Common;
using wedeli.Service.Interface;
using wedeli.Models.Response;
using wedeli.Models.DTO.Driver;

namespace wedeli.Controller
{
    /// <summary>
    /// Orders controller for managing orders
    /// </summary>
    [Route("api/v1/orders")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderPhotoService _photoService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            IOrderPhotoService photoService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _photoService = photoService;
            _logger = logger;
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(new ApiResponse<OrderDetailDto>
                {
                    Success = true,
                    Message = "Order retrieved successfully",
                    Data = order
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<OrderDetailDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order: {OrderId}", id);
                return StatusCode(500, new ApiResponse<OrderDetailDto>
                {
                    Success = false,
                    Message = "Error retrieving order"
                });
            }
        }

        /// <summary>
        /// Get order by tracking code
        /// </summary>
        [HttpGet("tracking/{trackingCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetByTrackingCode(string trackingCode)
        {
            try
            {
                var order = await _orderService.GetOrderByTrackingCodeAsync(trackingCode);
                return Ok(new ApiResponse<OrderResponseDto>
                {
                    Success = true,
                    Message = "Order retrieved successfully",
                    Data = order
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found by tracking code: {TrackingCode}", trackingCode);
                return NotFound(new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by tracking code: {TrackingCode}", trackingCode);
                return StatusCode(500, new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving order"
                });
            }
        }

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
                return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                return StatusCode(500, new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders"
                });
            }
        }

        /// <summary>
        /// Get orders by customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetByCustomer(
            int customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerAsync(customerId, pageNumber, pageSize);
                return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer not found: {CustomerId}", customerId);
                return NotFound(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for customer: {CustomerId}", customerId);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders"
                });
            }
        }

        /// <summary>
        /// Get orders by company (transport company)
        /// </summary>
        [HttpGet("company/{companyId}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetByCompany(
            int companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCompanyAsync(companyId, pageNumber, pageSize);
                return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for company: {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders"
                });
            }
        }

        /// <summary>
        /// Get orders by driver
        /// </summary>
        [HttpGet("driver/{driverId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetByDriver(
            int driverId,
            [FromQuery] string? status = null)
        {
            try
            {
                var orders = await _orderService.GetOrdersByDriverAsync(driverId, status ?? "");
                return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Driver not found: {DriverId}", driverId);
                return NotFound(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for driver: {DriverId}", driverId);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders"
                });
            }
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetByStatus(
            string status,
            [FromQuery] int? companyId = null)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status, companyId);
                return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status: {Status}", status);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders"
                });
            }
        }

        /// <summary>
        /// Get pending orders
        /// </summary>
        [HttpGet("pending/list")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetPendingOrders(
            [FromQuery] int? companyId = null)
        {
            try
            {
                var orders = await _orderService.GetPendingOrdersAsync(companyId);
                return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Pending orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending orders");
                return StatusCode(500, new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving pending orders"
                });
            }
        }

        /// <summary>
        /// Search orders
        /// </summary>
        [HttpGet("search")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Customer")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> Search(
            [FromQuery] string searchTerm,
            [FromQuery] int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return BadRequest(new ApiResponse<IEnumerable<OrderResponseDto>>
                    {
                        Success = false,
                        Message = "Search term is required"
                    });
                }

                var orders = await _orderService.SearchOrdersAsync(searchTerm, companyId);
                return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders found",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders: {SearchTerm}", searchTerm);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error searching orders"
                });
            }
        }

        /// <summary>
        /// Create new order
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Customer")]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> CreateOrder(CreateOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<OrderResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var order = await _orderService.CreateOrderAsync(dto);
                
                _logger.LogInformation("Order created successfully: {OrderId}", order.OrderId);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, 
                    new ApiResponse<OrderResponseDto>
                    {
                        Success = true,
                        Message = "Order created successfully",
                        Data = order
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found when creating order");
                return NotFound(new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = "Error creating order"
                });
            }
        }

        /// <summary>
        /// Update order
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateOrder(int id, UpdateOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    _logger.LogWarning("Order update validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new ApiResponse<OrderResponseDto>
                    {
                        Success = false,
                        Message = "Validation failed: " + string.Join("; ", errors)
                    });
                }

                var order = await _orderService.UpdateOrderAsync(id, dto);
                
                _logger.LogInformation("Order updated successfully: {OrderId}", id);
                return Ok(new ApiResponse<OrderResponseDto>
                {
                    Success = true,
                    Message = "Order updated successfully",
                    Data = order
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order: {OrderId}", id);
                return StatusCode(500, new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = "Error updating order"
                });
            }
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Customer")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelOrder(int id, [FromBody] CancelOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var result = await _orderService.CancelOrderAsync(id, dto.CancellationReason);
                
                _logger.LogInformation("Order cancelled successfully: {OrderId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order cancelled successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error cancelling order"
                });
            }
        }

        /// <summary>
        /// Assign driver and vehicle to order
        /// </summary>
        [HttpPost("{id}/assign-driver-vehicle")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignDriverAndVehicle(int id, DriverAssignmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var result = await _orderService.AssignDriverAndVehicleAsync(id, dto.DriverId, dto.CurrentVehicleId);
                
                _logger.LogInformation("Driver and vehicle assigned to order: {OrderId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Driver and vehicle assigned successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver and vehicle: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error assigning driver and vehicle"
                });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin,CompanyAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, UpdateOrderStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var userId = int.TryParse(User.FindFirst("user_id")?.Value, out var uid) ? uid : 0;
                var result = await _orderService.UpdateOrderStatusAsync(id, dto.NewStatus, userId, dto.Notes);
                
                _logger.LogInformation("Order status updated: {OrderId}, NewStatus: {Status}", id, dto.NewStatus);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order status updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating order status"
                });
            }
        }

        /// <summary>
        /// Confirm pickup
        /// </summary>
        [HttpPost("{id}/confirm-pickup")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmPickup(int id)
        {
            try
            {
                var userId = int.TryParse(User.FindFirst("user_id")?.Value, out var uid) ? uid : 0;
                var result = await _orderService.ConfirmPickupAsync(id, userId);
                
                _logger.LogInformation("Pickup confirmed for order: {OrderId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Pickup confirmed successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming pickup: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error confirming pickup"
                });
            }
        }

        /// <summary>
        /// Mark order as in transit
        /// </summary>
        [HttpPost("{id}/in-transit")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkInTransit(int id)
        {
            try
            {
                var result = await _orderService.MarkInTransitAsync(id);
                
                _logger.LogInformation("Order marked as in transit: {OrderId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order marked as in transit successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as in transit: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error marking order as in transit"
                });
            }
        }

        /// <summary>
        /// Mark order as out for delivery
        /// </summary>
        [HttpPost("{id}/out-for-delivery")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkOutForDelivery(int id)
        {
            try
            {
                var result = await _orderService.MarkOutForDeliveryAsync(id);
                
                _logger.LogInformation("Order marked as out for delivery: {OrderId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order marked as out for delivery successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as out for delivery: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error marking order as out for delivery"
                });
            }
        }

        /// <summary>
        /// Complete delivery
        /// </summary>
        [HttpPost("{id}/complete-delivery")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> CompleteDelivery(int id)
        {
            try
            {
                var userId = int.TryParse(User.FindFirst("user_id")?.Value, out var uid) ? uid : 0;
                var result = await _orderService.CompleteDeliveryAsync(id, userId);
                
                _logger.LogInformation("Delivery completed for order: {OrderId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Delivery completed successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing delivery: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error completing delivery"
                });
            }
        }

        /// <summary>
        /// Mark order as returned
        /// </summary>
        [HttpPost("{id}/mark-returned")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkReturned(int id, [FromBody] OrderStatisticsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                
                _logger.LogInformation("Order marked as returned: {OrderId}", id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order marked as returned successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as returned: {OrderId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error marking order as returned"
                });
            }
        }

        /// <summary>
        /// Track order by tracking code
        /// </summary>
        [HttpGet("track/{trackingCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<OrderTrackingDto>>> TrackOrder(string trackingCode)
        {
            try
            {
                var tracking = await _orderService.TrackOrderAsync(trackingCode);
                return Ok(new ApiResponse<OrderTrackingDto>
                {
                    Success = true,
                    Message = "Order tracking info retrieved successfully",
                    Data = tracking
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found by tracking code: {TrackingCode}", trackingCode);
                return NotFound(new ApiResponse<OrderTrackingDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking order: {TrackingCode}", trackingCode);
                return StatusCode(500, new ApiResponse<OrderTrackingDto>
                {
                    Success = false,
                    Message = "Error tracking order"
                });
            }
        }

        /// <summary>
        /// Get order status history
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderStatusDto>>>> GetOrderHistory(int id)
        {
            try
            {
                var history = await _orderService.GetOrderHistoryAsync(id);
                return Ok(new ApiResponse<IEnumerable<OrderStatusDto>>
                {
                    Success = true,
                    Message = "Order history retrieved successfully",
                    Data = history
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", id);
                return NotFound(new ApiResponse<IEnumerable<OrderStatusDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order history: {OrderId}", id);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderStatusDto>>
                {
                    Success = false,
                    Message = "Error retrieving order history"
                });
            }
        }

        /// <summary>
        /// Get order photos
        /// </summary>
        [HttpGet("{orderId}/photos")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderPhotoDto>>>> GetOrderPhotos(int orderId)
        {
            try
            {
                var photos = await _photoService.GetOrderPhotosAsync(orderId);
                return Ok(new ApiResponse<IEnumerable<OrderPhotoDto>>
                {
                    Success = true,
                    Message = "Order photos retrieved successfully",
                    Data = photos
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound(new ApiResponse<IEnumerable<OrderPhotoDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order photos: {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderPhotoDto>>
                {
                    Success = false,
                    Message = "Error retrieving order photos"
                });
            }
        }

        /// <summary>
        /// Get order photos by type
        /// </summary>
        [HttpGet("{orderId}/photos/{photoType}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderPhotoDto>>>> GetPhotosByType(int orderId, string photoType)
        {
            try
            {
                var photos = await _photoService.GetPhotosByTypeAsync(orderId, photoType);
                return Ok(new ApiResponse<IEnumerable<OrderPhotoDto>>
                {
                    Success = true,
                    Message = "Order photos retrieved successfully",
                    Data = photos
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound(new ApiResponse<IEnumerable<OrderPhotoDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photos by type - OrderId: {OrderId}, Type: {PhotoType}", orderId, photoType);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderPhotoDto>>
                {
                    Success = false,
                    Message = "Error retrieving photos"
                });
            }
        }

        /// <summary>
        /// Upload order photo
        /// </summary>
        [HttpPost("{orderId}/photos")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<OrderPhotoDto>>> UploadPhoto(int orderId, UploadOrderPhotoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<OrderPhotoDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                dto.OrderId = orderId;
                dto.UploadedBy = int.TryParse(User.FindFirst("user_id")?.Value, out var uid) ? uid : 0;

                var photo = await _photoService.UploadPhotoAsync(dto);
                
                _logger.LogInformation("Photo uploaded for order: {OrderId}", orderId);
                return CreatedAtAction(nameof(GetOrderPhotos), new { orderId }, 
                    new ApiResponse<OrderPhotoDto>
                    {
                        Success = true,
                        Message = "Photo uploaded successfully",
                        Data = photo
                    });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound(new ApiResponse<OrderPhotoDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for order: {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<OrderPhotoDto>
                {
                    Success = false,
                    Message = "Error uploading photo"
                });
            }
        }

        /// <summary>
        /// Delete order photo
        /// </summary>
        [HttpDelete("photos/{photoId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePhoto(int photoId)
        {
            try
            {
                var result = await _photoService.DeletePhotoAsync(photoId);
                
                _logger.LogInformation("Photo deleted: {PhotoId}", photoId);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Photo deleted successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Photo not found: {PhotoId}", photoId);
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo: {PhotoId}", photoId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting photo"
                });
            }
        }
    }
}
