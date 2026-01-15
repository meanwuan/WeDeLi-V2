using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using wedeli.Models.Domain;
using wedeli.Repositories.Interface;

namespace wedeli.Infrastructure
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public JwtService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
        }

        /// <summary>
        /// Generate JWT Access Token for Platform Users (SuperAdmin, Customer)
        /// </summary>
        public string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT Secret is not configured. Check appsettings.json");
            
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
            
            if (user.Role == null)
                throw new InvalidOperationException($"User {user.UserId} Role is null. Ensure .Include(u => u.Role) is used.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("user_id", user.UserId.ToString()),
                new Claim("user_type", "platform"),  // Platform user
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("full_name", user.FullName),
                new Claim("phone", user.Phone),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("role_id", user.RoleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate JWT Access Token for Company Users (CompanyAdmin, Driver, WarehouseStaff)
        /// </summary>
        public string GenerateAccessToken(CompanyUser companyUser, Driver? driver = null, WarehouseStaff? warehouseStaff = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT Secret is not configured. Check appsettings.json");
            
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
            
            if (companyUser.CompanyRole == null)
                throw new InvalidOperationException($"CompanyUser {companyUser.CompanyUserId} CompanyRole is null. Ensure .Include(u => u.CompanyRole) is used.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("company_user_id", companyUser.CompanyUserId.ToString()),
                new Claim("user_type", "company"),  // Company user
                new Claim("company_id", companyUser.CompanyId.ToString()),
                new Claim(ClaimTypes.Name, companyUser.Username),
                new Claim(ClaimTypes.Email, companyUser.Email ?? string.Empty),
                new Claim("full_name", companyUser.FullName),
                new Claim("phone", companyUser.Phone),
                new Claim(ClaimTypes.Role, companyUser.CompanyRole.RoleName),
                new Claim("company_role_id", companyUser.CompanyRoleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add driver_id or warehouse_staff_id if applicable
            if (driver != null)
            {
                claims.Add(new Claim("driver_id", driver.DriverId.ToString()));
            }
            else if (warehouseStaff != null)
            {
                claims.Add(new Claim("warehouse_staff_id", warehouseStaff.StaffId.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate Refresh Token (random string)
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Validate JWT token và trả về ClaimsPrincipal
        /// </summary>
        public ClaimsPrincipal ValidateToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Save Refresh Token vào database
        /// </summary>
        public async Task<RefreshToken> SaveRefreshTokenAsync(int userId, string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"]);

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            return await _refreshTokenRepository.AddAsync(refreshToken);
        }

        /// <summary>
        /// Get Refresh Token from database
        /// </summary>
        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _refreshTokenRepository.GetByTokenAsync(token);
        }

        /// <summary>
        /// Revoke Refresh Token
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(refreshToken);
            }
        }

        /// <summary>
        /// Check if Refresh Token is valid
        /// </summary>
        public async Task<bool> IsRefreshTokenValidAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (refreshToken == null)
                return false;

            if (refreshToken.IsRevoked)
                return false;

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return false;

            return true;
        }

        /// <summary>
        /// Get User ID from JWT token
        /// </summary>
        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null)
                    return null;

                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "user_id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}