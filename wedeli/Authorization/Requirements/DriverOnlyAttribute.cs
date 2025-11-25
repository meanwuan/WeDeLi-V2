using Microsoft.AspNetCore.Authorization;

namespace wedeli.Authorization.Requirements
{
    public class DriverOnlyAttribute : AuthorizeAttribute
    {
        public DriverOnlyAttribute()
        {
            Roles = "driver,multi_role";
        }
    }
}
