using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wedeli.Models.DTO.Auth;
using wedeli.Models.DTO.Common;
using wedeli.Models.Response;
using wedeli.Service.Interface;

namespace wedeli.Controller
{
    /// <summary>
    /// Users Controller - Handles user management operations
    /// </summary>
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/users/me
        /// </remarks>
        /// <returns>Current user profile</returns>
        /// <response code="200">User profile retrieved successfully</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="404">User not found</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                _logger.LogInformation("Get current user profile");

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in JWT token");
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var user = await _userService.GetCurrentUserAsync(userId);

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "User profile retrieved successfully",
                    Data = user
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting current user: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/users/1
        /// </remarks>
        /// <param name="id">User ID</param>
        /// <returns>User profile</returns>
        /// <response code="200">User retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                _logger.LogInformation("Get user by ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by ID: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/users/username/john_doe
        /// </remarks>
        /// <param name="username">Username</param>
        /// <returns>User profile</returns>
        /// <response code="200">User retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                _logger.LogInformation("Get user by username: {Username}", username);

                var user = await _userService.GetUserByUsernameAsync(username);

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by username: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all users (with pagination)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/users?pageNumber=1&pageSize=20
        /// </remarks>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>List of users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserProfileDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Get all users - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);

                return Ok(new ApiResponse<IEnumerable<UserProfileDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all users: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/users/role/Admin
        /// </remarks>
        /// <param name="roleName">Role name</param>
        /// <returns>List of users with specified role</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Role not found</response>
        [HttpGet("role/{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserProfileDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                _logger.LogInformation("Get users by role: {RoleName}", roleName);

                var users = await _userService.GetUsersByRoleAsync(roleName);

                return Ok(new ApiResponse<IEnumerable<UserProfileDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Role not found: {ex.Message}");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting users by role: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update current user profile
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/v1/users/me
        ///     {
        ///         "fullName": "John Doe Updated",
        ///         "phone": "0123456789",
        ///         "email": "john.updated@example.com"
        ///     }
        /// </remarks>
        /// <param name="updateRequest">Update profile request</param>
        /// <returns>Updated user profile</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpPatch("me")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateProfileRequestDto updateRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in JWT token");
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                _logger.LogInformation("Update current user profile: {UserId}", userId);

                var updatedUser = await _userService.UpdateUserAsync(userId, updateRequest);

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "User profile updated successfully",
                    Data = updatedUser
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user profile: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Search users by keyword
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/users/search?searchTerm=john&roleName=Customer
        /// </remarks>
        /// <param name="searchTerm">Search keyword</param>
        /// <param name="roleName">Optional role filter</param>
        /// <returns>List of matching users</returns>
        /// <response code="200">Search completed successfully</response>
        /// <response code="400">Invalid search term</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserProfileDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm, [FromQuery] string? roleName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Search term is required"
                    });
                }

                _logger.LogInformation("Search users - Term: {SearchTerm}, Role: {RoleName}", searchTerm, roleName ?? "All");

                var users = await _userService.SearchUsersAsync(searchTerm, roleName ?? "");

                return Ok(new ApiResponse<IEnumerable<UserProfileDto>>
                {
                    Success = true,
                    Message = "Search completed successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching users: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Toggle user active/inactive status (Admin only)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/v1/users/{id}/status
        ///     {
        ///         "isActive": false
        ///     }
        /// </remarks>
        /// <param name="id">User ID</param>
        /// <param name="isActive">Active status</param>
        /// <returns>Confirmation message</returns>
        /// <response code="200">User status updated successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - requires admin role</response>
        /// <response code="404">User not found</response>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleUserStatus(int id, [FromBody] ToggleStatusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                _logger.LogInformation("Toggle user status - UserId: {UserId}, IsActive: {IsActive}", id, request.IsActive);

                var result = await _userService.ToggleUserStatusAsync(id, request.IsActive);

                return Ok(new ApiResponse<object?>
                {
                    Success = true,
                    Message = $"User status updated to {(request.IsActive ? "active" : "inactive")}",
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error toggling user status: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/v1/users/1
        /// </remarks>
        /// <param name="id">User ID</param>
        /// <returns>Confirmation message</returns>
        /// <response code="200">User deleted successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - requires admin role</response>
        /// <response code="404">User not found</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                _logger.LogInformation("Delete user: {UserId}", id);

                var result = await _userService.DeleteUserAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(new ApiResponse<object?>
                {
                    Success = true,
                    Message = "User deleted successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
