# WeDeLi - Delivery Management System
## Project Structure & Architecture Guide

---

## 📌 PROJECT OVERVIEW

**WeDeLi** is an ASP.NET Core 8 backend API for a Delivery Management System that handles:
- User authentication & authorization with JWT
- Order management (CRUD operations)
- Driver and customer management
- Route & trip management
- COD (Cash on Delivery) transactions
- Payment processing
- Ratings and complaints

**Tech Stack:**
- **Framework**: ASP.NET Core 8
- **Database**: MySQL 8.0+
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT Bearer Tokens
- **Logging**: Serilog
- **Validation**: Fluent Validation
- **API Documentation**: Swagger/OpenAPI
- **Mapping**: AutoMapper

---

## 📁 PROJECT STRUCTURE

```
wedeli/
├── Controllers/              # API Endpoints
│   ├── AuthController.cs    # Authentication endpoints (login, register, etc.)
│   └── OrdersController.cs  # Order management endpoints (TODO)
│
├── Models/
│   ├── Domain/              # Database entities (EF Core)
│   │   ├── User.cs          # User model
│   │   ├── Order.cs         # Order model
│   │   ├── Customer.cs      # Customer model
│   │   ├── Driver.cs        # Driver model
│   │   ├── Vehicle.cs       # Vehicle model
│   │   ├── Route.cs         # Route model
│   │   ├── Trip.cs          # Trip/Shipment model
│   │   ├── Payment.cs       # Payment model
│   │   ├── CodTransaction.cs # Cash on Delivery transaction
│   │   ├── Rating.cs        # Rating model
│   │   ├── Complaint.cs     # Complaint model
│   │   ├── TransportCompany.cs # Company model
│   │   ├── WarehouseStaff.cs   # Warehouse staff model
│   │   ├── Notification.cs  # Notification model
│   │   ├── RefreshToken.cs  # JWT refresh token
│   │   ├── Role.cs          # User roles
│   │   ├── Data/
│   │   │   └── AppDbContext.cs # EF Core DbContext (all entities mapped)
│   │   └── [other models]
│   │
│   └── DTO/                 # Data Transfer Objects
│       ├── ApiResponse.cs          # Generic API response wrapper
│       ├── LoginRequest/Response    # Auth DTOs
│       ├── RegisterRequest          # Registration DTO
│       ├── ChangePasswordRequest    # Password change DTO
│       ├── ForgotPasswordRequest    # Forgot password DTO
│       ├── ResetPasswordRequest     # Password reset DTO
│       ├── UserResponse             # User info DTO
│       ├── CurrentUserResponse      # Current user DTO
│       ├── OrderDTOs.cs            # All order-related DTOs
│       │   ├── CreateOrderRequest
│       │   ├── UpdateOrderRequest
│       │   ├── OrderResponse
│       │   ├── OrderListItem
│       │   ├── OrderTrackingResponse
│       │   ├── CalculateShippingFeeRequest
│       │   ├── ShippingFeeResponse
│       │   ├── PagedOrderResponse
│       │   └── [summary classes]
│       └── [other DTOs]
│
├── Services/                # Business Logic
│   ├── Interface/
│   │   ├── IAuthService.cs         # Auth service interface
│   │   ├── IOrderService.cs        # Order service interface
│   │   ├── IEmailService.cs        # Email service (placeholder)
│   │   ├── ISmsService.cs          # SMS service (placeholder)
│   │   └── IJwtService.cs          # JWT token service interface
│   │
│   └── Implementation/
│       ├── AuthService.cs          # Authentication business logic
│       ├── OrderService.cs         # Order business logic
│       ├── JwtService.cs           # JWT token generation & validation
│       ├── EmailService.cs         # Email sending (stub)
│       └── SmsService.cs           # SMS sending (stub)
│
├── Repositories/            # Data Access Layer
│   ├── Interface/
│   │   ├── IUserRepository.cs      # User data operations
│   │   └── IOrderRepository.cs     # Order data operations
│   │
│   └── Repo/
│       ├── UserRepository.cs       # User CRUD operations
│       └── OrderRepository.cs      # Order CRUD operations
│
├── Infrastructure/          # Infrastructure Services
│   ├── IJwtService.cs
│   ├── JwtService.cs               # JWT token handling
│   └── JwtExtensions.cs            # JWT extension methods
│
├── Middleware/              # Custom Middleware
│   ├── JwtMiddleware.cs            # JWT validation middleware
│   ├── RoleAuthorizationMiddleware.cs # Role-based access control
│   └── ErrorHandlingMiddleware.cs  # Global error handler
│
├── Authorization/           # Authorization Policies & Handlers
│   ├── Policies/
│   │   ├── AuthorizationPolicies.cs # Define authorization policies
│   │   ├── CompanyAccessHandler.cs  # Company access authorization handler
│   │   └── CompanyAccessRequirement.cs
│   │
├── Attributes/              # Custom Attributes
│   ├── AdminOnlyAttribute.cs
│   ├── DriverOnlyAttribute.cs
│   ├── CustomerOnlyAttribute.cs
│   ├── WarehouseOnlyAttribute.cs
│   └── [other role attributes]
│
├── Requirements/
│   ├── ActiveUserRequirement.cs
│   └── [other requirements]
│
├── Extensions/              # Extension Methods
│   ├── EnumExtensions.cs            # Enum extensions
│   ├── MiddlewareExtensions.cs      # Middleware registration
│   ├── AuthorizationExtensions/
│   │   └── AuthorizationExtensions.cs # JWT claim extraction helpers
│   └── JwtExtensions.cs             # JWT-related extensions
│
├── Validators/              # Fluent Validation Rules
│   ├── OrderValidators.cs           # Order validation rules
│   └── [other validators]
│
├── Enums/                   # Enumeration Types
│   └── OrderEnums.cs                # Order-related enums
│       ├── ParcelType
│       ├── OrderStatus
│       ├── PaymentMethod
│       └── [other enums]
│
├── Hubs/                    # SignalR Hubs (TODO)
│   └── [WebSocket real-time features]
│
├── Mappings/                # AutoMapper Profiles (TODO)
│   └── [DTO to Domain mappings]
│
├── Program.cs               # Application startup & DI configuration
├── appsettings.json         # Production configuration
├── appsettings.Development.json # Development configuration
├── wedeli.csproj            # Project file
└── wedeli.http              # HTTP test file for manual testing

```

---

## 🔐 AUTHENTICATION FLOW

### JWT Token Structure
```
Access Token (60 minutes expiry)
├── Claims
│   ├── sub (userId)
│   ├── username
│   ├── email
│   ├── fullName
│   ├── phone
│   └── role

Refresh Token (7 days expiry)
└── Stored in database for revocation
```

### Auth Endpoints (`/api/auth`)
```
POST   /login              - Login with credentials → JWT tokens
POST   /register           - Register new user
POST   /refresh-token      - Get new access token using refresh token
POST   /change-password    - Change password (requires auth)
POST   /forgot-password    - Request OTP via SMS
POST   /reset-password     - Reset password using OTP
POST   /logout             - Logout (revoke refresh token)
GET    /me                 - Get current user info (requires auth)
GET    /health             - Health check
```

---

## 👥 USER ROLES & PERMISSIONS

```
1. Admin (role_id: 1)
   - Full system access
   - Manage users, drivers, customers
   - View all orders and reports

2. Driver (role_id: 2)
   - View assigned trips
   - Update delivery status
   - Collect COD payments
   - Submit ratings

3. Warehouse Staff (role_id: 3)
   - Manage warehouse operations
   - Receive/send packages
   - Inventory management

4. Multi-Role (role_id: 4)
   - User with multiple roles

5. Customer (role_id: 5)
   - Create orders
   - Track orders
   - View order history
   - Rate deliveries
   - Submit complaints
```

---

## 📦 ORDER MANAGEMENT

### Order Lifecycle
```
Pending → Picked-up → In-Transit → Delivered → Completed
           ↓
        Cancelled (at any stage)
```

### Order DTOs
```
CreateOrderRequest
├── Sender Info (name, phone, address)
├── Receiver Info (name, phone, address, province, district)
├── Package Info (type, weight, declared value, special instructions)
├── Pricing (COD amount, payment method)
├── Route ID (optional)
└── Scheduled pickup time (optional)

OrderResponse (Detailed)
├── Order ID, Tracking Code
├── Status, Current location
├── Sender/Receiver info
├── Driver info
├── Shipping fee breakdown
├── Tracking history
└── Timeline events

PagedOrderResponse (Paginated list)
├── Items (list of OrderListItem)
├── TotalCount, Page, PageSize
├── TotalPages, HasNextPage, HasPreviousPage
└── [pagination metadata]
```

### Order Operations
```
Create Order (Customer)
├── Generate unique tracking code
├── Calculate shipping fee based on:
│   ├── Route distance
│   ├── Package weight
│   └── COD amount
├── Set initial status (Pending)
└── Store in database

Get Order (by ID or Tracking Code)
├── Fetch order details
├── Include shipping fee breakdown
├── Include tracking history
└── Return OrderResponse

Update Order (before pickup)
├── Allow changes to receiver info
├── Allow changes to special instructions
└── Validate status is still "Pending"

Cancel Order
├── Validate order can be cancelled
├── Record cancellation reason
├── Update status to "Cancelled"
└── Process refund if applicable

Get Orders (Paginated by role)
├── Customers see their own orders
├── Drivers see assigned orders
├── Warehouse staff see warehouse orders
├── Admins see all orders
└── Support filtering by status, date range, etc.
```

---

## 🏗️ ARCHITECTURE PATTERNS

### 1. Repository Pattern
```
Controller → Service → Repository → DbContext → Database
                ↓
         Business Logic
```

Each repository implements:
- Generic CRUD operations
- Entity-specific queries
- Data validation

### 2. Dependency Injection (DI)
All services registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
```

### 3. DTO Pattern
- **DTOs for requests**: Validation & data binding
- **DTOs for responses**: Consistent API contracts
- **Mapping**: DTO ↔ Domain models via AutoMapper

### 4. Middleware Pipeline
```
Request
  ↓
HTTPS Redirection
  ↓
CORS
  ↓
Static Files
  ↓
Authentication (JWT)
  ↓
Authorization
  ↓
Custom JWT Middleware
  ↓
Custom Role Authorization Middleware
  ↓
Controllers
  ↓
Response
```

---

## 🔧 CONFIGURATION

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=wedeli;User=root;Password=..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-chars",
    "Issuer": "wedeli-api",
    "Audience": "wedeli-client",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:8080"]
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/wedeli-.log" } }
    ]
  }
}
```

---

## 📊 DATABASE SCHEMA

### Key Tables
```
users                    - All system users
roles                    - User roles
refresh_tokens           - JWT refresh tokens for revocation
customers                - Customer information
drivers                  - Driver information
vehicles                 - Transport vehicles
routes                   - Delivery routes
orders                   - Main order table
order_status_history     - Order status change log
order_photos             - Order package photos
order_transfers          - Internal order transfers
trips                    - Delivery trips/shipments
trip_orders              - Orders in each trip
payments                 - Payment records
cod_transactions         - COD payment details
ratings                  - Delivery ratings
complaints               - Customer complaints
notifications            - System notifications
transport_companies      - Transport company details
warehouse_staff          - Warehouse staff info
company_partnerships     - Company partnerships
daily_activity_log       - Daily activity tracking
daily_summary            - Daily summaries
```

---

## 🚀 API RESPONSE FORMAT

### Standard Response Wrapper
```csharp
{
  "success": true/false,
  "message": "Human-readable message",
  "data": { /* actual data */ },
  "errors": [ /* validation/error details */ ]
}
```

### Example Success Response
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "user": {
      "userId": 1,
      "username": "john_driver",
      "fullName": "John Doe",
      "email": "john@example.com",
      "role": "driver"
    }
  }
}
```

### Example Error Response
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "username",
      "message": "Username already exists"
    }
  ]
}
```

---

## 🔐 SECURITY FEATURES

✅ JWT Bearer Token Authentication
✅ BCrypt Password Hashing
✅ Refresh Token Rotation
✅ Role-Based Access Control (RBAC)
✅ CORS Policy Configuration
✅ Claim-based Authorization
✅ HTTPS Enforcement
✅ Sensitive Data Logging (Dev only)
✅ OTP-based Password Reset
✅ Token Revocation on Logout

---

## 📝 VALIDATION

### Fluent Validation Rules
- **Order validators**: `OrderValidators.cs`
  - Required fields validation
  - Data format validation
  - Business logic validation
  - Range/length validation

### Validation Pipeline
```
Request DTO
  ↓
Model Binding
  ↓
Fluent Validators (automatic via middleware)
  ↓
Business Logic Validation (in Service)
  ↓
Database Constraint Validation
```

---

## 🎯 IMPLEMENTATION CHECKLIST

### ✅ COMPLETED
- [x] JWT Authentication
- [x] User Registration & Login
- [x] Password Management (Change, Reset, Forgot)
- [x] Authorization Policies
- [x] Middleware Stack
- [x] Swagger API Documentation
- [x] Database Schema & Models
- [x] Repository Pattern Implementation
- [x] Service Layer Architecture
- [x] CORS Configuration
- [x] Serilog Logging
- [x] Error Handling

### 🔲 IN PROGRESS
- [ ] Orders Controller Endpoints
- [ ] Order Service Implementation
- [ ] Order CRUD Operations
- [ ] Shipping Fee Calculation
- [ ] Order Status Tracking

### 🔲 TODO
- [ ] Driver Management Module
- [ ] Customer Management Module
- [ ] Vehicle Management Module
- [ ] Route Management Module
- [ ] Trip Management Module
- [ ] Payment Processing
- [ ] COD Transaction Processing
- [ ] Rating & Complaint System
- [ ] Real-time Notifications (SignalR)
- [ ] Advanced Search & Filtering
- [ ] Reporting & Analytics
- [ ] Unit Tests
- [ ] Integration Tests
- [ ] API Documentation (XML comments)
- [ ] Data Seeding
- [ ] Performance Optimization

---

## 💡 KEY CONVENTIONS

### Naming Conventions
- **Controllers**: `{Entity}Controller.cs`
- **Services**: `{Entity}Service.cs` + `I{Entity}Service.cs`
- **Repositories**: `{Entity}Repository.cs` + `I{Entity}Repository.cs`
- **DTOs**: `{Entity}{Operation}Request/Response.cs`
- **Models**: `{Entity}.cs`
- **Enums**: `{Entity}Enums.cs`

### Folder Structure Rules
- Group by feature/domain, not by layer
- Keep related files close together
- Use namespaces that match folder structure
- Interfaces in `Interface/` subfolder
- Implementations in `Implementation/` or `Repo/` subfolder

### Code Style
- PascalCase for classes, methods, properties
- camelCase for local variables
- UPPER_CASE for constants
- Descriptive, English names only
- XML documentation for public members
- Fluent validation for business rules

---

## 🔗 DEPENDENCIES

### NuGet Packages
```
AutoMapper.Extensions.Microsoft.DependencyInjection
BCrypt.Net-Next
CloudinaryDotNet
FluentValidation.AspNetCore
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNetCore.SignalR
Microsoft.EntityFrameworkCore.*
Pomelo.EntityFrameworkCore.MySql
Serilog.AspNetCore
StackExchange.Redis
Swashbuckle.AspNetCore
```

---

## 📌 QUICK START FOR NEW DEVELOPERS

1. **Setup Database**
   ```bash
   # Update connection string in appsettings.json
   dotnet ef database update
   ```

2. **Run Application**
   ```bash
   dotnet run
   ```

3. **Access Swagger**
   - Navigate to: `https://localhost:5001/swagger`

4. **Test Endpoints**
   - Use Swagger UI or `wedeli.http` file for manual testing

5. **Understanding the Flow**
   - Controller receives request
   - Calls Service layer
   - Service calls Repository
   - Repository queries database
   - Response mapped to DTO
   - Returned to client

---

## 📞 CONTACT & SUPPORT

**Repository**: WeDeLi-V2 (GitHub)
**Branch**: main
**Owner**: meanwuan

---

**Last Updated**: November 24, 2025
