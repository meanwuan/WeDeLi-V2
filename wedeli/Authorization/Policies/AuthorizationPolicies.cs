using wedeli.Authorization.Requirements;

namespace wedeli.Authorization.Policies
{
    public static class AuthorizationPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string DriverOnly = "DriverOnly";
        public const string WarehouseOnly = "WarehouseOnly";
        public const string CustomerOnly = "CustomerOnly";
        public const string AdminOrDriver = "AdminOrDriver";
        public const string AdminOrWarehouse = "AdminOrWarehouse";
        public const string StaffOnly = "StaffOnly";
        public const string ActiveUserOnly = "ActiveUserOnly";

        /// <summary>
        /// Đăng ký tất cả policies trong Program.cs
        /// </summary>
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Policy: Chỉ Admin
                options.AddPolicy(AdminOnly, policy =>
                    policy.RequireRole("admin"));

                // Policy: Chỉ Driver
                options.AddPolicy(DriverOnly, policy =>
                    policy.RequireRole("driver", "multi_role"));

                // Policy: Chỉ Warehouse Staff
                options.AddPolicy(WarehouseOnly, policy =>
                    policy.RequireRole("warehouse_staff", "multi_role"));

                // Policy: Chỉ Customer
                options.AddPolicy(CustomerOnly, policy =>
                    policy.RequireRole("customer"));

                // Policy: Admin hoặc Driver
                options.AddPolicy(AdminOrDriver, policy =>
                    policy.RequireRole("admin", "driver", "multi_role"));

                // Policy: Admin hoặc Warehouse
                options.AddPolicy(AdminOrWarehouse, policy =>
                    policy.RequireRole("admin", "warehouse_staff", "multi_role"));

                // Policy: Tất cả Staff (không phải Customer)
                options.AddPolicy(StaffOnly, policy =>
                    policy.RequireRole("admin", "driver", "warehouse_staff", "multi_role"));

                // Policy: User phải active
                options.AddPolicy(ActiveUserOnly, policy =>
                    policy.Requirements.Add(new ActiveUserRequirement()));
            });
        }
    }
}
