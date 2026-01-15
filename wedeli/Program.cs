using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
// Removed: using wedeli.Data; - namespace does not exist
using wedeli.Infrastructure;
using wedeli.Middleware;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories;
using wedeli.Repositories.Implementation;
using wedeli.Repositories.Interface;
using wedeli.Repositories.Repo;
using wedeli.Service.Implementation;
using wedeli.Service.Interface;
using wedeli.Service.Service;
using wedeli.Hubs;

// ============================================
// BƯỚC 1: Khởi tạo WebApplicationBuilder
// ============================================

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================
// BƯỚC 2: Add Services to Container
// ============================================

// 2.1 Database Contexts - Dual Database Architecture
// Platform Database (Master): Users, Roles, Customers, TransportCompanies
var platformConnectionString = builder.Configuration.GetConnectionString("PlatformConnection");
builder.Services.AddDbContext<PlatformDbContext>(options =>
{
    options.UseMySql(
        platformConnectionString,
        new MySqlServerVersion(new Version(8, 0, 40)), // Explicit version for Aiven MySQL
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
            mysqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            mysqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Platform");
        }
    );
});

// Company Database (Operational): Drivers, Orders, Trips, Vehicles, etc.
var companyConnectionString = builder.Configuration.GetConnectionString("CompanyConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        companyConnectionString,
        new MySqlServerVersion(new Version(8, 0, 40)), // Explicit version for Aiven MySQL
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
            mysqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }
    );
});

// 2.2 Repository Pattern 
//builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Đăng ký các Repositories cụ thể
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IWarehouseStaffRepository, WarehouseStaffRepository>();
builder.Services.AddScoped<ITransportCompanyRepository, TransportCompanyRepository>();
builder.Services.AddScoped<ICODTransactionRepository, CodTransactionRepository>();
builder.Services.AddScoped<IOrderPhotoRepository, OrderPhotoRepository>();
builder.Services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
builder.Services.AddScoped<ICompanyPartnershipRepository, CompanyPartnershipRepository>();
builder.Services.AddScoped<IOrderTransferRepository, OrderTransferRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
builder.Services.AddScoped<ITripOrderRepository, TripOrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPeriodicInvoiceRepository, PeriodicInvoiceRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IVehicleLocationRepository, VehicleLocationRepository>();
builder.Services.AddScoped<ICompanyCustomerRepository, CompanyCustomerRepository>();

// 2.3 Business Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderPhotoService, OrderPhotoService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IWarehouseStaffService, WarehouseService>();
builder.Services.AddScoped<ICODService, CodService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPartnershipService, PartnershipService>();
builder.Services.AddScoped<IOrderTransferService, OrderTransferService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICompanyCustomerService, CompanyCustomerService>();
builder.Services.AddScoped<ITransportCompanyService, CompanyService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVehicleLocationService, VehicleLocationService>();

// 2.4 Infrastructure Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ISmsService, SmsService>();

// 2.4.1 Google Maps Geocoding Service
builder.Services.AddHttpClient<IGeocodingService, GeocodingService>();

// 2.5 AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 2.6 FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 2.7 JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Cho phép JWT qua SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CompanyAdminOnly", policy => policy.RequireRole("CompanyAdmin"));
    options.AddPolicy("DriverOnly", policy => policy.RequireRole("Driver"));
    options.AddPolicy("WarehouseOnly", policy => policy.RequireRole("WarehouseStaff"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("AdminOrDriver", policy => policy.RequireRole("CompanyAdmin", "Driver"));
    options.AddPolicy("AdminOrWarehouse", policy => policy.RequireRole("CompanyAdmin", "WarehouseStaff"));
});

// 2.8 CORS
var corsSettings = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings["PolicyName"], policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 2.9 Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WeDeli API",
        Version = "v1",
        Description = "API for WeDeli Delivery Management System",
        Contact = new OpenApiContact
        {
            Name = "WeDeli Team",
            Email = "support@wedeli.com"
        }
    });

    // JWT Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 2.10 SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// 2.11 Redis Cache (Optional)
//var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
//if (!string.IsNullOrEmpty(redisConnection))
//{
//    builder.Services.AddStackExchangeRedisCache(options =>
//    {
//        options.Configuration = redisConnection;
//        options.InstanceName = "WeDeli_";
//    });
//}
//else
//{
//    // Fallback to Memory Cache
//   
//}
builder.Services.AddMemoryCache();
// 2.12 Response Caching
builder.Services.AddResponseCaching();

// 2.13 Controllers & API Behavior
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = false;
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Messages = e.Value.Errors.Select(x => x.ErrorMessage).ToArray()
                })
                .ToList();

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            });
        };
    });

// 2.14 HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

//// 2.15 Health Checks
//builder.Services.AddHealthChecks()
//    .AddDbContextCheck<AppDbContext>();

// ============================================
// BƯỚC 3: Build Application
// ============================================
var app = builder.Build();

// ============================================
// BƯỚC 4: Configure Middleware Pipeline
// ============================================

// 1. HTTPS Redirection
app.UseHttpsRedirection();

// 2. Serilog Request Logging
app.UseSerilogRequestLogging();

// 3. Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WeDeli API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "WeDeli API Documentation";
    });
}

// 4. Static Files
app.UseStaticFiles();

// 5. CORS
app.UseCors(corsSettings["PolicyName"]);

// 6. Response Caching
app.UseResponseCaching();

// 7. Authentication
app.UseAuthentication();

// 8. Authorization
app.UseAuthorization();

// 9. Custom Error Handling Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// 10. Map Controllers
app.MapControllers();

// 11. SignalR Hubs
app.MapHub<VehicleTrackingHub>("/hubs/vehicle-tracking");
//app.MapHub<NotificationHub>("/hubs/notification");

// 12. Health Checks
//app.MapHealthChecks("/health");

// ============================================
//BƯỚC 5: Database Migration & Seeding
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var platformContext = services.GetRequiredService<PlatformDbContext>();
        var companyContext = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database migration...");

        // Apply pending migrations for Platform database
        if (platformContext.Database.GetPendingMigrations().Any())
        {
            platformContext.Database.Migrate();
            logger.LogInformation("Platform database migration completed successfully.");
        }
        else
        {
            logger.LogInformation("Platform database is already up to date.");
        }

        // Apply pending migrations for Company database
        if (companyContext.Database.GetPendingMigrations().Any())
        {
            companyContext.Database.Migrate();
            logger.LogInformation("Company database migration completed successfully.");
        }
        else
        {
            logger.LogInformation("Company database is already up to date.");
        }

        // Seed initial data to both databases
        var passwordService = services.GetRequiredService<IPasswordService>();
        await DbInitializer.SeedAsync(platformContext, companyContext, passwordService, logger);
        logger.LogInformation("Database migration and seeding completed.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
        throw;
    }
}

// ============================================
// BƯỚC 6: Run Application
// ============================================
try
{
    Log.Information("Starting WeDeli API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}