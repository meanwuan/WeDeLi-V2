using Microsoft.AspNetCore.Authorization;

namespace wedeli.Authorization.Requirements
{
    public class StaffOnlyAttribute : AuthorizeAttribute    
    {
        public StaffOnlyAttribute()
        {
            Roles = "admin,driver,warehouse_staff,multi_role";
        }
    }
}
