using Microsoft.AspNetCore.Authorization;
namespace wedeli.Authorization.Requirements
{
    public class ActiveUserRequirement : IAuthorizationRequirement
    {
        public class ActiveUserRequirementHandler : AuthorizationHandler<ActiveUserRequirement>
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public ActiveUserRequirementHandler(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            protected override Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ActiveUserRequirement requirement)
            {
                // Lấy user từ context.Items (được set bởi JwtMiddleware)
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext?.Items["User"] is Models.Domain.User user && user.IsActive == true)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    // Kiểm tra claim IsActive
                    var isActiveClaim = context.User.FindFirst("IsActive")?.Value;
                    if (bool.TryParse(isActiveClaim, out var isActive) && isActive)
                    {
                        context.Succeed(requirement);
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}
