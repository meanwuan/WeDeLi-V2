using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO;
using wedeli.service.Interface;

namespace wedeli.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        // ===== Customer CRUD Operations =====

        /// <summary>
        /// Get all customers with filtering and pagination
        /// GET /api/customers
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetCustomers([FromQuery] CustomerFilterDto filter)
        {
            try
            {
                var (customers, totalCount) = await _customerService.GetCustomersAsync(filter);

                return Ok(new
                {
                    data = customers,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get customer by ID
        /// GET /api/customers/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                // Allow customer to see their own profile
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                    if (customer != null && customer.CustomerId != id &&
                        !User.IsInRole("admin") && !User.IsInRole("warehouse_staff"))
                    {
                        return Forbid();
                    }
                }

                var result = await _customerService.GetCustomerByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Customer not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get customer by phone
        /// GET /api/customers/phone/{phone}
        /// </summary>
        [HttpGet("phone/{phone}")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetCustomerByPhone(string phone)
        {
            try
            {
                var customer = await _customerService.GetCustomerByPhoneAsync(phone);
                if (customer == null)
                    return NotFound(new { message = "Customer not found" });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer by phone: {phone}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new customer
        /// POST /api/customers
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(dto);
                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update customer information
        /// PUT /api/customers/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
        {
            try
            {
                // Allow customer to update their own profile
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                    if (customer != null && customer.CustomerId != id &&
                        !User.IsInRole("admin") && !User.IsInRole("warehouse_staff"))
                    {
                        return Forbid();
                    }
                }

                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, dto);
                return Ok(updatedCustomer);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Customer not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete customer
        /// DELETE /api/customers/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var deleted = await _customerService.DeleteCustomerAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Customer not found" });

                return Ok(new { message = "Customer deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer: {id}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Address Management =====

        /// <summary>
        /// Get all addresses for a customer
        /// GET /api/customers/{customerId}/addresses
        /// </summary>
        [HttpGet("{customerId}/addresses")]
        public async Task<IActionResult> GetCustomerAddresses(int customerId)
        {
            try
            {
                // Allow customer to see their own addresses
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                    if (customer != null && customer.CustomerId != customerId &&
                        !User.IsInRole("admin") && !User.IsInRole("warehouse_staff"))
                    {
                        return Forbid();
                    }
                }

                var addresses = await _customerService.GetCustomerAddressesAsync(customerId);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting addresses for customer: {customerId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get default address for a customer
        /// GET /api/customers/{customerId}/addresses/default
        /// </summary>
        [HttpGet("{customerId}/addresses/default")]
        public async Task<IActionResult> GetDefaultAddress(int customerId)
        {
            try
            {
                var address = await _customerService.GetDefaultAddressAsync(customerId);
                if (address == null)
                    return NotFound(new { message = "No default address found" });

                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting default address for customer: {customerId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create new address for customer
        /// POST /api/customers/addresses
        /// </summary>
        [HttpPost("addresses")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateCustomerAddressDto dto)
        {
            try
            {
                // Allow customer to create their own address
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                    if (customer != null && customer.CustomerId != dto.CustomerId &&
                        !User.IsInRole("admin") && !User.IsInRole("warehouse_staff"))
                    {
                        return Forbid();
                    }
                }

                var address = await _customerService.CreateAddressAsync(dto);
                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update customer address
        /// PUT /api/customers/addresses/{addressId}
        /// </summary>
        [HttpPut("addresses/{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UpdateCustomerAddressDto dto)
        {
            try
            {
                var address = await _customerService.UpdateAddressAsync(addressId, dto);
                return Ok(address);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Address not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating address: {addressId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete customer address
        /// DELETE /api/customers/addresses/{addressId}
        /// </summary>
        [HttpDelete("addresses/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            try
            {
                var deleted = await _customerService.DeleteAddressAsync(addressId);
                if (!deleted)
                    return NotFound(new { message = "Address not found" });

                return Ok(new { message = "Address deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting address: {addressId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Set default address for customer
        /// PUT /api/customers/{customerId}/addresses/{addressId}/set-default
        /// </summary>
        [HttpPut("{customerId}/addresses/{addressId}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int customerId, int addressId)
        {
            try
            {
                var success = await _customerService.SetDefaultAddressAsync(customerId, addressId);
                if (!success)
                    return NotFound(new { message = "Address not found" });

                return Ok(new { message = "Default address updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting default address: {addressId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Order History =====

        /// <summary>
        /// Get customer order history
        /// GET /api/customers/{customerId}/orders?status=delivered
        /// </summary>
        [HttpGet("{customerId}/orders")]
        public async Task<IActionResult> GetCustomerOrderHistory(int customerId, [FromQuery] string? status = null)
        {
            try
            {
                // Allow customer to see their own orders
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                    if (customer != null && customer.CustomerId != customerId &&
                        !User.IsInRole("admin") && !User.IsInRole("warehouse_staff"))
                    {
                        return Forbid();
                    }
                }

                var history = await _customerService.GetCustomerOrderHistoryAsync(customerId, status);
                return Ok(history);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Customer not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order history for customer: {customerId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Statistics =====

        /// <summary>
        /// Get customer statistics
        /// GET /api/customers/{customerId}/statistics
        /// </summary>
        [HttpGet("{customerId}/statistics")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetCustomerStatistics(int customerId)
        {
            try
            {
                var stats = await _customerService.GetCustomerStatisticsAsync(customerId);
                return Ok(stats);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Customer not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer statistics: {customerId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Customer Status (Regular/VIP) =====

        /// <summary>
        /// Get all regular customers
        /// GET /api/customers/regular
        /// </summary>
        [HttpGet("regular")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetRegularCustomers()
        {
            try
            {
                var customers = await _customerService.GetRegularCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regular customers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get VIP customers
        /// GET /api/customers/vip?minOrders=10&minRevenue=10000000
        /// </summary>
        [HttpGet("vip")]
        [Authorize(Roles = "admin,warehouse_staff")]
        public async Task<IActionResult> GetVipCustomers([FromQuery] int minOrders = 10, [FromQuery] decimal minRevenue = 10000000)
        {
            try
            {
                var customers = await _customerService.GetVipCustomersAsync(minOrders, minRevenue);
                return Ok(new
                {
                    criteria = new { minOrders, minRevenue },
                    count = customers.Count,
                    customers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VIP customers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== Current Customer Info (Self-Service) =====

        /// <summary>
        /// Get current logged-in customer profile
        /// GET /api/customers/me
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                    return NotFound(new { message = "Customer profile not found" });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current customer profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current customer's addresses
        /// GET /api/customers/me/addresses
        /// </summary>
        [HttpGet("me/addresses")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GetMyAddresses()
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                    return NotFound(new { message = "Customer profile not found" });

                var addresses = await _customerService.GetCustomerAddressesAsync(customer.CustomerId);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current customer addresses");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current customer's order history
        /// GET /api/customers/me/orders?status=delivered
        /// </summary>
        [HttpGet("me/orders")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GetMyOrders([FromQuery] string? status = null)
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                    return NotFound(new { message = "Customer profile not found" });

                var history = await _customerService.GetCustomerOrderHistoryAsync(customer.CustomerId, status);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current customer orders");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}