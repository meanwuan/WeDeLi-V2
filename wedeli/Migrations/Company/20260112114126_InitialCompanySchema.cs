using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wedeli.Migrations.Company
{
    /// <inheritdoc />
    public partial class InitialCompanySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "company_customers",
                columns: table => new
                {
                    company_customer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    full_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    custom_price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    discount_percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_vip = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'0'"),
                    total_orders = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'0'"),
                    total_revenue = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false, defaultValueSql: "'0.00'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.company_customer_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "company_partnerships",
                columns: table => new
                {
                    partnership_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: false, comment: "Nhà xe chính"),
                    partner_company_id = table.Column<int>(type: "int", nullable: false, comment: "Nhà xe đối tác quen"),
                    partnership_level = table.Column<string>(type: "enum('preferred','regular','backup')", nullable: true, defaultValueSql: "'regular'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    commission_rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "% hoa hồng khi chuyển hàng"),
                    priority_order = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'", comment: "Thứ tự ưu tiên (số càng nhỏ càng ưu tiên)"),
                    total_transferred_orders = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<int>(type: "int", nullable: true, comment: "Admin tạo quan hệ"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.partnership_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "company_roles",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    role_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.company_role_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "daily_activity_logs",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    log_date = table.Column<DateOnly>(type: "date", nullable: false),
                    log_type = table.Column<string>(type: "enum('order','payment','transfer','vehicle','complaint','system')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, comment: "orders, vehicles, drivers, etc", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_id = table.Column<int>(type: "int", nullable: true, comment: "ID của entity"),
                    action = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "created, updated, deleted, transferred, etc", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    old_value = table.Column<string>(type: "text", nullable: true, comment: "Giá trị cũ (JSON)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    new_value = table.Column<string>(type: "text", nullable: true, comment: "Giá trị mới (JSON)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    changed_by = table.Column<int>(type: "int", nullable: true, comment: "User thực hiện thay đổi"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.log_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "daily_summary",
                columns: table => new
                {
                    summary_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    summary_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_orders_created = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_orders_delivered = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_orders_cancelled = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_orders_transferred = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_vehicles_active = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_vehicles_overloaded = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_trips_completed = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_drivers_active = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    avg_deliveries_per_driver = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    total_revenue = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    total_cod_collected = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    total_cod_submitted = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    pending_cod_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    total_complaints = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    complaints_resolved = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    last_updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    generated_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValueSql: "'system'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.summary_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "periodic_invoices",
                columns: table => new
                {
                    invoice_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false, comment: "Nhà xe phát hành hóa đơn"),
                    billing_cycle = table.Column<string>(type: "enum('weekly','biweekly','monthly','custom')", nullable: false, comment: "Chu kỳ do nhà xe quy định", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invoice_period = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, comment: "YYYY-MM hoặc YYYY-Wxx hoặc custom range", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    invoice_status = table.Column<string>(type: "enum('pending','paid','overdue','cancelled')", nullable: true, defaultValueSql: "'pending'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    payment_terms = table.Column<string>(type: "text", nullable: true, comment: "Điều khoản thanh toán riêng của nhà xe", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.invoice_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    route_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    route_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    origin_province = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    origin_district = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    destination_province = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    destination_district = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    distance_km = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    estimated_duration_hours = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    base_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.route_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    vehicle_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    license_plate = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vehicle_type = table.Column<string>(type: "enum('truck','van','motorbike')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    max_weight_kg = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    max_volume_m3 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    current_weight_kg = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "Trọng tải hiện tại"),
                    capacity_percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "% tải trọng"),
                    overload_threshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, defaultValueSql: "'95.00'", comment: "Ngưỡng cảnh báo đầy (%)"),
                    allow_overload = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "Admin cho phép thêm hàng nhẹ"),
                    current_status = table.Column<string>(type: "enum('available','in_transit','maintenance','inactive','overloaded')", nullable: true, defaultValueSql: "'available'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gps_enabled = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.vehicle_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "company_users",
                columns: table => new
                {
                    company_user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    company_role_id = table.Column<int>(type: "int", nullable: false),
                    username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    full_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.company_user_id);
                    table.ForeignKey(
                        name: "company_users_role_fk",
                        column: x => x.company_role_id,
                        principalTable: "company_roles",
                        principalColumn: "company_role_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "vehicle_locations",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    speed = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    heading = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    accuracy = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    recorded_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_locations", x => x.location_id);
                    table.ForeignKey(
                        name: "FK_vehicle_locations_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "vehicle_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "drivers",
                columns: table => new
                {
                    driver_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_user_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    driver_license = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    license_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    total_trips = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    success_rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, defaultValueSql: "'100.00'"),
                    rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true, defaultValueSql: "'5.00'"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.driver_id);
                    table.ForeignKey(
                        name: "drivers_company_user_fk",
                        column: x => x.company_user_id,
                        principalTable: "company_users",
                        principalColumn: "company_user_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "warehouse_staff",
                columns: table => new
                {
                    staff_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_user_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    warehouse_location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.staff_id);
                    table.ForeignKey(
                        name: "warehouse_staff_company_user_fk",
                        column: x => x.company_user_id,
                        principalTable: "company_users",
                        principalColumn: "company_user_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "driver_cod_summary",
                columns: table => new
                {
                    summary_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    driver_id = table.Column<int>(type: "int", nullable: false),
                    summary_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_cod_collected = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    total_cod_submitted = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    pending_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "Số tiền chưa nộp"),
                    reconciliation_status = table.Column<string>(type: "enum('pending','reconciled','discrepancy')", nullable: true, defaultValueSql: "'pending'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reconciled_by = table.Column<int>(type: "int", nullable: true, comment: "Admin xác nhận đối soát"),
                    reconciled_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.summary_id);
                    table.ForeignKey(
                        name: "driver_cod_summary_ibfk_1",
                        column: x => x.driver_id,
                        principalTable: "drivers",
                        principalColumn: "driver_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tracking_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    sender_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sender_phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sender_address = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receiver_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receiver_phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receiver_address = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receiver_province = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receiver_district = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parcel_type = table.Column<string>(type: "enum('fragile','electronics','food','cold','document','other')", nullable: true, defaultValueSql: "'other'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    weight_kg = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    declared_value = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    special_instructions = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    route_id = table.Column<int>(type: "int", nullable: true),
                    vehicle_id = table.Column<int>(type: "int", nullable: true),
                    driver_id = table.Column<int>(type: "int", nullable: true),
                    shipping_fee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    cod_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "Tiền thu hộ"),
                    payment_method = table.Column<string>(type: "enum('cash','bank_transfer','e_wallet','periodic')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_status = table.Column<string>(type: "enum('unpaid','paid','pending')", nullable: true, defaultValueSql: "'unpaid'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    paid_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    order_status = table.Column<string>(type: "enum('pending_pickup','picked_up','in_transit','out_for_delivery','delivered','returned','cancelled')", nullable: true, defaultValueSql: "'pending_pickup'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    pickup_scheduled_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    pickup_confirmed_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.order_id);
                    table.ForeignKey(
                        name: "orders_ibfk_2",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "route_id");
                    table.ForeignKey(
                        name: "orders_ibfk_3",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "vehicle_id");
                    table.ForeignKey(
                        name: "orders_ibfk_4",
                        column: x => x.driver_id,
                        principalTable: "drivers",
                        principalColumn: "driver_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    trip_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    route_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    driver_id = table.Column<int>(type: "int", nullable: false),
                    trip_date = table.Column<DateOnly>(type: "date", nullable: false),
                    departure_time = table.Column<DateTime>(type: "timestamp", nullable: true),
                    arrival_time = table.Column<DateTime>(type: "timestamp", nullable: true),
                    trip_status = table.Column<string>(type: "enum('scheduled','in_progress','completed','cancelled')", nullable: true, defaultValueSql: "'scheduled'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_orders = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    total_weight_kg = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    is_return_trip = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "Chuyến về (E2)"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.trip_id);
                    table.ForeignKey(
                        name: "trips_ibfk_1",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "route_id");
                    table.ForeignKey(
                        name: "trips_ibfk_2",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "vehicle_id");
                    table.ForeignKey(
                        name: "trips_ibfk_3",
                        column: x => x.driver_id,
                        principalTable: "drivers",
                        principalColumn: "driver_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "cod_transactions",
                columns: table => new
                {
                    cod_transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    cod_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false, comment: "Số tiền thu hộ"),
                    collected_by_driver = table.Column<int>(type: "int", nullable: true, comment: "Driver ID"),
                    collected_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    collection_status = table.Column<string>(type: "enum('pending','collected','failed')", nullable: true, defaultValueSql: "'pending'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    collection_proof_photo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Ảnh xác nhận thu tiền", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_to_company = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    submitted_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    submitted_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, comment: "Số tiền thực nộp (có thể khác nếu có phí)"),
                    company_received_by = table.Column<int>(type: "int", nullable: true, comment: "Nhân viên nhà xe xác nhận nhận tiền"),
                    transferred_to_sender = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    transferred_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    transfer_method = table.Column<string>(type: "enum('cash','bank_transfer','e_wallet')", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    transfer_reference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    transfer_proof = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Ảnh/file chứng từ chuyển tiền", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    company_fee = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "Phí nhà xe trừ (nếu có)"),
                    adjustment_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "Điều chỉnh (nếu có)"),
                    adjustment_reason = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    overall_status = table.Column<string>(type: "enum('pending_collection','collected','submitted_to_company','completed','failed')", nullable: true, defaultValueSql: "'pending_collection'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.cod_transaction_id);
                    table.ForeignKey(
                        name: "cod_transactions_ibfk_1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "cod_transactions_ibfk_2",
                        column: x => x.collected_by_driver,
                        principalTable: "drivers",
                        principalColumn: "driver_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "complaints",
                columns: table => new
                {
                    complaint_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    complaint_type = table.Column<string>(type: "enum('lost','damaged','late','wrong_address','other')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    evidence_photos = table.Column<string>(type: "text", nullable: true, comment: "JSON array of photo URLs", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    complaint_status = table.Column<string>(type: "enum('pending','investigating','resolved','rejected')", nullable: true, defaultValueSql: "'pending'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resolution_notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resolved_by = table.Column<int>(type: "int", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.complaint_id);
                    table.ForeignKey(
                        name: "complaints_ibfk_1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    notification_type = table.Column<string>(type: "enum('order_status','payment','promotion','system')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    sent_via = table.Column<string>(type: "enum('sms','email','push','all')", nullable: true, defaultValueSql: "'push'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.notification_id);
                    table.ForeignKey(
                        name: "notifications_ibfk_2",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "order_photos",
                columns: table => new
                {
                    photo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    photo_type = table.Column<string>(type: "enum('before_delivery','after_delivery','parcel_condition','signature','damage_proof')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    photo_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "URL từ cloud storage (AWS S3, Cloudinary, etc)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size_kb = table.Column<int>(type: "int", nullable: true),
                    uploaded_by = table.Column<int>(type: "int", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.photo_id);
                    table.ForeignKey(
                        name: "order_photos_ibfk_1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "order_status_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    old_status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    new_status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.history_id);
                    table.ForeignKey(
                        name: "order_status_history_ibfk_1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "order_transfers",
                columns: table => new
                {
                    transfer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    from_company_id = table.Column<int>(type: "int", nullable: false),
                    to_company_id = table.Column<int>(type: "int", nullable: false),
                    transfer_reason = table.Column<string>(type: "enum('vehicle_full','route_unavailable','emergency','partnership','other')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    original_vehicle_id = table.Column<int>(type: "int", nullable: true, comment: "Xe ban đầu bị đầy"),
                    new_vehicle_id = table.Column<int>(type: "int", nullable: true),
                    transferred_by = table.Column<int>(type: "int", nullable: false, comment: "Admin thực hiện chuyển"),
                    transfer_fee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "Phí chuyển đổi (nếu có)"),
                    commission_paid = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true, defaultValueSql: "'0.00'", comment: "Hoa hồng trả cho đối tác"),
                    admin_notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    transfer_status = table.Column<string>(type: "enum('pending','accepted','rejected','completed')", nullable: true, defaultValueSql: "'pending'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    transferred_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    accepted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.transfer_id);
                    table.ForeignKey(
                        name: "order_transfers_ibfk_1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "order_transfers_ibfk_4",
                        column: x => x.original_vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "vehicle_id");
                    table.ForeignKey(
                        name: "order_transfers_ibfk_5",
                        column: x => x.new_vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "vehicle_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "enum('cash','bank_transfer','e_wallet')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_type = table.Column<string>(type: "enum('shipping_fee','cod_collection','refund')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_status = table.Column<string>(type: "enum('pending','completed','failed')", nullable: true, defaultValueSql: "'pending'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    transaction_ref = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    paid_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.payment_id);
                    table.ForeignKey(
                        name: "payments_ibfk_1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ratings",
                columns: table => new
                {
                    rating_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    driver_id = table.Column<int>(type: "int", nullable: true),
                    rating_score = table.Column<int>(type: "int", nullable: true),
                    review_text = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.rating_id);
                    table.ForeignKey(
                        name: "ratings_ibfk_1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "ratings_ibfk_3",
                        column: x => x.driver_id,
                        principalTable: "drivers",
                        principalColumn: "driver_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "trip_orders",
                columns: table => new
                {
                    trip_order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    trip_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    sequence_number = table.Column<int>(type: "int", nullable: true, comment: "Thứ tự giao hàng"),
                    pickup_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    delivery_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.trip_order_id);
                    table.ForeignKey(
                        name: "trip_orders_ibfk_1",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "trip_id");
                    table.ForeignKey(
                        name: "trip_orders_ibfk_2",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "collected_by_driver",
                table: "cod_transactions",
                column: "collected_by_driver");

            migrationBuilder.CreateIndex(
                name: "company_received_by",
                table: "cod_transactions",
                column: "company_received_by");

            migrationBuilder.CreateIndex(
                name: "order_id",
                table: "cod_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_company_customer_phone",
                table: "company_customers",
                columns: new[] { "company_id", "phone" });

            migrationBuilder.CreateIndex(
                name: "idx_company_customer_unique",
                table: "company_customers",
                columns: new[] { "company_id", "customer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "created_by",
                table: "company_partnerships",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "partner_company_id",
                table: "company_partnerships",
                column: "partner_company_id");

            migrationBuilder.CreateIndex(
                name: "unique_partnership",
                table: "company_partnerships",
                columns: new[] { "company_id", "partner_company_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_company_role_unique",
                table: "company_roles",
                columns: new[] { "company_id", "role_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_company_users_company",
                table: "company_users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "idx_company_users_role",
                table: "company_users",
                column: "company_role_id");

            migrationBuilder.CreateIndex(
                name: "idx_company_users_username",
                table: "company_users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "customer_id",
                table: "complaints",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "order_id1",
                table: "complaints",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "resolved_by",
                table: "complaints",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "changed_by",
                table: "daily_activity_logs",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "idx_entity",
                table: "daily_activity_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "idx_log_date",
                table: "daily_activity_logs",
                column: "log_date");

            migrationBuilder.CreateIndex(
                name: "idx_log_type",
                table: "daily_activity_logs",
                column: "log_type");

            migrationBuilder.CreateIndex(
                name: "idx_summary_date",
                table: "daily_summary",
                column: "summary_date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "reconciled_by",
                table: "driver_cod_summary",
                column: "reconciled_by");

            migrationBuilder.CreateIndex(
                name: "unique_driver_date",
                table: "driver_cod_summary",
                columns: new[] { "driver_id", "summary_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "company_id",
                table: "drivers",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "company_user_id",
                table: "drivers",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "order_id2",
                table: "notifications",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "order_id3",
                table: "order_photos",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "uploaded_by",
                table: "order_photos",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "order_id4",
                table: "order_status_history",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "updated_by",
                table: "order_status_history",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "from_company_id",
                table: "order_transfers",
                column: "from_company_id");

            migrationBuilder.CreateIndex(
                name: "new_vehicle_id",
                table: "order_transfers",
                column: "new_vehicle_id");

            migrationBuilder.CreateIndex(
                name: "order_id5",
                table: "order_transfers",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "original_vehicle_id",
                table: "order_transfers",
                column: "original_vehicle_id");

            migrationBuilder.CreateIndex(
                name: "to_company_id",
                table: "order_transfers",
                column: "to_company_id");

            migrationBuilder.CreateIndex(
                name: "transferred_by",
                table: "order_transfers",
                column: "transferred_by");

            migrationBuilder.CreateIndex(
                name: "driver_id",
                table: "orders",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_orders_customer",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_status",
                table: "orders",
                column: "order_status");

            migrationBuilder.CreateIndex(
                name: "idx_orders_tracking",
                table: "orders",
                column: "tracking_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "route_id",
                table: "orders",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "vehicle_id",
                table: "orders",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "customer_id1",
                table: "payments",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_status",
                table: "payments",
                column: "payment_status");

            migrationBuilder.CreateIndex(
                name: "order_id6",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "company_id1",
                table: "periodic_invoices",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "customer_id2",
                table: "periodic_invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "customer_id3",
                table: "ratings",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "driver_id1",
                table: "ratings",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "order_id7",
                table: "ratings",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "company_id2",
                table: "routes",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "order_id8",
                table: "trip_orders",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "trip_id",
                table: "trip_orders",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "driver_id2",
                table: "trips",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "idx_trips_date",
                table: "trips",
                columns: new[] { "trip_date", "trip_status" });

            migrationBuilder.CreateIndex(
                name: "route_id1",
                table: "trips",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "vehicle_id1",
                table: "trips",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_locations_recorded_at",
                table: "vehicle_locations",
                column: "recorded_at");

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_locations_vehicle_id",
                table: "vehicle_locations",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "company_id3",
                table: "vehicles",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "license_plate",
                table: "vehicles",
                column: "license_plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "company_id4",
                table: "warehouse_staff",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "company_user_id1",
                table: "warehouse_staff",
                column: "company_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cod_transactions");

            migrationBuilder.DropTable(
                name: "company_customers");

            migrationBuilder.DropTable(
                name: "company_partnerships");

            migrationBuilder.DropTable(
                name: "complaints");

            migrationBuilder.DropTable(
                name: "daily_activity_logs");

            migrationBuilder.DropTable(
                name: "daily_summary");

            migrationBuilder.DropTable(
                name: "driver_cod_summary");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "order_photos");

            migrationBuilder.DropTable(
                name: "order_status_history");

            migrationBuilder.DropTable(
                name: "order_transfers");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "periodic_invoices");

            migrationBuilder.DropTable(
                name: "ratings");

            migrationBuilder.DropTable(
                name: "trip_orders");

            migrationBuilder.DropTable(
                name: "vehicle_locations");

            migrationBuilder.DropTable(
                name: "warehouse_staff");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "drivers");

            migrationBuilder.DropTable(
                name: "company_users");

            migrationBuilder.DropTable(
                name: "company_roles");
        }
    }
}
