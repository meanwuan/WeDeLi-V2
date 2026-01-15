using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Infrastructure;

namespace wedeli.Models.Domain.Data;

/// <summary>
/// Database Initializer for dual database architecture (Platform + Company databases)
/// Creates comprehensive seed data for testing APIs
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initialize and seed both Platform and Company databases
    /// </summary>
    public static async Task SeedAsync(
        PlatformDbContext platformContext,
        AppDbContext companyContext,
        IPasswordService passwordService,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("Starting database seeding...");

            // Seed Platform Database (wedeli_platform)
            await SeedPlatformDataAsync(platformContext, passwordService, logger);

            // Seed Company Database (wedeli_company)
            await SeedCompanyDataAsync(companyContext, platformContext, passwordService, logger);

            logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    #region Platform Database Seeding

    private static async Task SeedPlatformDataAsync(
        PlatformDbContext context,
        IPasswordService passwordService,
        ILogger logger)
    {
        // 1. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            logger.LogInformation("Seeding Roles...");
            var roles = new List<Role>
            {
                new Role { RoleName = "SuperAdmin", Description = "System administrator with full access", CreatedAt = DateTime.UtcNow },
                new Role { RoleName = "CompanyAdmin", Description = "Transport company administrator", CreatedAt = DateTime.UtcNow },
                new Role { RoleName = "Admin", Description = "General administrator", CreatedAt = DateTime.UtcNow },
                new Role { RoleName = "Driver", Description = "Delivery driver", CreatedAt = DateTime.UtcNow },
                new Role { RoleName = "Customer", Description = "Customer/Sender", CreatedAt = DateTime.UtcNow },
                new Role { RoleName = "WarehouseStaff", Description = "Warehouse staff member", CreatedAt = DateTime.UtcNow }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} roles", roles.Count);
        }

        // 2. Seed Users
        if (!await context.Users.AnyAsync())
        {
            logger.LogInformation("Seeding Users...");
            var users = new List<User>
            {
                // SuperAdmin
                new User
                {
                    Username = "superadmin",
                    PasswordHash = passwordService.HashPassword("Admin@123"),
                    FullName = "Super Administrator",
                    Phone = "0900000001",
                    Email = "superadmin@wedeli.vn",
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // CompanyAdmin 1 (Nhà xe Hoàng Long)
                new User
                {
                    Username = "hoanglong_admin",
                    PasswordHash = passwordService.HashPassword("Admin@123"),
                    FullName = "Nguyễn Văn Long",
                    Phone = "0900000002",
                    Email = "admin@hoanglong.vn",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // CompanyAdmin 2 (Nhà xe Phương Trang)
                new User
                {
                    Username = "phuongtrang_admin",
                    PasswordHash = passwordService.HashPassword("Admin@123"),
                    FullName = "Trần Phương Trang",
                    Phone = "0900000003",
                    Email = "admin@phuongtrang.vn",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Customer 1 - Regular customer
                new User
                {
                    Username = "customer1",
                    PasswordHash = passwordService.HashPassword("Customer@123"),
                    FullName = "Hoàng Thị Mai",
                    Phone = "0900000006",
                    Email = "hoangmai@gmail.com",
                    RoleId = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Customer 2 - Regular customer
                new User
                {
                    Username = "customer2",
                    PasswordHash = passwordService.HashPassword("Customer@123"),
                    FullName = "Vũ Đức Anh",
                    Phone = "0900000007",
                    Email = "vuducanh@gmail.com",
                    RoleId = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Customer 3 - VIP customer
                new User
                {
                    Username = "customer3",
                    PasswordHash = passwordService.HashPassword("Customer@123"),
                    FullName = "Lê Văn Hùng",
                    Phone = "0900000008",
                    Email = "levanhung@gmail.com",
                    RoleId = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Customer 4
                new User
                {
                    Username = "customer4",
                    PasswordHash = passwordService.HashPassword("Customer@123"),
                    FullName = "Nguyễn Thị Hoa",
                    Phone = "0900000009",
                    Email = "nguyenhoa@gmail.com",
                    RoleId = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Customer 5
                new User
                {
                    Username = "customer5",
                    PasswordHash = passwordService.HashPassword("Customer@123"),
                    FullName = "Trần Minh Tuấn",
                    Phone = "0900000010",
                    Email = "tranminhtuan@gmail.com",
                    RoleId = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} users", users.Count);
        }

        // 3. Seed Customers (linked to User accounts with RoleId=5)
        if (!await context.Customers.AnyAsync())
        {
            logger.LogInformation("Seeding Customers...");
            var customerUsers = await context.Users.Where(u => u.RoleId == 5).ToListAsync();
            var customers = new List<Customer>();
            var isVip = true;
            foreach (var u in customerUsers)
            {
                customers.Add(new Customer
                {
                    UserId = u.UserId,
                    FullName = u.FullName ?? "Customer",
                    Phone = u.Phone ?? "",
                    Email = u.Email,
                    IsRegular = isVip,
                    TotalOrders = isVip ? 25 : 0,
                    TotalRevenue = isVip ? 5000000 : 0,
                    PaymentPrivilege = isVip ? "credit" : "standard",
                    CreditLimit = isVip ? 2000000 : 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                isVip = !isVip; // Alternate VIP status
            }
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} customers", customers.Count);
        }

        // 4. Seed Transport Companies
        if (!await context.TransportCompanies.AnyAsync())
        {
            logger.LogInformation("Seeding Transport Companies...");
            var companyAdmins = await context.Users.Where(u => u.RoleId == 2).OrderBy(u => u.UserId).ToListAsync();
            var companies = new List<TransportCompany>
            {
                new TransportCompany
                {
                    UserId = companyAdmins.ElementAtOrDefault(0)?.UserId ?? 2,
                    CompanyName = "Nhà xe Hoàng Long",
                    Phone = "0283123456",
                    Email = "contact@hoanglong.vn",
                    Address = "123 Lê Hồng Phong, Quận 10, TP.HCM",
                    Rating = 4.5m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TransportCompany
                {
                    UserId = companyAdmins.ElementAtOrDefault(1)?.UserId ?? 3,
                    CompanyName = "Nhà xe Phương Trang",
                    Phone = "0283789012",
                    Email = "contact@phuongtrang.vn",
                    Address = "456 Nguyễn Văn Cừ, Quận 5, TP.HCM",
                    Rating = 4.8m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.TransportCompanies.AddRangeAsync(companies);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} transport companies", companies.Count);
        }
    }

    #endregion

    #region Company Database Seeding

    private static async Task SeedCompanyDataAsync(
        AppDbContext context,
        PlatformDbContext platformContext,
        IPasswordService passwordService,
        ILogger logger)
    {
        // Get reference data from Platform DB
        var companies = await platformContext.TransportCompanies.ToListAsync();
        var customers = await platformContext.Customers.ToListAsync();

        if (!companies.Any())
        {
            logger.LogWarning("No companies found in Platform DB. Skipping Company DB seeding.");
            return;
        }

        // 1. Seed Company Roles
        if (!await context.CompanyRoles.AnyAsync())
        {
            logger.LogInformation("Seeding Company Roles...");
            foreach (var company in companies)
            {
                var companyRoles = new List<CompanyRole>
                {
                    new CompanyRole { CompanyId = company.CompanyId, RoleName = "Admin", Description = "Company Administrator", CreatedAt = DateTime.UtcNow },
                    new CompanyRole { CompanyId = company.CompanyId, RoleName = "Driver", Description = "Driver", CreatedAt = DateTime.UtcNow },
                    new CompanyRole { CompanyId = company.CompanyId, RoleName = "Staff", Description = "Warehouse Staff", CreatedAt = DateTime.UtcNow }
                };
                await context.CompanyRoles.AddRangeAsync(companyRoles);
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded company roles for {Count} companies", companies.Count);
        }

        // 2. Seed Company Users (4 drivers per company)
        if (!await context.CompanyUsers.AnyAsync())
        {
            logger.LogInformation("Seeding Company Users...");
            var driverRoles = await context.CompanyRoles.Where(r => r.RoleName == "Driver").ToListAsync();
            var staffRoles = await context.CompanyRoles.Where(r => r.RoleName == "Staff").ToListAsync();
            
            var driverNames = new[] { "Nguyễn Văn Minh", "Trần Đức Thắng", "Lê Hoàng Nam", "Phạm Quốc Việt" };
            
            foreach (var company in companies)
            {
                var driverRole = driverRoles.FirstOrDefault(r => r.CompanyId == company.CompanyId);
                var staffRole = staffRoles.FirstOrDefault(r => r.CompanyId == company.CompanyId);
                if (driverRole == null) continue;

                // 4 Drivers per company
                for (int i = 0; i < 4; i++)
                {
                    await context.CompanyUsers.AddAsync(new CompanyUser
                    {
                        CompanyId = company.CompanyId,
                        CompanyRoleId = driverRole.CompanyRoleId,
                        Username = $"driver_{company.CompanyId}_{i + 1}",
                        PasswordHash = passwordService.HashPassword("Driver@123"),
                        FullName = $"{driverNames[i]} ({company.CompanyName.Split(' ').Last()})",
                        Phone = $"09{company.CompanyId:D2}{(i + 1):D2}00{company.CompanyId}{i + 1}",
                        Email = $"driver{i + 1}@company{company.CompanyId}.vn",
                        IsActive = i < 3, // 3 active, 1 inactive
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                
                // 2 Warehouse staff per company
                if (staffRole != null)
                {
                    await context.CompanyUsers.AddAsync(new CompanyUser
                    {
                        CompanyId = company.CompanyId,
                        CompanyRoleId = staffRole.CompanyRoleId,
                        Username = $"staff_{company.CompanyId}_1",
                        PasswordHash = passwordService.HashPassword("Staff@123"),
                        FullName = $"Kho {company.CompanyId} - Nhân viên 1",
                        Phone = $"09{company.CompanyId:D2}9001",
                        Email = $"staff1@company{company.CompanyId}.vn",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    
                    // Second warehouse staff
                    await context.CompanyUsers.AddAsync(new CompanyUser
                    {
                        CompanyId = company.CompanyId,
                        CompanyRoleId = staffRole.CompanyRoleId,
                        Username = $"staff_{company.CompanyId}_2",
                        PasswordHash = passwordService.HashPassword("Staff@123"),
                        FullName = $"Kho {company.CompanyId} - Nhân viên 2",
                        Phone = $"09{company.CompanyId:D2}9002",
                        Email = $"staff2@company{company.CompanyId}.vn",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded company users");
        }

        // 3. Seed Drivers with varied stats
        if (!await context.Drivers.AnyAsync())
        {
            logger.LogInformation("Seeding Drivers...");
            var companyUsers = await context.CompanyUsers
                .Join(context.CompanyRoles.Where(r => r.RoleName == "Driver"),
                    u => u.CompanyRoleId, r => r.CompanyRoleId, (u, r) => u)
                .ToListAsync();
            
            var random = new Random(42);
            int idx = 0;
            foreach (var companyUser in companyUsers)
            {
                var driver = new Driver
                {
                    CompanyUserId = companyUser.CompanyUserId,
                    CompanyId = companyUser.CompanyId,
                    DriverLicense = $"B2-{78000 + idx:D6}",
                    LicenseExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(random.Next(1, 5))),
                    TotalTrips = random.Next(10, 200),
                    SuccessRate = 85 + random.Next(0, 15),
                    Rating = 3.5m + (decimal)(random.NextDouble() * 1.5),
                    IsActive = companyUser.IsActive == true,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Drivers.AddAsync(driver);
                idx++;
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} drivers", companyUsers.Count);
        }

        // 3.5. Seed WarehouseStaff entities
        if (!await context.WarehouseStaffs.AnyAsync())
        {
            logger.LogInformation("Seeding Warehouse Staff...");
            var staffUsers = await context.CompanyUsers
                .Join(context.CompanyRoles.Where(r => r.RoleName == "Staff"),
                    u => u.CompanyRoleId, r => r.CompanyRoleId, (u, r) => u)
                .ToListAsync();
            
            var warehouseLocations = new[] { "Kho Quận 7", "Kho Thủ Đức", "Kho Tân Bình", "Kho Bình Thạnh" };
            int locIdx = 0;
            
            foreach (var staffUser in staffUsers)
            {
                await context.WarehouseStaffs.AddAsync(new WarehouseStaff
                {
                    CompanyUserId = staffUser.CompanyUserId,
                    CompanyId = staffUser.CompanyId,
                    WarehouseLocation = warehouseLocations[locIdx % warehouseLocations.Length],
                    IsActive = staffUser.IsActive == true,
                    CreatedAt = DateTime.UtcNow
                });
                locIdx++;
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} warehouse staff", staffUsers.Count);
        }

        // 4. Seed Vehicles with varied statuses
        if (!await context.Vehicles.AnyAsync())
        {
            logger.LogInformation("Seeding Vehicles...");
            var vehicleConfigs = new[]
            {
                ("truck", 2000m, "available"),
                ("truck", 1500m, "in_transit"),
                ("van", 800m, "available"),
                ("van", 600m, "maintenance"),
                ("motorbike", 50m, "available"),
                ("motorbike", 50m, "inactive")
            };
            
            foreach (var company in companies)
            {
                int plateNum = 1;
                foreach (var (type, maxWeight, status) in vehicleConfigs)
                {
                    var currentWeight = status == "in_transit" ? maxWeight * 0.7m : 0;
                    await context.Vehicles.AddAsync(new Vehicle
                    {
                        CompanyId = company.CompanyId,
                        LicensePlate = $"51{(char)('A' + company.CompanyId - 1)}-{company.CompanyId:D2}{plateNum:D3}",
                        VehicleType = type,
                        MaxWeightKg = maxWeight,
                        CurrentWeightKg = currentWeight,
                        OverloadThreshold = 90,
                        CapacityPercentage = currentWeight / maxWeight * 100,
                        AllowOverload = false,
                        CurrentStatus = status,
                        GpsEnabled = true,
                        CreatedAt = DateTime.UtcNow
                    });
                    plateNum++;
                }
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded vehicles for {Count} companies", companies.Count);
        }

        // 5. Seed Routes (multiple routes per company)
        if (!await context.Routes.AnyAsync())
        {
            logger.LogInformation("Seeding Routes...");
            var routeConfigs = new[]
            {
                ("TP.HCM", "Lâm Đồng", "HCM - Đà Lạt", 310, 7.0m, 150000m),
                ("TP.HCM", "Khánh Hòa", "HCM - Nha Trang", 430, 8.0m, 180000m),
                ("TP.HCM", "Bình Thuận", "HCM - Phan Thiết", 200, 4.0m, 100000m),
                ("TP.HCM", "Bà Rịa - Vũng Tàu", "HCM - Vũng Tàu", 120, 2.5m, 80000m),
                ("TP.HCM", "Cần Thơ", "HCM - Cần Thơ", 170, 3.5m, 90000m),
                ("TP.HCM", "Đồng Nai", "HCM - Biên Hòa", 35, 1.0m, 50000m)
            };
            
            foreach (var company in companies)
            {
                foreach (var (origin, dest, name, distance, hours, price) in routeConfigs)
                {
                    // Slightly vary prices between companies
                    var priceMultiplier = company.CompanyId == 1 ? 1.0m : 0.95m;
                    await context.Routes.AddAsync(new Route
                    {
                        CompanyId = company.CompanyId,
                        RouteName = name,
                        OriginProvince = origin,
                        OriginDistrict = "Quận 1",
                        DestinationProvince = dest,
                        DestinationDistrict = "Trung tâm",
                        EstimatedDurationHours = hours,
                        DistanceKm = distance,
                        BasePrice = price * priceMultiplier,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded routes for {Count} companies", companies.Count);
        }

        // 6. Seed Sample Orders with various statuses
        if (!await context.Orders.AnyAsync())
        {
            logger.LogInformation("Seeding Orders...");
            var drivers = await context.Drivers.Where(d => d.IsActive == true).ToListAsync();
            var vehicles = await context.Vehicles.Where(v => v.CurrentStatus != "maintenance").ToListAsync();
            var routes = await context.Routes.ToListAsync();

            if (!customers.Any() || !drivers.Any() || !vehicles.Any() || !routes.Any())
            {
                logger.LogWarning("Skipping Orders seeding - missing required data");
                return;
            }

            var orderStatuses = new[] { "pending_pickup", "picked_up", "in_transit", "out_for_delivery", "delivered", "returned", "cancelled" };
            var parcelTypes = new[] { "electronics", "fragile", "food", "document", "other" };
            var paymentMethods = new[] { "cash", "bank_transfer", "e_wallet" };
            var receivers = new[]
            {
                ("Nguyễn Văn A", "0911111111", "456 Trần Phú, Đà Lạt", "Lâm Đồng"),
                ("Trần Thị B", "0922222222", "789 Trần Hưng Đạo, Nha Trang", "Khánh Hòa"),
                ("Lê Văn C", "0933333333", "123 Nguyễn Huệ, Phan Thiết", "Bình Thuận"),
                ("Phạm Thị D", "0944444444", "456 Bà Triệu, Vũng Tàu", "Bà Rịa - Vũng Tàu"),
                ("Hoàng Văn E", "0955555555", "789 Hùng Vương, Cần Thơ", "Cần Thơ"),
                ("Đỗ Thị F", "0966666666", "321 Lê Lợi, Biên Hòa", "Đồng Nai")
            };

            var random = new Random(42);
            var orderCount = 0;

            foreach (var company in companies)
            {
                var companyRoutes = routes.Where(r => r.CompanyId == company.CompanyId).ToList();
                var companyDrivers = drivers.Where(d => d.CompanyId == company.CompanyId).ToList();
                var companyVehicles = vehicles.Where(v => v.CompanyId == company.CompanyId).ToList();

                // 15 orders per company
                for (int i = 0; i < 15; i++)
                {
                    var customer = customers[random.Next(customers.Count)];
                    var route = companyRoutes[random.Next(companyRoutes.Count)];
                    var driver = companyDrivers[random.Next(companyDrivers.Count)];
                    var vehicle = companyVehicles[random.Next(companyVehicles.Count)];
                    var receiver = receivers[random.Next(receivers.Length)];
                    var status = orderStatuses[random.Next(orderStatuses.Length)];
                    var paymentStatus = status == "delivered" ? "paid" : (random.Next(2) == 0 ? "unpaid" : "paid");

                    var daysAgo = random.Next(0, 14);
                    var createdAt = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-random.Next(0, 12));

                    await context.Orders.AddAsync(new Order
                    {
                        TrackingCode = $"WDL{DateTime.UtcNow:yyyyMMdd}{company.CompanyId:D2}{i + 1:D3}",
                        CustomerId = customer.CustomerId,
                        RouteId = route.RouteId,
                        DriverId = status != "pending_pickup" ? driver.DriverId : null,
                        VehicleId = status != "pending_pickup" ? vehicle.VehicleId : null,
                        SenderName = customer.FullName,
                        SenderPhone = customer.Phone,
                        SenderAddress = "123 Nguyễn Huệ, Q1, TP.HCM",
                        ReceiverName = receiver.Item1,
                        ReceiverPhone = receiver.Item2,
                        ReceiverAddress = receiver.Item3,
                        ReceiverProvince = receiver.Item4,
                        ReceiverDistrict = "Trung tâm",
                        ParcelType = parcelTypes[random.Next(parcelTypes.Length)],
                        WeightKg = 0.5m + (decimal)(random.NextDouble() * 9.5),
                        DeclaredValue = random.Next(100000, 5000000),
                        ShippingFee = route.BasePrice ?? 100000m,
                        CodAmount = random.Next(3) == 0 ? 0 : random.Next(50000, 500000),
                        PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                        PaymentStatus = paymentStatus,
                        OrderStatus = status,
                        SpecialInstructions = random.Next(3) == 0 ? "Gọi trước khi giao" : null,
                        CreatedAt = createdAt,
                        UpdatedAt = createdAt.AddHours(random.Next(1, 24)),
                        PickupScheduledAt = createdAt.AddHours(2),
                        PickupConfirmedAt = status != "pending_pickup" ? createdAt.AddHours(3) : null,
                        DeliveredAt = status == "delivered" ? createdAt.AddDays(1) : null,
                        PaidAt = paymentStatus == "paid" ? createdAt.AddDays(1) : null
                    });
                    orderCount++;
                }
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} orders", orderCount);
        }

        // 7. Seed Company Customers (khách quen của từng công ty)
        if (!await context.CompanyCustomers.AnyAsync())
        {
            logger.LogInformation("Seeding Company Customers...");
            var random = new Random(123);
            var customerNames = new[]
            {
                ("Hoàng Thị Mai", "0900000006", "hoangmai@gmail.com"),
                ("Vũ Đức Anh", "0900000007", "vuducanh@gmail.com"),
                ("Lê Văn Hùng", "0900000008", "levanhung@gmail.com"),
                ("Nguyễn Thị Hoa", "0900000009", "nguyenhoa@gmail.com"),
                ("Trần Minh Tuấn", "0900000010", "tranminhtuan@gmail.com"),
                ("Phạm Quốc Bảo", "0911111001", "phamquocbao@gmail.com"),
                ("Đỗ Thị Lan", "0911111002", "dothilan@gmail.com"),
                ("Bùi Văn Nam", "0911111003", "buivannam@gmail.com")
            };

            int idx = 0;
            foreach (var company in companies)
            {
                // Create 5 customers per company
                for (int i = 0; i < 5; i++)
                {
                    var (name, phone, email) = customerNames[(idx + i) % customerNames.Length];
                    var isVip = i < 2; // First 2 customers are VIP
                    var hasDiscount = i % 2 == 0;
                    var totalOrders = random.Next(5, 50);
                    var totalRevenue = totalOrders * random.Next(80000, 300000);

                    await context.CompanyCustomers.AddAsync(new CompanyCustomer
                    {
                        CompanyId = company.CompanyId,
                        FullName = name,
                        Phone = $"{phone.Substring(0, 7)}{company.CompanyId}{i}",
                        Email = email?.Replace("@", $"{company.CompanyId}@"),
                        CustomPrice = hasDiscount ? random.Next(50000, 100000) : null,
                        DiscountPercent = hasDiscount ? random.Next(5, 20) : null,
                        IsVip = isVip,
                        TotalOrders = totalOrders,
                        TotalRevenue = totalRevenue,
                        Notes = isVip ? "Khách VIP - ưu tiên xử lý" : null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                idx++;
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded company customers for {Count} companies", companies.Count);
        }
    }

    #endregion
}