using wedeli.Infrastructure;
using wedeli.Repositories.Interface;

namespace wedeli.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService, IUserRepository userRepository)
        {
            // Lấy token từ header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    // Validate token
                    var principal = jwtService.ValidateToken(token);

                    if (principal != null)
                    {
                        // Attach user info vào context
                        context.User = principal;

                        // Lấy userId để kiểm tra user còn active không
                        var userId = principal.GetUserId();

                        if (userId.HasValue)
                        {
                            var user = await userRepository.GetByIdAsync(userId.Value);

                            if (user == null || (bool)!user.IsActive)
                            {
                                _logger.LogWarning("User {UserId} not found or inactive", userId);
                                context.User = new System.Security.Claims.ClaimsPrincipal();
                            }
                            else
                            {
                                // Lưu user object vào Items để dùng trong controllers
                                context.Items["User"] = user;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Token validation failed");
                    // Không throw exception, chỉ log và tiếp tục
                    // [Authorize] attribute sẽ xử lý 401
                }
            }

            await _next(context);
        }
    }
}
