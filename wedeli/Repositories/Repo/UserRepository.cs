using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository( AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting users");
                throw;
            }
        }

        public async Task<int> CountByRoleAsync(int roleId)
        {
            try
            {
                return await _context.Users
                    .CountAsync(u => u.RoleId == roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting users by role: {RoleId}", roleId);
                throw;
            }
           
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(user.Username))
                    throw new ArgumentException("Username is required");

                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                    throw new ArgumentException("Password hash is required");

                if (string.IsNullOrWhiteSpace(user.FullName))
                    throw new ArgumentException("Full name is required");

                if (string.IsNullOrWhiteSpace(user.Phone))
                    throw new ArgumentException("Phone is required");

                // Check duplicates
                if (await UsernameExistsAsync(user.Username))
                    throw new InvalidOperationException($"Username '{user.Username}' already exists");

                if (await PhoneExistsAsync(user.Phone))
                    throw new InvalidOperationException($"Phone '{user.Phone}' already exists");

                if (!string.IsNullOrEmpty(user.Email) && await EmailExistsAsync(user.Email))
                    throw new InvalidOperationException($"Email '{user.Email}' already exists");

                // Set timestamps
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new user: {UserId} ({Username})", user.UserId, user.Username);

                // Reload with Role
                return await GetByIdAsync(user.UserId)
                       ?? throw new InvalidOperationException("Failed to retrieve created user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", user?.Username);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent user: {UserId}", userId);
                    return false;
                }

                // Soft delete
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted user: {UserId} ({Username})", userId, user.Username);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                return await _context.Users
                    .AnyAsync(u => u.Email != null && u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                throw;
            }
        }

        public async Task<List<User>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var users = await _context.Users
                    .Include(u => u.Role)
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} users (page {Page}, pageSize {PageSize})",
                    users.Count, page, pageSize);

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<List<User>> GetByActiveStatusAsync(bool isActive)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.IsActive == isActive)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by active status: {IsActive}", isActive);
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                return await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(U => U.UserId == userId);
                if (user != null)
                {
                    _logger.LogDebug("Found user {UserID} {Username}", userId, user.Username);
                }
                else
                {
                    _logger.LogWarning("User not found {UserID}", userId);
                }
                return user;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error retrieving user {UserID}", userId);
                throw;
            }
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                {
                    return null;
                }
                return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Phone == phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by phone {Phone}", phone);
                throw;
            }
        }

        public async Task<List<User>> GetByRoleAsync(int roleId)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.RoleId == roleId)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role: {RoleId}", roleId);
                throw;
            }
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("GetByUsernameAsync called with null or empty username");
                    return null;
                }
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    _logger.LogDebug("Found user by user name {Username}", username);
                }
                return user;
            }
            catch (Exception)
            {
                _logger.LogError("Error retrieving user by username {Username}", username);
                throw;
            }
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                    return false;

                return await _context.Users
                    .AnyAsync(u => u.Phone == phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking phone existence: {Phone}", phone);
                throw;
            }
        }

        public async Task<List<User>> SearchAsync(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return new List<User>();

                keyword = keyword.ToLower();

                return await _context.Users
                    .Include(u => u.Role)
                    .Where(u =>
                        u.Username.ToLower().Contains(keyword) ||
                        u.FullName.ToLower().Contains(keyword) ||
                        u.Phone.Contains(keyword) ||
                        (u.Email != null && u.Email.ToLower().Contains(keyword))
                    )
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with keyword: {Keyword}", keyword);
                throw;
            }
        }

        public async Task<User> UpdateAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                if (existingUser == null)
                    throw new KeyNotFoundException($"User {user.UserId} not found");

                // Check username duplicate (nếu thay đổi)
                if (existingUser.Username != user.Username && await UsernameExistsAsync(user.Username))
                    throw new InvalidOperationException($"Username '{user.Username}' already exists");

                // Check phone duplicate (nếu thay đổi)
                if (existingUser.Phone != user.Phone && await PhoneExistsAsync(user.Phone))
                    throw new InvalidOperationException($"Phone '{user.Phone}' already exists");

                // Check email duplicate (nếu thay đổi)
                if (existingUser.Email != user.Email &&
                    !string.IsNullOrEmpty(user.Email) &&
                    await EmailExistsAsync(user.Email))
                    throw new InvalidOperationException($"Email '{user.Email}' already exists");

                // Update fields
                existingUser.Username = user.Username;
                existingUser.FullName = user.FullName;
                existingUser.Phone = user.Phone;
                existingUser.Email = user.Email;
                existingUser.RoleId = user.RoleId;
                existingUser.IsActive = user.IsActive;

                // Chỉ update password nếu có thay đổi
                if (!string.IsNullOrEmpty(user.PasswordHash))
                    existingUser.PasswordHash = user.PasswordHash;

                existingUser.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated user: {UserId} ({Username})", user.UserId, user.Username);

                return await GetByIdAsync(user.UserId)
                       ?? throw new InvalidOperationException("Failed to retrieve updated user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user?.UserId);
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return false;

                return await _context.Users
                    .AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username existence: {Username}", username);
                throw;
            }
        }
    }
}