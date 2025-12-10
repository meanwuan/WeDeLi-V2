using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;

namespace wedeli.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Seeds the database with initial data - runs only once per table
        /// </summary>
        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // 1. Seed Roles
                await SeedRolesAsync(context, logger);

                // 2. Seed Users FIRST (for FK constraint)
                bool usersSeeded = await SeedUsersAsync(context, logger);

                // 3. Seed Transport Companies (depends on Users for UserId FK)
                await SeedTransportCompaniesAsync(context, logger);

                // 4-8. Only seed dependent tables if Users were seeded
                // (to avoid FK constraint errors with existing data)
                if (usersSeeded)
                {
                    // 4. Seed Drivers
                    await SeedDriversAsync(context, logger);

                    // 5. Seed Customers
                    await SeedCustomersAsync(context, logger);

                    // 8. Seed Warehouse Staff
                    await SeedWarehouseStaffAsync(context, logger);
                }
                else
                {
                    logger.LogInformation("Skipping Driver, Customer, WarehouseStaff seeding (Users already exist)");
                }

                // 6. Seed Vehicles (independent of Users)
                await SeedVehiclesAsync(context, logger);

                // 7. Seed Routes (independent of Users)
                await SeedRoutesAsync(context, logger);

                // 9. Seed Company Partnerships
                await SeedCompanyPartnershipsAsync(context, logger);

                // 10. Seed Orders (depends on Customers, Routes, Vehicles, Drivers)
                await SeedOrdersAsync(context, logger);

                // 11. Seed Trips (depends on Routes, Vehicles, Drivers)
                await SeedTripsAsync(context, logger);

                // 12. Seed Trip Orders (depends on Trips, Orders)
                await SeedTripOrdersAsync(context, logger);

                // 13. Seed Ratings (depends on Orders, Customers, Drivers)
                await SeedRatingsAsync(context, logger);

                // 14. Seed Notifications (depends on Users, Orders)
                await SeedNotificationsAsync(context, logger);

                // 15. Seed Order Status Histories (depends on Orders, Users)
                await SeedOrderStatusHistoriesAsync(context, logger);

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        #region Seed Methods

        private static async Task SeedRolesAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Roles.AnyAsync()) return;

            logger.LogInformation("Seeding Roles...");

            var roles = new[]
            {
                new Role { RoleId = 1, RoleName = "SuperAdmin", Description = "Super Administrator with full system access", CreatedAt = DateTime.UtcNow },
                new Role { RoleId = 2, RoleName = "CompanyAdmin", Description = "Company Administrator managing transport company operations", CreatedAt = DateTime.UtcNow },
                new Role { RoleId = 3, RoleName = "WarehouseStaff", Description = "Warehouse staff managing inventory and packages", CreatedAt = DateTime.UtcNow },
                new Role { RoleId = 4, RoleName = "Driver", Description = "Driver delivering packages", CreatedAt = DateTime.UtcNow },
                new Role { RoleId = 5, RoleName = "Customer", Description = "Customer placing orders", CreatedAt = DateTime.UtcNow }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {roles.Length} roles.");
        }

        private static async Task SeedTransportCompaniesAsync(AppDbContext context, ILogger logger)
        {
            if (await context.TransportCompanies.AnyAsync()) return;

            logger.LogInformation("Seeding Transport Companies...");

            var companies = new[]
            {
                new TransportCompany
                {
                    CompanyId = 1,
                    CompanyName = "Nhà xe Thành Bưởi",
                    BusinessLicense = "GP-001-2024",
                    Address = "266 Lê Hồng Phong, Phường 4, Quận 5, TP.HCM",
                    Phone = "1900 6067",
                    Email = "contact@thanhbuoi.vn",
                    IsActive = true,
                    Rating = 4.5m,
                    Latitude = 10.7567890m,
                    Longitude = 106.6789012m,
                    UserId = 15, // thanhbuoi_admin_seed
                    CreatedAt = DateTime.UtcNow
                },
                new TransportCompany
                {
                    CompanyId = 2,
                    CompanyName = "Nhà xe Phương Trang",
                    BusinessLicense = "GP-002-2024",
                    Address = "272 Đề Thám, Phường Cô Giang, Quận 1, TP.HCM",
                    Phone = "1900 6067",
                    Email = "contact@futabus.vn",
                    IsActive = true,
                    Rating = 4.7m,
                    Latitude = 10.7623456m,
                    Longitude = 106.6901234m,
                    UserId = 16, // phuongtrang_admin_seed
                    CreatedAt = DateTime.UtcNow
                },
                new TransportCompany
                {
                    CompanyId = 3,
                    CompanyName = "Nhà xe Hoàng Long",
                    BusinessLicense = "GP-003-2024",
                    Address = "456 Điện Biên Phủ, Phường 21, Quận Bình Thạnh, TP.HCM",
                    Phone = "028 3512 3456",
                    Email = "contact@hoanglong.vn",
                    IsActive = true,
                    Rating = 4.3m,
                    Latitude = 10.8012345m,
                    Longitude = 106.7123456m,
                    UserId = 17, // hoanglong_admin_seed
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.TransportCompanies.AddRangeAsync(companies);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {companies.Length} transport companies.");
        }

        private static async Task<bool> SeedUsersAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Users table already has data. Skipping user seeding.");
                return false;
            }

            logger.LogInformation("Seeding Users...");

            // Password hash for "Password123!" using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

            var users = new[]
            {
                // SuperAdmin
                new User
                {
                    UserId = 14,
                    Username = "superadmin_seed",
                    PasswordHash = passwordHash,
                    Email = "superadmin_seed@wedeli.vn",
                    FullName = "Super Administrator (Seed)",
                    Phone = "0901100001",
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // CompanyAdmin - Thành Bưởi
                new User
                {
                    UserId = 15,
                    Username = "thanhbuoi_admin_seed",
                    PasswordHash = passwordHash,
                    Email = "admin_seed@thanhbuoi.vn",
                    FullName = "Nguyễn Văn Admin (Seed)",
                    Phone = "0901100002",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // CompanyAdmin - Phương Trang
                new User
                {
                    UserId = 16,
                    Username = "phuongtrang_admin_seed",
                    PasswordHash = passwordHash,
                    Email = "admin_seed@futabus.vn",
                    FullName = "Trần Thị Quản Lý (Seed)",
                    Phone = "0901100003",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // CompanyAdmin - Hoàng Long
                new User
                {
                    UserId = 17,
                    Username = "hoanglong_admin_seed",
                    PasswordHash = passwordHash,
                    Email = "admin_seed@hoanglong.vn",
                    FullName = "Lê Văn Hoàng (Seed)",
                    Phone = "0901100004",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Drivers - Thành Bưởi
                new User
                {
                    UserId = 18,
                    Username = "driver_tb1_seed",
                    PasswordHash = passwordHash,
                    Email = "driver1_seed@thanhbuoi.vn",
                    FullName = "Phạm Văn Tài Xế 1 (Seed)",
                    Phone = "0902100001",
                    RoleId = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 19,
                    Username = "driver_tb2_seed",
                    PasswordHash = passwordHash,
                    Email = "driver2_seed@thanhbuoi.vn",
                    FullName = "Nguyễn Văn Tài Xế 2 (Seed)",
                    Phone = "0902100002",
                    RoleId = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Drivers - Phương Trang
                new User
                {
                    UserId = 20,
                    Username = "driver_pt1_seed",
                    PasswordHash = passwordHash,
                    Email = "driver1_seed@futabus.vn",
                    FullName = "Trần Văn Lái Xe (Seed)",
                    Phone = "0902100003",
                    RoleId = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Warehouse Staff
                new User
                {
                    UserId = 21,
                    Username = "warehouse_tb1_seed",
                    PasswordHash = passwordHash,
                    Email = "warehouse1_seed@thanhbuoi.vn",
                    FullName = "Lê Thị Kho 1 (Seed)",
                    Phone = "0903100001",
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Customers
                new User
                {
                    UserId = 22,
                    Username = "customer1_seed",
                    PasswordHash = passwordHash,
                    Email = "customer1_seed@gmail.com",
                    FullName = "Nguyễn Thị Khách Hàng (Seed)",
                    Phone = "0904100001",
                    RoleId = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 23,
                    Username = "customer2_seed",
                    PasswordHash = passwordHash,
                    Email = "customer2_seed@gmail.com",
                    FullName = "Trần Văn Mua Hàng (Seed)",
                    Phone = "0904100002",
                    RoleId = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {users.Length} users.");
            return true;
        }

        private static async Task SeedDriversAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Drivers.AnyAsync()) return;

            logger.LogInformation("Seeding Drivers...");

            var drivers = new[]
            {
                new Driver
                {
                    DriverId = 1,
                    UserId = 18, // driver_tb1_seed
                    CompanyId = 1,
                    DriverLicense = "B2-123456",
                    LicenseExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3)),
                    TotalTrips = 150,
                    SuccessRate = 96.7m,
                    Rating = 4.8m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Driver
                {
                    DriverId = 2,
                    UserId = 19, // driver_tb2_seed
                    CompanyId = 1,
                    DriverLicense = "B2-234567",
                    LicenseExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                    TotalTrips = 200,
                    SuccessRate = 97.5m,
                    Rating = 4.9m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Driver
                {
                    DriverId = 3,
                    UserId = 20, // driver_pt1_seed
                    CompanyId = 2,
                    DriverLicense = "B2-345678",
                    LicenseExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(4)),
                    TotalTrips = 100,
                    SuccessRate = 98.0m,
                    Rating = 4.7m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Drivers.AddRangeAsync(drivers);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {drivers.Length} drivers.");
        }

        private static async Task SeedCustomersAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Customers.AnyAsync()) return;

            logger.LogInformation("Seeding Customers...");

            var customers = new[]
            {
                new Customer
                {
                    CustomerId = 1,
                    UserId = 22, // customer1_seed
                    FullName = "Nguyễn Thị Khách Hàng (Seed)",
                    Phone = "0904100001",
                    Email = "customer1_seed@gmail.com",
                    TotalOrders = 10,
                    TotalRevenue = 5000000m,
                    IsRegular = true,
                    PaymentPrivilege = "prepay",
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    CustomerId = 2,
                    UserId = 23, // customer2_seed
                    FullName = "Trần Văn Mua Hàng (Seed)",
                    Phone = "0904100002",
                    Email = "customer2_seed@gmail.com",
                    TotalOrders = 50,
                    TotalRevenue = 25000000m,
                    IsRegular = true,
                    PaymentPrivilege = "postpay",
                    CreditLimit = 10000000m,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();

            // Seed Customer Addresses
            var addresses = new[]
            {
                new CustomerAddress
                {
                    AddressId = 1,
                    CustomerId = 1,
                    AddressLabel = "Nhà riêng",
                    FullAddress = "123 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP.HCM",
                    Ward = "Phường Bến Thành",
                    District = "Quận 1",
                    Province = "TP.HCM",
                    Latitude = 10.7700000m,
                    Longitude = 106.6900000m,
                    IsDefault = true,
                    UsageCount = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new CustomerAddress
                {
                    AddressId = 2,
                    CustomerId = 1,
                    AddressLabel = "Văn phòng",
                    FullAddress = "456 Lê Lợi, Phường Bến Nghé, Quận 1, TP.HCM",
                    Ward = "Phường Bến Nghé",
                    District = "Quận 1",
                    Province = "TP.HCM",
                    Latitude = 10.7720000m,
                    Longitude = 106.7000000m,
                    IsDefault = false,
                    UsageCount = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new CustomerAddress
                {
                    AddressId = 3,
                    CustomerId = 2,
                    AddressLabel = "Công ty",
                    FullAddress = "789 Điện Biên Phủ, Phường 15, Quận Bình Thạnh, TP.HCM",
                    Ward = "Phường 15",
                    District = "Quận Bình Thạnh",
                    Province = "TP.HCM",
                    Latitude = 10.8000000m,
                    Longitude = 106.7100000m,
                    IsDefault = true,
                    UsageCount = 10,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.CustomerAddresses.AddRangeAsync(addresses);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {customers.Length} customers with addresses.");
        }

        private static async Task SeedVehiclesAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Vehicles.AnyAsync()) return;

            logger.LogInformation("Seeding Vehicles...");

            var vehicles = new[]
            {
                // Thành Bưởi vehicles
                new Vehicle
                {
                    VehicleId = 1,
                    CompanyId = 1,
                    LicensePlate = "51A-12345",
                    VehicleType = "truck",
                    MaxWeightKg = 5000m,
                    MaxVolumeM3 = 20m,
                    CurrentWeightKg = 0m,
                    CapacityPercentage = 0m,
                    OverloadThreshold = 90m,
                    AllowOverload = false,
                    CurrentStatus = "available",
                    GpsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    VehicleId = 2,
                    CompanyId = 1,
                    LicensePlate = "51A-23456",
                    VehicleType = "van",
                    MaxWeightKg = 1500m,
                    MaxVolumeM3 = 8m,
                    CurrentWeightKg = 500m,
                    CapacityPercentage = 33.3m,
                    OverloadThreshold = 85m,
                    AllowOverload = false,
                    CurrentStatus = "in_transit",
                    GpsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Phương Trang vehicles
                new Vehicle
                {
                    VehicleId = 3,
                    CompanyId = 2,
                    LicensePlate = "51B-34567",
                    VehicleType = "truck",
                    MaxWeightKg = 6000m,
                    MaxVolumeM3 = 25m,
                    CurrentWeightKg = 2000m,
                    CapacityPercentage = 33.3m,
                    OverloadThreshold = 90m,
                    AllowOverload = false,
                    CurrentStatus = "available",
                    GpsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    VehicleId = 4,
                    CompanyId = 2,
                    LicensePlate = "51B-45678",
                    VehicleType = "motorbike",
                    MaxWeightKg = 50m,
                    MaxVolumeM3 = 0.5m,
                    CurrentWeightKg = 0m,
                    CapacityPercentage = 0m,
                    OverloadThreshold = 100m,
                    AllowOverload = true,
                    CurrentStatus = "available",
                    GpsEnabled = false,
                    CreatedAt = DateTime.UtcNow
                },
                // Hoàng Long vehicles
                new Vehicle
                {
                    VehicleId = 5,
                    CompanyId = 3,
                    LicensePlate = "51C-56789",
                    VehicleType = "truck",
                    MaxWeightKg = 8000m,
                    MaxVolumeM3 = 30m,
                    CurrentWeightKg = 0m,
                    CapacityPercentage = 0m,
                    OverloadThreshold = 90m,
                    AllowOverload = false,
                    CurrentStatus = "maintenance",
                    GpsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {vehicles.Length} vehicles.");
        }

        private static async Task SeedRoutesAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Routes.AnyAsync()) return;

            logger.LogInformation("Seeding Routes...");

            var routes = new[]
            {
                new Models.Domain.Route
                {
                    RouteId = 1,
                    CompanyId = 1,
                    RouteName = "HCM - Đà Lạt",
                    OriginProvince = "TP.HCM",
                    OriginDistrict = "Quận 5",
                    DestinationProvince = "Lâm Đồng",
                    DestinationDistrict = "TP Đà Lạt",
                    DistanceKm = 310m,
                    EstimatedDurationHours = 7m,
                    BasePrice = 150000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Models.Domain.Route
                {
                    RouteId = 2,
                    CompanyId = 1,
                    RouteName = "HCM - Nha Trang",
                    OriginProvince = "TP.HCM",
                    OriginDistrict = "Quận 5",
                    DestinationProvince = "Khánh Hòa",
                    DestinationDistrict = "TP Nha Trang",
                    DistanceKm = 430m,
                    EstimatedDurationHours = 8m,
                    BasePrice = 200000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Models.Domain.Route
                {
                    RouteId = 3,
                    CompanyId = 2,
                    RouteName = "HCM - Cần Thơ",
                    OriginProvince = "TP.HCM",
                    OriginDistrict = "Quận 1",
                    DestinationProvince = "Cần Thơ",
                    DestinationDistrict = "Quận Ninh Kiều",
                    DistanceKm = 170m,
                    EstimatedDurationHours = 4m,
                    BasePrice = 100000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Models.Domain.Route
                {
                    RouteId = 4,
                    CompanyId = 2,
                    RouteName = "HCM - Vũng Tàu",
                    OriginProvince = "TP.HCM",
                    OriginDistrict = "Quận 1",
                    DestinationProvince = "Bà Rịa - Vũng Tàu",
                    DestinationDistrict = "TP Vũng Tàu",
                    DistanceKm = 120m,
                    EstimatedDurationHours = 3m,
                    BasePrice = 80000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Models.Domain.Route
                {
                    RouteId = 5,
                    CompanyId = 3,
                    RouteName = "HCM - Hà Nội",
                    OriginProvince = "TP.HCM",
                    OriginDistrict = "Quận Bình Thạnh",
                    DestinationProvince = "Hà Nội",
                    DestinationDistrict = "Quận Hoàn Kiếm",
                    DistanceKm = 1700m,
                    EstimatedDurationHours = 36m,
                    BasePrice = 500000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Routes.AddRangeAsync(routes);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {routes.Length} routes.");
        }

        private static async Task SeedWarehouseStaffAsync(AppDbContext context, ILogger logger)
        {
            if (await context.WarehouseStaffs.AnyAsync()) return;

            logger.LogInformation("Seeding Warehouse Staff...");

            var staff = new[]
            {
                new WarehouseStaff
                {
                    StaffId = 1,
                    UserId = 21, // warehouse_tb1_seed
                    CompanyId = 1,
                    WarehouseLocation = "Kho Bến xe Miền Đông",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.WarehouseStaffs.AddRangeAsync(staff);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {staff.Length} warehouse staff.");
        }

        private static async Task SeedCompanyPartnershipsAsync(AppDbContext context, ILogger logger)
        {
            if (await context.CompanyPartnerships.AnyAsync()) return;

            logger.LogInformation("Seeding Company Partnerships...");

            var partnerships = new[]
            {
                new CompanyPartnership
                {
                    PartnershipId = 1,
                    CompanyId = 1, // Thành Bưởi
                    PartnerCompanyId = 2, // Phương Trang
                    PartnershipLevel = "preferred",
                    CommissionRate = 5.0m,
                    PriorityOrder = 1,
                    TotalTransferredOrders = 0,
                    IsActive = true,
                    Notes = "Đối tác chiến lược",
                    CreatedBy = 15, // thanhbuoi_admin_seed
                    CreatedAt = DateTime.UtcNow
                },
                new CompanyPartnership
                {
                    PartnershipId = 2,
                    CompanyId = 1, // Thành Bưởi
                    PartnerCompanyId = 3, // Hoàng Long
                    PartnershipLevel = "regular",
                    CommissionRate = 7.0m,
                    PriorityOrder = 2,
                    TotalTransferredOrders = 0,
                    IsActive = true,
                    Notes = "Đối tác thường xuyên",
                    CreatedBy = 15,
                    CreatedAt = DateTime.UtcNow
                },
                new CompanyPartnership
                {
                    PartnershipId = 3,
                    CompanyId = 2, // Phương Trang
                    PartnerCompanyId = 3, // Hoàng Long
                    PartnershipLevel = "backup",
                    CommissionRate = 8.0m,
                    PriorityOrder = 3,
                    TotalTransferredOrders = 0,
                    IsActive = true,
                    Notes = "Đối tác dự phòng",
                    CreatedBy = 16,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.CompanyPartnerships.AddRangeAsync(partnerships);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {partnerships.Length} company partnerships.");
        }

        private static async Task SeedOrdersAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Orders.AnyAsync()) return;

            logger.LogInformation("Seeding Orders...");

            var orders = new[]
            {
                new Order
                {
                    OrderId = 1,
                    TrackingCode = "WDL-2024-000001",
                    CustomerId = 1,
                    SenderName = "Nguyễn Thị Khách Hàng",
                    SenderPhone = "0904100001",
                    SenderAddress = "123 Nguyễn Trãi, Quận 1, TP.HCM",
                    ReceiverName = "Trần Văn Nhận",
                    ReceiverPhone = "0901234567",
                    ReceiverAddress = "456 Trần Phú, TP. Đà Lạt, Lâm Đồng",
                    ReceiverProvince = "Lâm Đồng",
                    ReceiverDistrict = "TP Đà Lạt",
                    ParcelType = "fragile",
                    WeightKg = 2.5m,
                    DeclaredValue = 1000000m,
                    SpecialInstructions = "Hàng dễ vỡ, xin nhẹ tay",
                    RouteId = 1,
                    VehicleId = 1,
                    DriverId = 1,
                    ShippingFee = 150000m,
                    CodAmount = 500000m,
                    PaymentMethod = "cash",
                    PaymentStatus = "unpaid",
                    OrderStatus = "in_transit",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    PickupScheduledAt = DateTime.UtcNow.AddDays(-2),
                    PickupConfirmedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Order
                {
                    OrderId = 2,
                    TrackingCode = "WDL-2024-000002",
                    CustomerId = 1,
                    SenderName = "Nguyễn Thị Khách Hàng",
                    SenderPhone = "0904100001",
                    SenderAddress = "123 Nguyễn Trãi, Quận 1, TP.HCM",
                    ReceiverName = "Lê Thị Hoa",
                    ReceiverPhone = "0909876543",
                    ReceiverAddress = "789 Lê Lợi, TP. Nha Trang, Khánh Hòa",
                    ReceiverProvince = "Khánh Hòa",
                    ReceiverDistrict = "TP Nha Trang",
                    ParcelType = "electronics",
                    WeightKg = 5.0m,
                    DeclaredValue = 5000000m,
                    SpecialInstructions = "Điện thoại, cần giao cẩn thận",
                    RouteId = 2,
                    VehicleId = 2,
                    DriverId = 2,
                    ShippingFee = 200000m,
                    CodAmount = 0m,
                    PaymentMethod = "bank_transfer",
                    PaymentStatus = "paid",
                    PaidAt = DateTime.UtcNow.AddDays(-3),
                    OrderStatus = "delivered",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    PickupScheduledAt = DateTime.UtcNow.AddDays(-5),
                    PickupConfirmedAt = DateTime.UtcNow.AddDays(-4),
                    DeliveredAt = DateTime.UtcNow.AddDays(-3)
                },
                new Order
                {
                    OrderId = 3,
                    TrackingCode = "WDL-2024-000003",
                    CustomerId = 2,
                    SenderName = "Trần Văn Mua Hàng",
                    SenderPhone = "0904100002",
                    SenderAddress = "789 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM",
                    ReceiverName = "Phạm Văn An",
                    ReceiverPhone = "0912345678",
                    ReceiverAddress = "123 Nguyễn Văn Linh, Quận Ninh Kiều, Cần Thơ",
                    ReceiverProvince = "Cần Thơ",
                    ReceiverDistrict = "Quận Ninh Kiều",
                    ParcelType = "document",
                    WeightKg = 0.5m,
                    DeclaredValue = 100000m,
                    RouteId = 3,
                    VehicleId = 3,
                    DriverId = 3,
                    ShippingFee = 100000m,
                    CodAmount = 0m,
                    PaymentMethod = "periodic",
                    PaymentStatus = "pending",
                    OrderStatus = "pending_pickup",
                    CreatedAt = DateTime.UtcNow
                },
                new Order
                {
                    OrderId = 4,
                    TrackingCode = "WDL-2024-000004",
                    CustomerId = 2,
                    SenderName = "Trần Văn Mua Hàng",
                    SenderPhone = "0904100002",
                    SenderAddress = "789 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM",
                    ReceiverName = "Nguyễn Thị Mai",
                    ReceiverPhone = "0987654321",
                    ReceiverAddress = "456 Bãi Trước, TP. Vũng Tàu",
                    ReceiverProvince = "Bà Rịa - Vũng Tàu",
                    ReceiverDistrict = "TP Vũng Tàu",
                    ParcelType = "food",
                    WeightKg = 3.0m,
                    DeclaredValue = 300000m,
                    SpecialInstructions = "Thực phẩm, giao trong ngày",
                    RouteId = 4,
                    VehicleId = 4,
                    ShippingFee = 80000m,
                    CodAmount = 300000m,
                    PaymentMethod = "cash",
                    PaymentStatus = "unpaid",
                    OrderStatus = "picked_up",
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    PickupScheduledAt = DateTime.UtcNow.AddHours(-8),
                    PickupConfirmedAt = DateTime.UtcNow.AddHours(-5)
                },
                new Order
                {
                    OrderId = 5,
                    TrackingCode = "WDL-2024-000005",
                    CustomerId = 1,
                    SenderName = "Nguyễn Thị Khách Hàng",
                    SenderPhone = "0904100001",
                    SenderAddress = "456 Lê Lợi, Quận 1, TP.HCM",
                    ReceiverName = "Hoàng Văn Long",
                    ReceiverPhone = "0923456789",
                    ReceiverAddress = "789 Phố Huế, Quận Hoàn Kiếm, Hà Nội",
                    ReceiverProvince = "Hà Nội",
                    ReceiverDistrict = "Quận Hoàn Kiếm",
                    ParcelType = "other",
                    WeightKg = 10.0m,
                    DeclaredValue = 2000000m,
                    RouteId = 5,
                    VehicleId = 5,
                    ShippingFee = 500000m,
                    CodAmount = 1500000m,
                    PaymentMethod = "cash",
                    PaymentStatus = "unpaid",
                    OrderStatus = "cancelled",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {orders.Length} orders.");
        }

        private static async Task SeedTripsAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Trips.AnyAsync()) return;

            logger.LogInformation("Seeding Trips...");

            var trips = new[]
            {
                new Trip
                {
                    TripId = 1,
                    RouteId = 1, // HCM - Đà Lạt
                    VehicleId = 1,
                    DriverId = 1,
                    TripDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    DepartureTime = DateTime.UtcNow.AddHours(-4),
                    TripStatus = "in_progress",
                    TotalOrders = 1,
                    TotalWeightKg = 2.5m,
                    IsReturnTrip = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Trip
                {
                    TripId = 2,
                    RouteId = 2, // HCM - Nha Trang
                    VehicleId = 2,
                    DriverId = 2,
                    TripDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                    DepartureTime = DateTime.UtcNow.AddDays(-3),
                    ArrivalTime = DateTime.UtcNow.AddDays(-3).AddHours(8),
                    TripStatus = "completed",
                    TotalOrders = 1,
                    TotalWeightKg = 5.0m,
                    IsReturnTrip = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Trip
                {
                    TripId = 3,
                    RouteId = 3, // HCM - Cần Thơ
                    VehicleId = 3,
                    DriverId = 3,
                    TripDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                    TripStatus = "scheduled",
                    TotalOrders = 1,
                    TotalWeightKg = 0.5m,
                    IsReturnTrip = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Trips.AddRangeAsync(trips);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {trips.Length} trips.");
        }

        private static async Task SeedTripOrdersAsync(AppDbContext context, ILogger logger)
        {
            if (await context.TripOrders.AnyAsync()) return;

            logger.LogInformation("Seeding Trip Orders...");

            var tripOrders = new[]
            {
                new TripOrder
                {
                    TripOrderId = 1,
                    TripId = 1,
                    OrderId = 1,
                    SequenceNumber = 1,
                    PickupConfirmed = true,
                    DeliveryConfirmed = false
                },
                new TripOrder
                {
                    TripOrderId = 2,
                    TripId = 2,
                    OrderId = 2,
                    SequenceNumber = 1,
                    PickupConfirmed = true,
                    DeliveryConfirmed = true
                },
                new TripOrder
                {
                    TripOrderId = 3,
                    TripId = 3,
                    OrderId = 3,
                    SequenceNumber = 1,
                    PickupConfirmed = false,
                    DeliveryConfirmed = false
                }
            };

            await context.TripOrders.AddRangeAsync(tripOrders);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {tripOrders.Length} trip orders.");
        }

        private static async Task SeedRatingsAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Ratings.AnyAsync()) return;

            logger.LogInformation("Seeding Ratings...");

            var ratings = new[]
            {
                new Rating
                {
                    RatingId = 1,
                    OrderId = 2, // Đơn hàng đã giao
                    CustomerId = 1,
                    DriverId = 2,
                    RatingScore = 5,
                    ReviewText = "Giao hàng nhanh, tài xế thân thiện",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await context.Ratings.AddRangeAsync(ratings);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {ratings.Length} ratings.");
        }

        private static async Task SeedNotificationsAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Notifications.AnyAsync()) return;

            logger.LogInformation("Seeding Notifications...");

            var notifications = new[]
            {
                new Notification
                {
                    NotificationId = 1,
                    UserId = 22, // customer1_seed
                    Title = "Đơn hàng đang vận chuyển",
                    Message = "Đơn hàng WDL-2024-000001 của bạn đang được vận chuyển đến Đà Lạt",
                    NotificationType = "order_status",
                    OrderId = 1,
                    IsRead = false,
                    SentVia = "push",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Notification
                {
                    NotificationId = 2,
                    UserId = 22,
                    Title = "Đơn hàng đã giao thành công",
                    Message = "Đơn hàng WDL-2024-000002 đã được giao thành công",
                    NotificationType = "order_status",
                    OrderId = 2,
                    IsRead = true,
                    SentVia = "push",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Notification
                {
                    NotificationId = 3,
                    UserId = 18, // driver_tb1_seed
                    Title = "Chuyến xe mới",
                    Message = "Bạn có chuyến xe mới đi Đà Lạt vào hôm nay",
                    NotificationType = "system",
                    IsRead = true,
                    SentVia = "push",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {notifications.Length} notifications.");
        }

        private static async Task SeedOrderStatusHistoriesAsync(AppDbContext context, ILogger logger)
        {
            if (await context.OrderStatusHistories.AnyAsync()) return;

            logger.LogInformation("Seeding Order Status Histories...");

            var histories = new[]
            {
                // Order 1 history
                new OrderStatusHistory
                {
                    HistoryId = 1,
                    OrderId = 1,
                    OldStatus = null,
                    NewStatus = "pending_pickup",
                    Notes = "Đơn hàng mới tạo",
                    UpdatedBy = 22,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new OrderStatusHistory
                {
                    HistoryId = 2,
                    OrderId = 1,
                    OldStatus = "pending_pickup",
                    NewStatus = "picked_up",
                    Notes = "Đã lấy hàng",
                    UpdatedBy = 18,
                    CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
                },
                new OrderStatusHistory
                {
                    HistoryId = 3,
                    OrderId = 1,
                    OldStatus = "picked_up",
                    NewStatus = "in_transit",
                    Notes = "Đang vận chuyển",
                    UpdatedBy = 18,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                // Order 2 history
                new OrderStatusHistory
                {
                    HistoryId = 4,
                    OrderId = 2,
                    OldStatus = null,
                    NewStatus = "pending_pickup",
                    Notes = "Đơn hàng mới tạo",
                    UpdatedBy = 22,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new OrderStatusHistory
                {
                    HistoryId = 5,
                    OrderId = 2,
                    OldStatus = "pending_pickup",
                    NewStatus = "delivered",
                    Notes = "Giao hàng thành công",
                    UpdatedBy = 19,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            await context.OrderStatusHistories.AddRangeAsync(histories);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {histories.Length} order status histories.");
        }

        #endregion

        /// <summary>
        /// Initialize database synchronously
        /// </summary>
        public static void Initialize(AppDbContext context, ILogger logger)
        {
            SeedAsync(context, logger).GetAwaiter().GetResult();
        }
    }
}