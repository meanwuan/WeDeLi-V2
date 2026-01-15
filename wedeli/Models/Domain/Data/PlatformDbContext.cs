using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain.Data;

/// <summary>
/// Platform (Master) Database Context
/// Contains: Users, Roles, Customers, TransportCompanies, RefreshTokens
/// </summary>
public partial class PlatformDbContext : DbContext
{
    public PlatformDbContext()
    {
    }

    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    // Platform (Master) entities
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public virtual DbSet<TransportCompany> TransportCompanies { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =============================
        // IGNORE Company entities - they are managed by AppDbContext
        // These entities should NOT be created in Platform (wedeli_platform) database
        // This prevents EF from auto-creating them via navigation properties
        // =============================
        modelBuilder.Ignore<CodTransaction>();
        modelBuilder.Ignore<CompanyPartnership>();
        modelBuilder.Ignore<Complaint>();
        modelBuilder.Ignore<DailyActivityLog>();
        modelBuilder.Ignore<DailySummary>();
        modelBuilder.Ignore<Driver>();
        modelBuilder.Ignore<DriverCodSummary>();
        modelBuilder.Ignore<Notification>();
        modelBuilder.Ignore<Order>();
        modelBuilder.Ignore<OrderPhoto>();
        modelBuilder.Ignore<OrderStatusHistory>();
        modelBuilder.Ignore<OrderTransfer>();
        modelBuilder.Ignore<Payment>();
        modelBuilder.Ignore<PeriodicInvoice>();
        modelBuilder.Ignore<Rating>();
        modelBuilder.Ignore<Route>();
        modelBuilder.Ignore<Trip>();
        modelBuilder.Ignore<TripOrder>();
        modelBuilder.Ignore<Vehicle>();
        modelBuilder.Ignore<VehicleLocation>();
        modelBuilder.Ignore<WarehouseStaff>();
        modelBuilder.Ignore<CompanyRole>();
        modelBuilder.Ignore<CompanyUser>();
        modelBuilder.Ignore<CompanyCustomer>();

        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        // Role entity configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");
            entity.ToTable("roles");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
        });

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
            entity.ToTable("users");

            entity.HasIndex(e => e.RoleId, "role_id");
            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_ibfk_1");
        });

        // Customer entity configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PRIMARY");
            entity.ToTable("customers");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsRegular)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_regular");
            entity.Property(e => e.TotalOrders)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_orders");
            entity.Property(e => e.TotalRevenue)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("total_revenue");
            entity.Property(e => e.PaymentPrivilege)
                .HasDefaultValueSql("'standard'")
                .HasColumnType("enum('standard','credit','periodic')")
                .HasColumnName("payment_privilege");
            entity.Property(e => e.CreditLimit)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("credit_limit");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.User).WithMany(p => p.Customers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("customers_ibfk_1");
        });

        // CustomerAddress entity configuration
        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PRIMARY");
            entity.ToTable("customer_addresses");

            entity.HasIndex(e => e.CustomerId, "customer_id");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.AddressLabel)
                .HasMaxLength(100)
                .HasColumnName("address_label");
            entity.Property(e => e.FullAddress)
                .HasMaxLength(500)
                .HasColumnName("full_address");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .HasColumnName("province");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasColumnName("ward");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8)
                .HasColumnName("longitude");
            entity.Property(e => e.IsDefault)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_default");
            entity.Property(e => e.UsageCount)
                .HasColumnName("usage_count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerAddresses)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("customer_addresses_ibfk_1");
        });

        // TransportCompany entity configuration
        modelBuilder.Entity<TransportCompany>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PRIMARY");
            entity.ToTable("transport_companies");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(200)
                .HasColumnName("company_name");
            entity.Property(e => e.BusinessLicense)
                .HasMaxLength(100)
                .HasColumnName("business_license");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Rating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("rating");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8)
                .HasColumnName("longitude");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.TransportCompany)
                .HasForeignKey<TransportCompany>(d => d.UserId)
                .HasConstraintName("transport_companies_ibfk_1");
        });

        // RefreshToken entity configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PRIMARY");
            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.UserId, "user_id");
            entity.HasIndex(e => e.Token, "token").IsUnique();

            entity.Property(e => e.RefreshTokenId).HasColumnName("id");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRevoked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_revoked");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refresh_tokens_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
