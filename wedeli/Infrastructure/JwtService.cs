using System.Security.Claims;
using wedeli.Models.Domain;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace wedeli.Infrastructure
{
    public class JwtService : IJwtService
    {
        /// <summary>
        /// Implementation của JWT Service
        /// Chịu trách nhiệm tạo, validate, và parse JWT tokens
        /// </summary>
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;
        private readonly int _refreshTokenExpiryDays;
        
        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var jwtSettings = _configuration.GetSection("JwtSettings");
            _secretKey = jwtSettings["Secretkey"]?? throw new ArgumentNullException("JWT Secret Key is not configured.");
            _issuer = jwtSettings["Issuer"] ?? "WeDeli";
            _audience = jwtSettings["Audience"] ?? "WeDeli_Users";
            _expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
            _refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");
            if (_secretKey.Length < 32)
            { 
                throw new ArgumentException("JWT Secret Key must be at least 32 characters long for security reasons.");
            }
        }
        public string GenerateAccessToken(User user)
        {
            if(user == null) throw new ArgumentNullException(nameof(user));
            if(user.Role == null) throw new ArgumentNullException("User role is not loaded.");
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID cho token
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), // Issued at
                
                    // Custom claims
                     new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("Username", user.Username), // Username field
                    new Claim(ClaimTypes.Role, user.Role.RoleName),
                    new Claim("RoleId", user.RoleId.ToString()),
                    new Claim("Phone", user.Phone),
                    new Claim("IsActive", user.IsActive.ToString())
                };

                // Kiểm tra user có thuộc company nào không
                var driver = user.Drivers?.FirstOrDefault();
                var warehouseStaff = user.WarehouseStaffs?.FirstOrDefault();
                
                if(!string.IsNullOrEmpty(user.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, user.Email));
                    claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                }
                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                    signingCredentials: credentials
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("Generated access token for user {UserId} ({Username}), role: {Role}, expires in {Minutes} minutes", 
                    user.UserId, user.Username, user.Role.RoleName, _expiryMinutes);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user {UserId}", user.UserId);
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            try
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);
                _logger.LogInformation("Generated new refresh token.");
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token.");
                throw;
            }
        }

        public string? GetEmailFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            return principal.FindFirst("Username")?.Value 
                ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
        }
        public string? GetRoleFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            return principal.FindFirst(ClaimTypes.Role)?.Value;
        }

        public int? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        public bool IsTokenExpired(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return true;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("ValidateToken called with null or empty token.");
                return null;
            }
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Không cho phép sai lệch thời gian
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm.");
                    return null;
                }
                _logger.LogInformation("Token validated successfully.");
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning( "Token has expired.");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Token validation failed due to security token exception.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed.");
                return null;
            }
        }
    }
}
