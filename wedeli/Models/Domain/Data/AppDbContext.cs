using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using wedeli.Models.Domain;
using Route = wedeli.Models.Domain.Route;

namespace wedeli.Models.Domain.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // ====================================
    // Company Database (Operational) Entities
    // Platform entities (User, Role, Customer, TransportCompany) are in PlatformDbContext
    // ====================================
    
    public virtual DbSet<CodTransaction> CodTransactions { get; set; }
    public virtual DbSet<CompanyPartnership> CompanyPartnerships { get; set; }
    public virtual DbSet<Complaint> Complaints { get; set; }
    public virtual DbSet<DailyActivityLog> DailyActivityLogs { get; set; }
    public virtual DbSet<DailySummary> DailySummaries { get; set; }
    public virtual DbSet<Driver> Drivers { get; set; }
    public virtual DbSet<DriverCodSummary> DriverCodSummaries { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderPhoto> OrderPhotos { get; set; }
    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public virtual DbSet<OrderTransfer> OrderTransfers { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<PeriodicInvoice> PeriodicInvoices { get; set; }
    public virtual DbSet<Rating> Ratings { get; set; }
    public virtual DbSet<Route> Routes { get; set; }
    public virtual DbSet<Trip> Trips { get; set; }
    public virtual DbSet<TripOrder> TripOrders { get; set; }
    public virtual DbSet<Vehicle> Vehicles { get; set; }
    public virtual DbSet<VehicleLocation> VehicleLocations { get; set; }
    public virtual DbSet<WarehouseStaff> WarehouseStaffs { get; set; }

    // Multi-tenant entities
    public virtual DbSet<CompanyRole> CompanyRoles { get; set; }
    public virtual DbSet<CompanyUser> CompanyUsers { get; set; }
    public virtual DbSet<CompanyCustomer> CompanyCustomers { get; set; }

    // Note: Connection string is configured via DI in Program.cs
    // Do NOT use OnConfiguring with hardcoded connection - it overrides DI settings
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseMySql(...)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =============================
        // IGNORE Platform entities - they are managed by PlatformDbContext
        // These entities should NOT be created in Company (wedeli_company) database
        // =============================
        modelBuilder.Ignore<User>();
        modelBuilder.Ignore<Role>();
        modelBuilder.Ignore<Customer>();
        modelBuilder.Ignore<CustomerAddress>();
        modelBuilder.Ignore<TransportCompany>();
        modelBuilder.Ignore<RefreshToken>();

        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CodTransaction>(entity =>
        {
            entity.HasKey(e => e.CodTransactionId).HasName("PRIMARY");

            entity.Property(e => e.AdjustmentAmount)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Điều chỉnh (nếu có)");
            entity.Property(e => e.CodAmount).HasComment("Số tiền thu hộ");
            entity.Property(e => e.CollectedByDriver).HasComment("Driver ID");
            entity.Property(e => e.CollectionProofPhoto).HasComment("Ảnh xác nhận thu tiền");
            entity.Property(e => e.CollectionStatus).HasDefaultValueSql("'pending'");
            entity.Property(e => e.CompanyFee)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Phí nhà xe trừ (nếu có)");
            entity.Property(e => e.CompanyReceivedBy).HasComment("Nhân viên nhà xe xác nhận nhận tiền");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.OverallStatus).HasDefaultValueSql("'pending_collection'");
            entity.Property(e => e.SubmittedAmount).HasComment("Số tiền thực nộp (có thể khác nếu có phí)");
            entity.Property(e => e.SubmittedToCompany).HasDefaultValueSql("'0'");
            entity.Property(e => e.TransferProof).HasComment("Ảnh/file chứng từ chuyển tiền");
            entity.Property(e => e.TransferredToSender).HasDefaultValueSql("'0'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.CollectedByDriverNavigation).WithMany(p => p.CodTransactions).HasConstraintName("cod_transactions_ibfk_2");

            // Cross-DB: User is in Platform DB
            // entity.HasOne(d => d.CompanyReceivedByNavigation).WithMany(p => p.CodTransactions).HasConstraintName("cod_transactions_ibfk_3");

            entity.HasOne(d => d.Order).WithMany(p => p.CodTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cod_transactions_ibfk_1");
        });

        modelBuilder.Entity<CompanyPartnership>(entity =>
        {
            entity.HasKey(e => e.PartnershipId).HasName("PRIMARY");

            entity.Property(e => e.CommissionRate)
                .HasDefaultValueSql("'0.00'")
                .HasComment("% hoa hồng khi chuyển hàng");
            entity.Property(e => e.CompanyId).HasComment("Nhà xe chính");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedBy).HasComment("Admin tạo quan hệ");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.PartnerCompanyId).HasComment("Nhà xe đối tác quen");
            entity.Property(e => e.PartnershipLevel).HasDefaultValueSql("'regular'");
            entity.Property(e => e.PriorityOrder)
                .HasDefaultValueSql("'0'")
                .HasComment("Thứ tự ưu tiên (số càng nhỏ càng ưu tiên)");
            entity.Property(e => e.TotalTransferredOrders).HasDefaultValueSql("'0'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Cross-DB relationships removed - CompanyId references Platform DB
            // entity.HasOne(d => d.Company)...
            // entity.HasOne(d => d.CreatedByNavigation)...
            // entity.HasOne(d => d.PartnerCompany)...
        });

        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId).HasName("PRIMARY");

            entity.Property(e => e.ComplaintStatus).HasDefaultValueSql("'pending'");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EvidencePhotos).HasComment("JSON array of photo URLs");

            // Cross-DB Customer relationship removed - Customer is in Platform DB
            // entity.HasOne(d => d.Customer).WithMany(p => p.Complaints)...

            entity.HasOne(d => d.Order).WithMany(p => p.Complaints)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("complaints_ibfk_1");

            // Cross-DB User relationship removed - User is in Platform DB
            // entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.Complaints)...
        });

        /* ignored platform entity
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreditLimit)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Hạn mức công nợ cho khách quen");
            entity.Property(e => e.IsRegular)
                .HasDefaultValueSql("'0'")
                .HasComment("Khách hàng quen thuộc");
            entity.Property(e => e.PaymentPrivilege).HasDefaultValueSql("'prepay'");
            entity.Property(e => e.TotalOrders).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalRevenue).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithMany(p => p.Customers).HasConstraintName("customers_ibfk_1");
        });
        */

        /* ignored platform entity
        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PRIMARY");

            entity.Property(e => e.AddressLabel).HasComment("Nhà riêng, Văn phòng, etc");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsDefault).HasDefaultValueSql("'0'");
            entity.Property(e => e.UsageCount)
                .HasDefaultValueSql("'0'")
                .HasComment("Số lần sử dụng địa chỉ này");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerAddresses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_addresses_ibfk_1");
        });
        */

        modelBuilder.Entity<DailyActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PRIMARY");

            entity.Property(e => e.Action).HasComment("created, updated, deleted, transferred, etc");
            entity.Property(e => e.ChangedBy).HasComment("User thực hiện thay đổi");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EntityId).HasComment("ID của entity");
            entity.Property(e => e.EntityType).HasComment("orders, vehicles, drivers, etc");
            entity.Property(e => e.NewValue).HasComment("Giá trị mới (JSON)");
            entity.Property(e => e.OldValue).HasComment("Giá trị cũ (JSON)");

            // Cross-DB: User is in Platform DB
            // entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.DailyActivityLogs).HasConstraintName("daily_activity_logs_ibfk_1");
        });

        modelBuilder.Entity<DailySummary>(entity =>
        {
            entity.HasKey(e => e.SummaryId).HasName("PRIMARY");

            entity.Property(e => e.AvgDeliveriesPerDriver).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.ComplaintsResolved).HasDefaultValueSql("'0'");
            entity.Property(e => e.GeneratedBy).HasDefaultValueSql("'system'");
            entity.Property(e => e.LastUpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PendingCodAmount).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalCodCollected).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalCodSubmitted).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalComplaints).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalDriversActive).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalOrdersCancelled).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalOrdersCreated).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalOrdersDelivered).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalOrdersTransferred).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalRevenue).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalTripsCompleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalVehiclesActive).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalVehiclesOverloaded).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.DriverId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.Rating).HasDefaultValueSql("'5.00'");
            entity.Property(e => e.SuccessRate).HasDefaultValueSql("'100.00'");
            entity.Property(e => e.TotalTrips).HasDefaultValueSql("'0'");

            // Cross-DB: CompanyId references Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.Drivers)...

            entity.HasOne(d => d.CompanyUser).WithMany(p => p.Drivers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("drivers_company_user_fk");
        });

        modelBuilder.Entity<DriverCodSummary>(entity =>
        {
            entity.HasKey(e => e.SummaryId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PendingAmount)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Số tiền chưa nộp");
            entity.Property(e => e.ReconciledBy).HasComment("Admin xác nhận đối soát");
            entity.Property(e => e.ReconciliationStatus).HasDefaultValueSql("'pending'");
            entity.Property(e => e.TotalCodCollected).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalCodSubmitted).HasDefaultValueSql("'0.00'");

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverCodSummaries)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("driver_cod_summary_ibfk_1");

            // Cross-DB: User is in Platform DB
            // entity.HasOne(d => d.ReconciledByNavigation).WithMany(p => p.DriverCodSummaries).HasConstraintName("driver_cod_summary_ibfk_2");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsRead).HasDefaultValueSql("'0'");
            entity.Property(e => e.SentVia).HasDefaultValueSql("'push'");

            entity.HasOne(d => d.Order).WithMany(p => p.Notifications).HasConstraintName("notifications_ibfk_2");

            // Cross-DB: User is in Platform DB
            // entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasConstraintName("notifications_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.Property(e => e.CodAmount)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Tiền thu hộ");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.OrderStatus).HasDefaultValueSql("'pending_pickup'");
            entity.Property(e => e.ParcelType).HasDefaultValueSql("'other'");
            entity.Property(e => e.PaymentStatus).HasDefaultValueSql("'unpaid'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Cross-DB: Customer is in Platform DB
            // entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("orders_ibfk_1");

            entity.HasOne(d => d.Driver).WithMany(p => p.Orders).HasConstraintName("orders_ibfk_4");

            entity.HasOne(d => d.Route).WithMany(p => p.Orders).HasConstraintName("orders_ibfk_2");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Orders).HasConstraintName("orders_ibfk_3");
        });

        modelBuilder.Entity<OrderPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PRIMARY");

            entity.Property(e => e.PhotoUrl).HasComment("URL từ cloud storage (AWS S3, Cloudinary, etc)");
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderPhotos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_photos_ibfk_1");

            // Cross-DB: User is in Platform DB
            // entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.OrderPhotos).HasConstraintName("order_photos_ibfk_2");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_status_history_ibfk_1");

            // Cross-DB: User is in Platform DB
            // entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.OrderStatusHistories).HasConstraintName("order_status_history_ibfk_2");
        });

        modelBuilder.Entity<OrderTransfer>(entity =>
        {
            entity.HasKey(e => e.TransferId).HasName("PRIMARY");

            entity.Property(e => e.CommissionPaid)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Hoa hồng trả cho đối tác");
            entity.Property(e => e.OriginalVehicleId).HasComment("Xe ban đầu bị đầy");
            entity.Property(e => e.TransferFee)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Phí chuyển đổi (nếu có)");
            entity.Property(e => e.TransferStatus).HasDefaultValueSql("'pending'");
            entity.Property(e => e.TransferredAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.TransferredBy).HasComment("Admin thực hiện chuyển");

            // Cross-DB: FromCompany and ToCompany reference TransportCompany in Platform DB
            // entity.HasOne(d => d.FromCompany).WithMany(p => p.OrderTransferFromCompanies)...

            entity.HasOne(d => d.NewVehicle).WithMany(p => p.OrderTransferNewVehicles).HasConstraintName("order_transfers_ibfk_5");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderTransfers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_transfers_ibfk_1");

            entity.HasOne(d => d.OriginalVehicle).WithMany(p => p.OrderTransferOriginalVehicles).HasConstraintName("order_transfers_ibfk_4");

            // Cross-DB: ToCompany references TransportCompany in Platform DB
            // entity.HasOne(d => d.ToCompany).WithMany(p => p.OrderTransferToCompanies)...

            // Cross-DB: TransferredByNavigation references User in Platform DB
            // entity.HasOne(d => d.TransferredByNavigation).WithMany(p => p.OrderTransfers)...
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PaymentStatus).HasDefaultValueSql("'pending'");

            // Cross-DB: Customer is in Platform DB
            // entity.HasOne(d => d.Customer).WithMany(p => p.Payments)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("payments_ibfk_2");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_ibfk_1");
        });

        modelBuilder.Entity<PeriodicInvoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PRIMARY");

            entity.Property(e => e.BillingCycle).HasComment("Chu kỳ do nhà xe quy định");
            entity.Property(e => e.CompanyId).HasComment("Nhà xe phát hành hóa đơn");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.InvoicePeriod).HasComment("YYYY-MM hoặc YYYY-Wxx hoặc custom range");
            entity.Property(e => e.InvoiceStatus).HasDefaultValueSql("'pending'");
            entity.Property(e => e.PaidAmount).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.PaymentTerms).HasComment("Điều khoản thanh toán riêng của nhà xe");

            // Cross-DB: Company and Customer are in Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.PeriodicInvoices)...
            // entity.HasOne(d => d.Customer).WithMany(p => p.PeriodicInvoices)...
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Cross-DB: Customer is in Platform DB
            // entity.HasOne(d => d.Customer).WithMany(p => p.Ratings)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("ratings_ibfk_2");

            entity.HasOne(d => d.Driver).WithMany(p => p.Ratings).HasConstraintName("ratings_ibfk_3");

            entity.HasOne(d => d.Order).WithMany(p => p.Ratings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ratings_ibfk_1");
        });

        /* ignored platform entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        */

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");

            // Cross-DB: CompanyId references Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.Routes)...
        });

        /* ignored platform entity
        modelBuilder.Entity<TransportCompany>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.Rating).HasDefaultValueSql("'5.00'");

            entity.HasOne(d => d.User)
                .WithOne(p => p.TransportCompany)
                .HasForeignKey<TransportCompany>(d => d.UserId)
                .HasConstraintName("transport_companies_ibfk_user");
        });
        */

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.TripId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsReturnTrip)
                .HasDefaultValueSql("'0'")
                .HasComment("Chuyến về (E2)");
            entity.Property(e => e.TotalOrders).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalWeightKg).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TripStatus).HasDefaultValueSql("'scheduled'");

            entity.HasOne(d => d.Driver).WithMany(p => p.Trips)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trips_ibfk_3");

            entity.HasOne(d => d.Route).WithMany(p => p.Trips)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trips_ibfk_1");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Trips)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trips_ibfk_2");
        });

        modelBuilder.Entity<TripOrder>(entity =>
        {
            entity.HasKey(e => e.TripOrderId).HasName("PRIMARY");

            entity.Property(e => e.DeliveryConfirmed).HasDefaultValueSql("'0'");
            entity.Property(e => e.PickupConfirmed).HasDefaultValueSql("'0'");
            entity.Property(e => e.SequenceNumber).HasComment("Thứ tự giao hàng");

            entity.HasOne(d => d.Order).WithMany(p => p.TripOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trip_orders_ibfk_2");

            entity.HasOne(d => d.Trip).WithMany(p => p.TripOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trip_orders_ibfk_1");
        });

        /* ignored platform entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_ibfk_1");
        });
        */

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PRIMARY");

            entity.Property(e => e.AllowOverload)
                .HasDefaultValueSql("'0'")
                .HasComment("Admin cho phép thêm hàng nhẹ");
            entity.Property(e => e.CapacityPercentage)
                .HasDefaultValueSql("'0.00'")
                .HasComment("% tải trọng");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CurrentStatus).HasDefaultValueSql("'available'");
            entity.Property(e => e.CurrentWeightKg)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Trọng tải hiện tại");
            entity.Property(e => e.GpsEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.OverloadThreshold)
                .HasDefaultValueSql("'95.00'")
                .HasComment("Ngưỡng cảnh báo đầy (%)");

            // Cross-DB: CompanyId references Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.Vehicles)...
        });

        modelBuilder.Entity<WarehouseStaff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");

            // Cross-DB: CompanyId references Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.WarehouseStaffs)...

            entity.HasOne(d => d.CompanyUser).WithMany(p => p.WarehouseStaffs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("warehouse_staff_company_user_fk");
        });

        // CompanyRole configuration
        modelBuilder.Entity<CompanyRole>(entity =>
        {
            entity.HasKey(e => e.CompanyRoleId).HasName("PRIMARY");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Cross-DB: CompanyId references Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.CompanyRoles)...
        });

        // CompanyUser configuration
        modelBuilder.Entity<CompanyUser>(entity =>
        {
            entity.HasKey(e => e.CompanyUserId).HasName("PRIMARY");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Cross-DB: CompanyId references Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.CompanyUsers)...

            entity.HasOne(d => d.CompanyRole).WithMany(p => p.CompanyUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("company_users_role_fk");
        });

        // CompanyCustomer configuration
        modelBuilder.Entity<CompanyCustomer>(entity =>
        {
            entity.HasKey(e => e.CompanyCustomerId).HasName("PRIMARY");
            entity.Property(e => e.IsVip).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalOrders).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalRevenue).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Cross-DB: Company and Customer are in Platform DB
            // entity.HasOne(d => d.Company).WithMany(p => p.CompanyCustomers)...
            // entity.HasOne(d => d.Customer).WithMany(p => p.CompanyCustomers)...
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
