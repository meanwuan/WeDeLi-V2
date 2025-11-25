using wedeli.Infrastructure;

namespace wedeli.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var user = context.User;

            // Kiểm tra routes cần role cụ thể
            if (path.StartsWith("/api/admin") && !user.IsInRole("admin"))
            {
                _logger.LogWarning("Unauthorized access to admin route by user {UserId}", user.GetUserId());
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Access denied. Admin role required." });
                return;
            }

            if (path.StartsWith("/api/driver") && !user.HasAnyRole("driver", "multi_role"))
            {
                _logger.LogWarning("Unauthorized access to driver route by user {UserId}", user.GetUserId());
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Access denied. Driver role required." });
                return;
            }

            if (path.StartsWith("/api/warehouse") && !user.HasAnyRole("warehouse_staff", "multi_role"))
            {
                _logger.LogWarning("Unauthorized access to warehouse route by user {UserId}", user.GetUserId());
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Access denied. Warehouse staff role required." });
                return;
            }

            await _next(context);
        }
    }
}
