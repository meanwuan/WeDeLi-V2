using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Auth;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// User service implementation for user management
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserProfileDto> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    throw new KeyNotFoundException($"User {userId} not found");
                }

                var userDto = _mapper.Map<UserProfileDto>(user);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get current user
        /// </summary>
        public async Task<UserProfileDto> GetCurrentUserAsync(int currentUserId)
        {
            return await GetUserByIdAsync(currentUserId);
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<UserProfileDto> GetUserByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Username is required");

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found by username: {Username}", username);
                    throw new KeyNotFoundException($"User '{username}' not found");
                }

                var userDto = _mapper.Map<UserProfileDto>(user);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        public async Task<IEnumerable<UserProfileDto>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    throw new ArgumentException("Role name is required");

                // Get role by name
                var role = await _roleRepository.GetByNameAsync(roleName);

                // Get users by role ID
                var users = await _userRepository.GetByRoleAsync(role.RoleId);

                var userDtos = _mapper.Map<IEnumerable<UserProfileDto>>(users);
                _logger.LogInformation("Retrieved {Count} users for role: {RoleName}", users.Count, roleName);

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role: {RoleName}", roleName);
                throw;
            }
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                // Validate pagination parameters
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var users = await _userRepository.GetAllAsync(pageNumber, pageSize);
                var userDtos = _mapper.Map<IEnumerable<UserProfileDto>>(users);

                _logger.LogInformation("Retrieved {Count} users (page {PageNumber}, size {PageSize})",
                    users.Count, pageNumber, pageSize);

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        public async Task<UserProfileDto> CreateUserAsync(RegisterRequestDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Validate input
                if (string.IsNullOrWhiteSpace(dto.Username))
                    throw new ArgumentException("Username is required");

                if (string.IsNullOrWhiteSpace(dto.FullName))
                    throw new ArgumentException("Full name is required");

                if (string.IsNullOrWhiteSpace(dto.Phone))
                    throw new ArgumentException("Phone is required");

                if (string.IsNullOrWhiteSpace(dto.Password))
                    throw new ArgumentException("Password is required");

                // Check if username exists
                if (await _userRepository.UsernameExistsAsync(dto.Username))
                    throw new InvalidOperationException($"Username '{dto.Username}' already exists");

                // Check if phone exists
                if (await _userRepository.PhoneExistsAsync(dto.Phone))
                    throw new InvalidOperationException($"Phone '{dto.Phone}' already exists");

                // Check if email exists (if provided)
                if (!string.IsNullOrEmpty(dto.Email) && await _userRepository.EmailExistsAsync(dto.Email))
                    throw new InvalidOperationException($"Email '{dto.Email}' already exists");

                // Map DTO to entity
                var user = _mapper.Map<User>(dto);

                // Create user in repository
                var createdUser = await _userRepository.CreateAsync(user);

                var userDto = _mapper.Map<UserProfileDto>(createdUser);
                _logger.LogInformation("Created new user: {UserId} ({Username})", createdUser.UserId, createdUser.Username);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", dto?.Username);
                throw;
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        public async Task<UserProfileDto> UpdateUserAsync(int userId, UpdateProfileRequestDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Get existing user
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User {userId} not found");

                // Validate input
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    throw new ArgumentException("Full name is required");

                if (string.IsNullOrWhiteSpace(dto.Phone))
                    throw new ArgumentException("Phone is required");

                // Check phone uniqueness
                if (user.Phone != dto.Phone && await _userRepository.PhoneExistsAsync(dto.Phone))
                    throw new InvalidOperationException($"Phone '{dto.Phone}' already exists");

                // Check email uniqueness
                if (!string.IsNullOrEmpty(dto.Email) &&
                    user.Email != dto.Email &&
                    await _userRepository.EmailExistsAsync(dto.Email))
                    throw new InvalidOperationException($"Email '{dto.Email}' already exists");

                // Update fields
                user.FullName = dto.FullName;
                user.Phone = dto.Phone;
                user.Email = dto.Email;
                user.UpdatedAt = DateTime.UtcNow;

                // Update in repository
                var updatedUser = await _userRepository.UpdateAsync(user);

                var userDto = _mapper.Map<UserProfileDto>(updatedUser);
                _logger.LogInformation("Updated user: {UserId} ({Username})", userId, updatedUser.Username);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var result = await _userRepository.DeleteAsync(userId);
                if (result)
                {
                    _logger.LogInformation("Deleted user: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete user: {UserId}", userId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Toggle user active status
        /// </summary>
        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User {userId} not found");

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Toggled user status - UserId: {UserId}, IsActive: {IsActive}", userId, isActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Search users by keyword
        /// </summary>
        public async Task<IEnumerable<UserProfileDto>> SearchUsersAsync(string searchTerm, string? roleName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<UserProfileDto>();

                // Search for users
                var users = await _userRepository.SearchAsync(searchTerm);

                // Filter by role if specified
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    var role = await _roleRepository.GetByNameAsync(roleName);
                    users = users.Where(u => u.RoleId == role.RoleId).ToList();
                }

                var userDtos = _mapper.Map<IEnumerable<UserProfileDto>>(users);
                _logger.LogInformation("Search found {Count} users for term: {SearchTerm}", users.Count, searchTerm);

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users: {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
}
