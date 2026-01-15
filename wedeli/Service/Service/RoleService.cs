using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Role service implementation for role management
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            IMapper mapper,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);

                _logger.LogInformation("Retrieved {Count} roles", roles.Count());
                return roleDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                throw;
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        public async Task<RoleDto> GetRoleByIdAsync(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role not found: {RoleId}", roleId);
                    throw new KeyNotFoundException($"Role {roleId} not found");
                }

                var roleDto = _mapper.Map<RoleDto>(role);
                return roleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by ID: {RoleId}", roleId);
                throw;
            }
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    throw new ArgumentException("Role name is required");

                var role = await _roleRepository.GetByNameAsync(roleName);
                var roleDto = _mapper.Map<RoleDto>(role);

                _logger.LogInformation("Retrieved role: {RoleName}", roleName);
                return roleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
                throw;
            }
        }
    }
}
