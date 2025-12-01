//using Microsoft.EntityFrameworkCore;
//using wedeli.Models.Domain;
//using wedeli.Models.Domain.Data;

//namespace wedeli.Data
//{
//    public static class DbInitializer
//    {
//        public static void Initialize(AppDbContext context)
//        {
//            // Đảm bảo database được tạo
//            context.Database.EnsureCreated();

//            // Kiểm tra nếu đã có data thì không seed nữa
//            if (context.Users.Any())
//            {
//                return; // DB đã được seed
//            }

//            // Seed Users
//            var users = new[]
//            {
//                new User
//                {
//                    Username = "admin",
//                    PasswordHash = "hashed_password", // Nên hash password thật
//                    Email = "admin@wedeli.com",
//                    FullName = "Admin User",
//                    PhoneNumber = "0123456789",
//                    Role = "admin",
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow
//                },
//                new User
//                {
//                    Username = "customer1",
//                    PasswordHash = "hashed_password",
//                    Email = "customer1@example.com",
//                    FullName = "Customer One",
//                    PhoneNumber = "0987654321",
//                    Role = "customer",
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow
//                }
//            };
//            context.Users.AddRange(users);
//            context.SaveChanges();

//            // Seed Transport Companies
//            var companies = new[]
//            {
//                new TransportCompany
//                {
//                    CompanyName = "Express Delivery Co.",
//                    Phone = "0901234567",
//                    Email = "contact@express.com",
//                    Address = "123 Main St, HCMC",
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow
//                }
//            };
//            context.TransportCompanies.AddRange(companies);
//            context.SaveChanges();

//            // Seed Customers
//            var customers = new[]
//            {
//                new Customer
//                {
//                    UserId = users[1].UserId,
//                    FullName = "Customer One",
//                    Phone = "0987654321",
//                    Email = "customer1@example.com",
//                    IsRegular = false,
//                    PaymentPrivilege = "prepay",
//                    TotalOrders = 0,
//                    TotalRevenue = 0,
//                    CreatedAt = DateTime.UtcNow
//                }
//            };
//            context.Customers.AddRange(customers);
//            context.SaveChanges();

//            // Có thể thêm seed data cho các bảng khác
//        }

//        // Phương thức async nếu cần
//        public static async Task InitializeAsync(AppDbContext context)
//        {
//            await context.Database.EnsureCreatedAsync();

//            if (await context.Users.AnyAsync())
//            {
//                return;
//            }

//            // Thêm seed data tương tự như trên
//            await context.SaveChangesAsync();
//        }
//    }
//}