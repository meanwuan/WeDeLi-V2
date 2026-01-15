using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO.Customer;
using wedeli.Service.Interface;

namespace wedeli.Controllers
{
    [Route("api/v1/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        // ===== Customer Management =====

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerById(int customerId)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while retrieving the customer" });
            }
        }

        /// <summary>
        /// Get detailed customer information including addresses and orders
        /// </summary>
        [HttpGet("{customerId}/detail")]
        [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerDetail(int customerId)
        {
            try
            {
                var detail = await _customerService.GetCustomerDetailAsync(customerId);
                return Ok(detail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer detail: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while retrieving customer details" });
            }
        }

        /// <summary>
        /// Get customer by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerByUserId(int userId)
        {
            try
            {
                _logger.LogInformation("GetCustomerByUserId called with userId: {UserId}", userId);
                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                _logger.LogInformation("Customer found: {CustomerId}", customer?.CustomerId);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("KeyNotFoundException: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by user ID: {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving the customer" });
            }
        }

        /// <summary>
        /// Get customer by phone number
        /// </summary>
        [HttpGet("phone/{phone}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerByPhone(string phone)
        {
            try
            {
                var customer = await _customerService.GetCustomerByPhoneAsync(phone);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by phone: {Phone}", phone);
                return StatusCode(500, new { message = "An error occurred while retrieving the customer" });
            }
        }

        /// <summary>
        /// Get all customers with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CustomerListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync(pageNumber, pageSize);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                return StatusCode(500, new { message = "An error occurred while retrieving customers" });
            }
        }

        /// <summary>
        /// Get all regular customers
        /// </summary>
        [HttpGet("regular")]
        [ProducesResponseType(typeof(IEnumerable<CustomerResponseDto>), StatusCodes.Status200OK)]
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
                return StatusCode(500, new { message = "An error occurred while retrieving regular customers" });
            }
        }

        /// <summary>
        /// Search customers with filters
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<CustomerListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCustomers([FromBody] CustomerSearchDto searchDto)
        {
            try
            {
                var customers = await _customerService.SearchCustomersAsync(searchDto);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers");
                return StatusCode(500, new { message = "An error occurred while searching customers" });
            }
        }

        /// <summary>
        /// Create new customer
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var customer = await _customerService.CreateCustomerAsync(dto);
                return CreatedAtAction(nameof(GetCustomerById), new { customerId = customer.CustomerId }, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, new { message = "An error occurred while creating the customer" });
            }
        }

        /// <summary>
        /// Update customer information
        /// </summary>
        [HttpPut("{customerId}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCustomer(int customerId, [FromBody] UpdateCustomerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var customer = await _customerService.UpdateCustomerAsync(customerId, dto);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while updating the customer" });
            }
        }

        /// <summary>
        /// Toggle customer regular status
        /// </summary>
        [HttpPatch("{customerId}/regular-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRegularStatus(int customerId)
        {
            try
            {
                var result = await _customerService.UpdateRegularStatusAsync(customerId);
                return Ok(new { message = "Regular status updated successfully", success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regular status: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while updating regular status" });
            }
        }

        /// <summary>
        /// Update customer payment privilege
        /// </summary>
        [HttpPatch("{customerId}/payment-privilege")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePaymentPrivilege(int customerId, [FromBody] UpdatePaymentPrivilegeDto dto)
        {
            try
            {
                var result = await _customerService.UpdatePaymentPrivilegeAsync(customerId, dto.Privilege);
                return Ok(new { message = "Payment privilege updated successfully", success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment privilege: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while updating payment privilege" });
            }
        }

        /// <summary>
        /// Get customer statistics
        /// </summary>
        [HttpGet("{customerId}/statistics")]
        [ProducesResponseType(typeof(CustomerStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerStatistics(int customerId)
        {
            try
            {
                var statistics = await _customerService.GetCustomerStatisticsAsync(customerId);
                return Ok(statistics);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        // ===== Address Management =====

        /// <summary>
        /// Get all addresses for a customer
        /// </summary>
        [HttpGet("{customerId}/addresses")]
        [ProducesResponseType(typeof(IEnumerable<CustomerAddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerAddresses(int customerId)
        {
            try
            {
                var addresses = await _customerService.GetCustomerAddressesAsync(customerId);
                return Ok(addresses);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer addresses: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while retrieving addresses" });
            }
        }

        /// <summary>
        /// Get default address for a customer
        /// </summary>
        [HttpGet("{customerId}/addresses/default")]
        [ProducesResponseType(typeof(CustomerAddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDefaultAddress(int customerId)
        {
            try
            {
                var address = await _customerService.GetDefaultAddressAsync(customerId);
                return Ok(address);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while retrieving default address" });
            }
        }

        /// <summary>
        /// Add new address for customer
        /// </summary>
        [HttpPost("{customerId}/addresses")]
        [ProducesResponseType(typeof(CustomerAddressDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAddress(int customerId, [FromBody] CreateCustomerAddressDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var address = await _customerService.AddAddressAsync(customerId, dto);
                return CreatedAtAction(nameof(GetCustomerAddresses), new { customerId }, address);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address: {CustomerId}", customerId);
                return StatusCode(500, new { message = "An error occurred while adding the address" });
            }
        }

        /// <summary>
        /// Update address
        /// </summary>
        [HttpPut("addresses/{addressId}")]
        [ProducesResponseType(typeof(CustomerAddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UpdateCustomerAddressDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var address = await _customerService.UpdateAddressAsync(addressId, dto);
                return Ok(address);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address: {AddressId}", addressId);
                return StatusCode(500, new { message = "An error occurred while updating the address" });
            }
        }

        /// <summary>
        /// Delete address
        /// </summary>
        [HttpDelete("addresses/{addressId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            try
            {
                var result = await _customerService.DeleteAddressAsync(addressId);
                return Ok(new { message = "Address deleted successfully", success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address: {AddressId}", addressId);
                return StatusCode(500, new { message = "An error occurred while deleting the address" });
            }
        }

        /// <summary>
        /// Set default address for customer
        /// </summary>
        [HttpPatch("{customerId}/addresses/{addressId}/set-default")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetDefaultAddress(int customerId, int addressId)
        {
            try
            {
                var result = await _customerService.SetDefaultAddressAsync(customerId, addressId);
                return Ok(new { message = "Default address set successfully", success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address: {CustomerId}, {AddressId}", customerId, addressId);
                return StatusCode(500, new { message = "An error occurred while setting default address" });
            }
        }
    }

    // ===== Supporting DTOs =====
    public class UpdatePaymentPrivilegeDto
    {
        public string Privilege { get; set; } = string.Empty;
    }
}