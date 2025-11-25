using wedeli.Middleware;

namespace wedeli.Extensions
{
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Sử dụng JWT Middleware
        /// </summary>
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtMiddleware>();
        }

        /// <summary>
        /// Sử dụng Role Authorization Middleware
        /// </summary>
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RoleAuthorizationMiddleware>();
        }
    }
}
