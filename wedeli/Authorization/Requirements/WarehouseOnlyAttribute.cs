using Microsoft.AspNetCore.Authorization;

namespace wedeli.Authorization.Requirements
{
    public class WarehouseOnlyAttribute : AuthorizeAttribute
    {
        public WarehouseOnlyAttribute()
        {
            Roles = "warehouse_staff,multi_role";
        }
    }
}
