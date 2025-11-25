using Microsoft.AspNetCore.Authorization;

namespace wedeli.Authorization.Requirements
{
    public class CustomerOnlyAttribute : AuthorizeAttribute
    { 
        public CustomerOnlyAttribute()
        {
            Roles = "customer";
        }
    }
}
