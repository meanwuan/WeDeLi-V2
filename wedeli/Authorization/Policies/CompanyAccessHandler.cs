using Microsoft.AspNetCore.Authorization;

namespace wedeli.Authorization.Policies
{
    public class CompanyAccessHandler : AuthorizationHandler<CompanyAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CompanyAccessHandler> _logger;

        public CompanyAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CompanyAccessHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CompanyAccessRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Task.CompletedTask;
            }

            // Admin có quyền truy cập tất cả companies
            if (context.User.IsInRole("admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Lấy companyId từ route
            var routeCompanyId = httpContext.Request.RouteValues[requirement.CompanyIdRouteParam]?.ToString();

            if (string.IsNullOrEmpty(routeCompanyId))
            {
                // Không có companyId trong route → check query string
                routeCompanyId = httpContext.Request.Query["companyId"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(routeCompanyId))
            {
                // Không yêu cầu companyId → cho qua
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Lấy companyId của user từ context (được set bởi service hoặc repository)
            // Note: Cần implement logic lấy companyId từ drivers/warehouse_staff table
            var userCompanyId = httpContext.Items["UserCompanyId"]?.ToString();

            if (userCompanyId == routeCompanyId)
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning(
                    "User attempted to access company {RouteCompanyId} but belongs to {UserCompanyId}",
                    routeCompanyId, userCompanyId);
            }

            return Task.CompletedTask;
        }
    }
}
