using Microsoft.AspNetCore.Authorization;

namespace wedeli.Authorization.Attributes
{
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute()
        {
            Roles = "admin";
        }
    }
}
