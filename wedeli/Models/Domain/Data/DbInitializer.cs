using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;

namespace wedeli.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Seeds the database with initial data
        /// </summary>
        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed Roles
                if (!await context.Roles.AnyAsync())
                {
                    logger.LogInformation("Seeding Roles...");

                    var roles = new[]
                    {
                        new Role
                        {
                            RoleId = 1,
                            RoleName = "SuperAdmin",
                            Description = "Super Administrator with full system access",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Role
                        {
                            RoleId = 2,
                            RoleName = "CompanyAdmin",
                            Description = "Company Administrator managing transport company operations",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Role
                        {
                            RoleId = 3,
                            RoleName = "WarehouseStaff",
                            Description = "Warehouse staff managing inventory and packages",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Role
                        {
                            RoleId = 4,
                            RoleName = "Driver",
                            Description = "Driver delivering packages",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Role
                        {
                            RoleId = 5,
                            RoleName = "Customer",
                            Description = "Customer placing orders",
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Roles.AddRangeAsync(roles);
                    await context.SaveChangesAsync();

                    logger.LogInformation($"Seeded {roles.Length} roles successfully.");
                }
                else
                {
                    logger.LogInformation("Roles table already contains data. Skipping role seeding.");
                }

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        /// <summary>
        /// Initialize database synchronously
        /// </summary>
        public static void Initialize(AppDbContext context, ILogger logger)
        {
            SeedAsync(context, logger).GetAwaiter().GetResult();
        }
    }
}