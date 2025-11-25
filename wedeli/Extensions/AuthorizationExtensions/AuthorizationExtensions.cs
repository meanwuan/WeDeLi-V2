using Microsoft.AspNetCore.Authorization;
using wedeli.Authorization.Policies;
using static wedeli.Authorization.Requirements.ActiveUserRequirement;

namespace wedeli.Extensions.AuthorizationExtensions
{
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Đăng ký tất cả authorization services
        /// </summary>
        public static IServiceCollection AddWeDeliAuthorization(this IServiceCollection services)
        {
            // Đăng ký Authorization Handlers
            services.AddScoped<IAuthorizationHandler, ActiveUserRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, CompanyAccessHandler>();

            // Đăng ký Policies
            services.AddAuthorizationPolicies();

            return services;
        }
    }
}
