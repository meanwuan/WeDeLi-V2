using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Auth;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// User service interface for user management operations
    /// </summary>
    public interface IUserService
    {
        Task<UserProfileDto> GetUserByIdAsync(int userId);
        Task<UserProfileDto> GetCurrentUserAsync(int currentUserId);
        Task<UserProfileDto> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserProfileDto>> GetUsersByRoleAsync(string roleName);
        Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20);
        Task<UserProfileDto> CreateUserAsync(RegisterRequestDto dto);
        Task<UserProfileDto> UpdateUserAsync(int userId, UpdateProfileRequestDto dto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ToggleUserStatusAsync(int userId, bool isActive);
        Task<IEnumerable<UserProfileDto>> SearchUsersAsync(string searchTerm, string roleName = null);
    }
}
