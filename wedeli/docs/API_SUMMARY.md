# WeDeli API - Tổng hợp Endpoints và JSON Responses

**Base URL:** `https://your-server/api/v1`

---

## 📦 CẤU TRÚC RESPONSE CHUẨN

Tất cả API đều trả về `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Thông báo",
  "data": { ... }
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Lỗi xảy ra",
  "data": null
}
```

---

## 🔐 1. AUTH CONTROLLER (`/auth`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| POST | `/register` | Đăng ký tài khoản | `TokenResponseDto` |
| POST | `/login` | Đăng nhập | `TokenResponseDto` |
| POST | `/refresh-token` | Làm mới token | `TokenResponseDto` |
| POST | `/forgot-password` | Quên mật khẩu | `null` |
| POST | `/reset-password` | Đặt lại mật khẩu | `null` |
| POST | `/logout` | Đăng xuất | `null` |
| POST | `/change-password` | Đổi mật khẩu | `null` |
| GET | `/profile` | Lấy thông tin user | `UserProfileDto` |

### Request/Response DTOs:

```json
// LoginRequestDto
{
  "username": "admin",
  "password": "123456"
}

// RegisterRequestDto
{
  "username": "newuser",
  "password": "password123",
  "email": "user@email.com",
  "fullName": "Nguyen Van A",
  "role": "Customer"
}

// TokenResponseDto (Login/Register Response)
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "abc123def456...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}

// UserProfileDto
{
  "userId": 1,
  "username": "admin",
  "email": "admin@wedeli.vn",
  "fullName": "Admin User",
  "role": "Admin",
  "isActive": true,
  "companyId": 1,
  "companyName": "Nhà xe Thành Bưởi"
}
```

---

## 👥 2. USERS CONTROLLER (`/users`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/me` | Profile hiện tại | `UserProfileDto` |
| GET | `/{id}` | User theo ID | `UserProfileDto` |
| GET | `/username/{username}` | User theo username | `UserProfileDto` |
| GET | `/` | Danh sách users | `IEnumerable<UserProfileDto>` |
| GET | `/role/{roleName}` | Users theo role | `IEnumerable<UserProfileDto>` |
| GET | `/search?searchTerm=...` | Tìm kiếm users | `IEnumerable<UserProfileDto>` |
| PATCH | `/me` | Cập nhật profile | `UserProfileDto` |
| PATCH | `/{id}/status` | Bật/tắt user | `null` |
| DELETE | `/{id}` | Xóa user | `null` |

---

## 🏢 3. COMPANIES CONTROLLER (`/companies`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/` | Danh sách công ty | `IEnumerable<CompanyResponseDto>` |
| GET | `/{id}` | Chi tiết công ty | `CompanyResponseDto` |
| GET | `/owner/{userId}` | Công ty theo chủ sở hữu | `CompanyResponseDto` |
| GET | `/active` | Công ty đang hoạt động | `IEnumerable<CompanyResponseDto>` |
| GET | `/{id}/statistics` | Thống kê công ty | `CompanyStatisticsDto` |
| POST | `/` | Tạo công ty | `CompanyResponseDto` |
| PUT | `/{id}` | Cập nhật công ty | `CompanyResponseDto` |
| PATCH | `/{id}/status` | Bật/tắt công ty | `bool` |
| DELETE | `/{id}` | Xóa công ty | `bool` |

### CompanyResponseDto:
```json
{
  "companyId": 1,
  "companyName": "Nhà xe Thành Bưởi",
  "address": "266 Lê Hồng Phong, Q.10, TP.HCM",
  "phone": "1900 6067",
  "email": "contact@thanhbuoi.vn",
  "ownerId": 1,
  "latitude": 10.7567890,
  "longitude": 106.6789012,
  "rating": 4.5,
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

---

## 📦 4. ORDERS CONTROLLER (`/orders`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/` | Danh sách đơn hàng | `IEnumerable<OrderResponseDto>` |
| GET | `/{id}` | Chi tiết đơn hàng | `OrderDetailDto` |
| GET | `/tracking/{trackingCode}` | Theo mã tracking | `OrderResponseDto` |
| GET | `/customer/{customerId}` | Đơn theo khách hàng | `IEnumerable<OrderResponseDto>` |
| GET | `/driver/{driverId}` | Đơn theo tài xế | `IEnumerable<OrderResponseDto>` |
| GET | `/status/{status}` | Đơn theo trạng thái | `IEnumerable<OrderResponseDto>` |
| GET | `/pending/list` | Đơn chờ xử lý | `IEnumerable<OrderResponseDto>` |
| GET | `/search?searchTerm=...` | Tìm kiếm đơn | `IEnumerable<OrderResponseDto>` |
| GET | `/track/{trackingCode}` | Theo dõi đơn hàng | `OrderTrackingDto` |
| POST | `/` | Tạo đơn mới | `OrderResponseDto` |
| PUT | `/{id}` | Cập nhật đơn | `OrderResponseDto` |
| POST | `/{id}/cancel` | Hủy đơn | `bool` |
| POST | `/{id}/assign-driver-vehicle` | Gán tài xế/xe | `bool` |
| PATCH | `/{id}/status` | Cập nhật trạng thái | `bool` |
| POST | `/{id}/confirm-pickup` | Xác nhận lấy hàng | `bool` |
| POST | `/{id}/in-transit` | Đang vận chuyển | `bool` |
| POST | `/{id}/out-for-delivery` | Đang giao hàng | `bool` |
| POST | `/{id}/complete-delivery` | Hoàn thành giao | `bool` |
| POST | `/{id}/mark-returned` | Đánh dấu hoàn | `bool` |

### OrderResponseDto:
```json
{
  "orderId": 1,
  "trackingCode": "WDL-2024-001234",
  "customerId": 5,
  "driverId": 3,
  "vehicleId": 2,
  "companyId": 1,
  "originAddress": "123 Nguyễn Huệ, Q.1, TP.HCM",
  "destinationAddress": "456 Lê Lợi, Q.3, TP.HCM",
  "packageDetails": "Hàng điện tử",
  "weightKg": 2.5,
  "dimensionsCm": "30x20x15",
  "price": 50000,
  "orderStatus": "InTransit",
  "paymentStatus": "Paid",
  "createdAt": "2024-01-10T08:00:00Z",
  "updatedAt": "2024-01-10T10:30:00Z"
}
```

### Order Status Values:
- `Pending` - Chờ xử lý
- `Confirmed` - Đã xác nhận
- `PickedUp` - Đã lấy hàng
- `InTransit` - Đang vận chuyển
- `OutForDelivery` - Đang giao hàng
- `Delivered` - Đã giao hàng
- `Cancelled` - Đã hủy
- `Returned` - Đã hoàn trả

---

## 👤 5. CUSTOMERS CONTROLLER (`/customers`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/` | Danh sách khách hàng | `IEnumerable<CustomerResponseDto>` |
| GET | `/{id}` | Chi tiết khách hàng | `CustomerResponseDto` |
| GET | `/user/{userId}` | Khách theo user | `CustomerResponseDto` |
| POST | `/` | Tạo khách hàng | `CustomerResponseDto` |
| PUT | `/{id}` | Cập nhật khách hàng | `CustomerResponseDto` |
| DELETE | `/{id}` | Xóa khách hàng | `bool` |

### CustomerResponseDto:
```json
{
  "customerId": 1,
  "userId": 10,
  "fullName": "Nguyễn Văn A",
  "phone": "0901234567",
  "email": "customer@email.com",
  "defaultAddress": "123 Nguyễn Huệ, Q.1, TP.HCM",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

## 🚚 6. DRIVERS CONTROLLER (`/drivers`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/{id}` | Chi tiết tài xế | `DriverResponseDto` |
| GET | `/user/{userId}` | Tài xế theo user | `DriverResponseDto` |
| GET | `/company/{companyId}` | Tài xế theo công ty | `IEnumerable<DriverResponseDto>` |
| GET | `/company/{companyId}/active` | Tài xế đang hoạt động | `IEnumerable<DriverResponseDto>` |
| GET | `/company/{companyId}/top-performers` | Top tài xế | `IEnumerable<DriverResponseDto>` |
| GET | `/{id}/performance` | Hiệu suất tài xế | `DriverPerformanceDto` |
| POST | `/` | Tạo tài xế | `DriverResponseDto` |
| PUT | `/{id}` | Cập nhật tài xế | `DriverResponseDto` |
| PATCH | `/{id}/status` | Bật/tắt tài xế | `bool` |
| DELETE | `/{id}` | Xóa tài xế | `bool` |
| POST | `/{id}/update-statistics` | Cập nhật thống kê | `bool` |
| POST | `/{id}/update-rating` | Cập nhật rating | `bool` |

### DriverResponseDto:
```json
{
  "driverId": 1,
  "userId": 5,
  "companyId": 1,
  "licenseNumber": "B2-123456",
  "vehicleId": 2,
  "isAvailable": true,
  "rating": 4.8,
  "totalTrips": 150,
  "completedTrips": 145,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-15T00:00:00Z"
}
```

---

## 🚗 7. VEHICLES CONTROLLER (`/vehicles`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/` | Danh sách xe | `IEnumerable<VehicleResponseDto>` |
| GET | `/{id}` | Chi tiết xe | `VehicleResponseDto` |
| GET | `/{id}/capacity` | Dung lượng xe | `VehicleCapacityDto` |
| GET | `/{id}/current-orders` | Đơn đang chở | `IEnumerable<OrderResponseDto>` |
| GET | `/{id}/trips` | Lịch sử chuyến | `IEnumerable<TripResponseDto>` |
| GET | `/available` | Xe còn trống | `IEnumerable<VehicleResponseDto>` |
| GET | `/overloaded` | Xe quá tải | `IEnumerable<VehicleResponseDto>` |
| POST | `/` | Thêm xe | `VehicleResponseDto` |
| PUT | `/{id}` | Cập nhật xe | `VehicleResponseDto` |
| PATCH | `/{id}/status` | Cập nhật trạng thái | `bool` |
| PUT | `/{id}/load` | Cập nhật tải trọng | `VehicleCapacityDto` |
| POST | `/{id}/allow-overload` | Cho phép quá tải | `bool` |
| DELETE | `/{id}` | Xóa xe | `bool` |

### VehicleResponseDto:
```json
{
  "vehicleId": 1,
  "companyId": 1,
  "licensePlate": "51A-12345",
  "vehicleType": "Truck",
  "capacityKg": 1000,
  "currentWeightKg": 500,
  "isAvailable": true,
  "status": "Active",
  "allowOverload": false
}
```

### VehicleCapacityDto:
```json
{
  "vehicleId": 1,
  "licensePlate": "51A-12345",
  "capacityKg": 1000,
  "currentWeightKg": 500,
  "availableCapacityKg": 500,
  "loadPercentage": 50.0,
  "isOverloaded": false
}
```

---

## 🛣️ 8. ROUTES CONTROLLER (`/routes`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/{routeId}` | Chi tiết tuyến | `RouteResponseDto` |
| GET | `/company/{companyId}` | Tuyến theo công ty | `IEnumerable<RouteResponseDto>` |
| GET | `/company/{companyId}/active` | Tuyến đang hoạt động | `IEnumerable<RouteResponseDto>` |
| GET | `/search?origin=...&destination=...` | Tìm tuyến | `IEnumerable<RouteResponseDto>` |
| GET | `/optimal?origin=...&destination=...` | Tuyến tối ưu | `RouteResponseDto` |
| POST | `/` | Tạo tuyến | `RouteResponseDto` |
| PUT | `/{routeId}` | Cập nhật tuyến | `RouteResponseDto` |
| PATCH | `/{routeId}/status` | Bật/tắt tuyến | `bool` |
| DELETE | `/{routeId}` | Xóa tuyến | `bool` |

### RouteResponseDto:
```json
{
  "routeId": 1,
  "companyId": 1,
  "originProvince": "Hồ Chí Minh",
  "originDistrict": "Quận 1",
  "destinationProvince": "Đà Lạt",
  "destinationDistrict": "TP Đà Lạt",
  "distanceKm": 300,
  "estimatedTimeHours": 6.5,
  "pricePerKm": 5000,
  "isActive": true
}
```

---

## 🚌 9. TRIPS CONTROLLER (`/trips`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/{id}` | Chi tiết chuyến | `TripResponseDto` |
| GET | `/route/{routeId}` | Chuyến theo tuyến | `IEnumerable<TripResponseDto>` |
| GET | `/vehicle/{vehicleId}` | Chuyến theo xe | `IEnumerable<TripResponseDto>` |
| GET | `/driver/{driverId}` | Chuyến theo tài xế | `IEnumerable<TripResponseDto>` |
| GET | `/date/{date}` | Chuyến theo ngày | `IEnumerable<TripResponseDto>` |
| GET | `/status/{status}` | Chuyến theo trạng thái | `IEnumerable<TripResponseDto>` |
| GET | `/active` | Chuyến đang chạy | `IEnumerable<TripResponseDto>` |
| GET | `/return` | Chuyến về | `IEnumerable<TripResponseDto>` |
| GET | `/{tripId}/orders` | Đơn trong chuyến | `IEnumerable<TripOrderDto>` |
| POST | `/` | Tạo chuyến | `TripResponseDto` |
| PUT | `/{id}` | Cập nhật chuyến | `TripResponseDto` |
| PATCH | `/{id}/status` | Cập nhật trạng thái | `bool` |
| POST | `/{id}/start` | Bắt đầu chuyến | `bool` |
| POST | `/{id}/complete` | Hoàn thành chuyến | `bool` |
| DELETE | `/{id}` | Xóa chuyến | `bool` |
| POST | `/{tripId}/orders/{orderId}` | Thêm đơn vào chuyến | `bool` |
| DELETE | `/{tripId}/orders/{orderId}` | Xóa đơn khỏi chuyến | `bool` |

### TripResponseDto:
```json
{
  "tripId": 1,
  "routeId": 1,
  "driverId": 3,
  "vehicleId": 2,
  "scheduledStartTime": "2024-01-15T06:00:00Z",
  "actualStartTime": "2024-01-15T06:15:00Z",
  "actualEndTime": null,
  "status": "InProgress",
  "totalDistanceKm": 300,
  "estimatedRevenue": 150000
}
```

### Trip Status Values:
- `Scheduled` - Đã lên lịch
- `InProgress` - Đang chạy
- `Completed` - Hoàn thành
- `Cancelled` - Đã hủy

---

## 💰 10. PAYMENTS CONTROLLER (`/payments`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| POST | `/` | Tạo thanh toán | `PaymentResponseDto` |
| GET | `/{id}` | Chi tiết thanh toán | `PaymentResponseDto` |
| GET | `/order/{orderId}` | Thanh toán theo đơn | `IEnumerable<PaymentResponseDto>` |
| GET | `/customer/{customerId}` | Thanh toán theo KH | `IEnumerable<PaymentResponseDto>` |
| GET | `/status/{status}` | Theo trạng thái | `IEnumerable<PaymentResponseDto>` |
| POST | `/{id}/process` | Xử lý thanh toán | `PaymentResponseDto` |
| PATCH | `/{id}/status` | Cập nhật trạng thái | `bool` |
| POST | `/{id}/refund` | Hoàn tiền | `PaymentResponseDto` |

### PaymentResponseDto:
```json
{
  "paymentId": 1,
  "orderId": 10,
  "amount": 50000,
  "paymentMethod": "COD",
  "transactionReference": "TXN-2024-001234",
  "paymentStatus": "Completed",
  "paymentDate": "2024-01-15T15:30:00Z"
}
```

### Payment Status Values:
- `Pending` - Chờ thanh toán
- `Processing` - Đang xử lý
- `Completed` - Hoàn thành
- `Failed` - Thất bại
- `Refunded` - Đã hoàn tiền

---

## ⭐ 11. RATINGS CONTROLLER (`/ratings`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| POST | `/` | Tạo đánh giá | `RatingResponseDto` |
| GET | `/{id}` | Chi tiết đánh giá | `RatingResponseDto` |
| GET | `/order/{orderId}` | Đánh giá theo đơn | `RatingResponseDto` |
| GET | `/driver/{driverId}` | Đánh giá theo tài xế | `IEnumerable<RatingResponseDto>` |
| GET | `/driver/{driverId}/average` | Điểm TB tài xế | `double` |
| GET | `/driver/{driverId}/summary` | Tóm tắt đánh giá | `DriverRatingSummaryDto` |
| GET | `/customer/{customerId}` | Đánh giá theo KH | `IEnumerable<RatingResponseDto>` |
| PUT | `/{id}` | Cập nhật đánh giá | `RatingResponseDto` |
| DELETE | `/{id}` | Xóa đánh giá | `bool` |

### RatingResponseDto:
```json
{
  "ratingId": 1,
  "orderId": 10,
  "customerId": 5,
  "driverId": 3,
  "ratingValue": 5,
  "comment": "Giao hàng nhanh, tài xế thân thiện",
  "ratingDate": "2024-01-15T16:00:00Z"
}
```

### DriverRatingSummaryDto:
```json
{
  "driverId": 3,
  "driverName": "Nguyễn Văn B",
  "averageRating": 4.8,
  "totalRatings": 150,
  "fiveStarCount": 120,
  "fourStarCount": 25,
  "threeStarCount": 5,
  "twoStarCount": 0,
  "oneStarCount": 0
}
```

---

## 🔔 12. NOTIFICATIONS CONTROLLER (`/notifications`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/user/{userId}` | Thông báo theo user | `IEnumerable<NotificationResponseDto>` |
| GET | `/user/{userId}/unread` | Thông báo chưa đọc | `IEnumerable<NotificationResponseDto>` |
| GET | `/` | Tất cả thông báo | `IEnumerable<NotificationResponseDto>` |
| PUT | `/{id}/read` | Đánh dấu đã đọc | `bool` |
| PUT | `/user/{userId}/read-all` | Đọc tất cả | `bool` |
| DELETE | `/{id}` | Xóa thông báo | `bool` |

### NotificationResponseDto:
```json
{
  "notificationId": 1,
  "userId": 10,
  "orderId": 5,
  "notificationType": "OrderUpdate",
  "title": "Đơn hàng đã được giao",
  "message": "Đơn hàng WDL-2024-001234 đã giao thành công",
  "sentVia": "Push",
  "isRead": false,
  "createdAt": "2024-01-15T15:30:00Z"
}
```

---

## 🤝 13. PARTNERSHIPS CONTROLLER (`/partnerships`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/companies` | Danh sách công ty đối tác | `IEnumerable<CompanyResponseDto>` |
| GET | `/companies/{id}` | Chi tiết công ty | `CompanyResponseDto` |
| GET | `/{id}` | Chi tiết partnership | `PartnershipResponseDto` |
| GET | `/company/{companyId}` | Partnerships của công ty | `IEnumerable<PartnershipResponseDto>` |
| GET | `/pending` | Yêu cầu chờ duyệt | `IEnumerable<PartnershipResponseDto>` |
| POST | `/` | Tạo yêu cầu hợp tác | `PartnershipResponseDto` |
| PUT | `/{id}` | Cập nhật partnership | `PartnershipResponseDto` |
| POST | `/{id}/accept` | Chấp nhận yêu cầu | `bool` |
| POST | `/{id}/reject` | Từ chối yêu cầu | `bool` |
| DELETE | `/{id}` | Xóa partnership | `bool` |

### PartnershipResponseDto:
```json
{
  "partnershipId": 1,
  "company1Id": 1,
  "company2Id": 2,
  "status": "Active",
  "requestedAt": "2024-01-01T00:00:00Z",
  "respondedAt": "2024-01-02T10:00:00Z"
}
```

---

## 🔄 14. TRANSFERS CONTROLLER (`/transfers`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| POST | `/` | Chuyển đơn sang đối tác | `OrderTransferResponseDto` |
| GET | `/outgoing` | Yêu cầu chuyển đi | `IEnumerable<OrderTransferResponseDto>` |
| GET | `/incoming` | Yêu cầu nhận vào | `IEnumerable<OrderTransferResponseDto>` |
| GET | `/{id}` | Chi tiết transfer | `OrderTransferResponseDto` |
| POST | `/{id}/accept` | Chấp nhận transfer | `bool` |
| POST | `/{id}/reject` | Từ chối transfer | `bool` |
| POST | `/{id}/cancel` | Hủy transfer | `bool` |
| GET | `/pending` | Transfers chờ xử lý | `IEnumerable<OrderTransferResponseDto>` |
| GET | `/order/{orderId}/history` | Lịch sử transfer | `IEnumerable<OrderTransferResponseDto>` |

### OrderTransferResponseDto:
```json
{
  "transferId": 1,
  "orderId": 10,
  "fromCompanyId": 1,
  "toCompanyId": 2,
  "transferStatus": "Pending",
  "transferReason": "Không thuộc vùng giao hàng",
  "requestedAt": "2024-01-15T10:00:00Z",
  "respondedAt": null
}
```

---

## 📊 15. REPORTS CONTROLLER (`/reports`)

| Method | Endpoint | Mô tả | Response Data |
|--------|----------|-------|---------------|
| GET | `/daily/{date}` | Báo cáo ngày | `DailyReportSummaryDto` |
| GET | `/daily/today` | Báo cáo hôm nay | `DailyReportSummaryDto` |
| GET | `/daily/range?from=...&to=...` | Báo cáo theo khoảng | `IEnumerable<DailyReportSummaryDto>` |
| GET | `/daily/last/{days}` | N ngày gần nhất | `IEnumerable<DailyReportSummaryDto>` |
| GET | `/monthly/current` | Tháng hiện tại | `DailyReportSummaryDto` |
| POST | `/daily/generate` | Tạo báo cáo ngày | `DailyReportSummaryDto` |
| GET | `/driver/{driverId}` | Hiệu suất tài xế | `DriverPerformanceDto` |
| GET | `/driver/{driverId}/monthly` | Hiệu suất tháng | `DriverPerformanceDto` |
| GET | `/drivers/top?count=...` | Top tài xế | `IEnumerable<DriverPerformanceDto>` |
| GET | `/drivers/company/{companyId}` | Tất cả tài xế công ty | `IEnumerable<DriverPerformanceDto>` |
| GET | `/export/csv` | Xuất CSV | `501 Not Implemented` |
| GET | `/export/pdf` | Xuất PDF | `501 Not Implemented` |

### DailyReportSummaryDto:
```json
{
  "date": "2024-01-15",
  "totalOrders": 150,
  "completedOrders": 140,
  "cancelledOrders": 5,
  "pendingOrders": 5,
  "totalRevenue": 7500000,
  "averageRating": 4.7
}
```

### DriverPerformanceDto:
```json
{
  "driverId": 3,
  "driverName": "Nguyễn Văn B",
  "totalTrips": 50,
  "completedTrips": 48,
  "cancelledTrips": 2,
  "totalDistanceKm": 1500,
  "averageRating": 4.8,
  "totalRevenue": 2500000
}
```

---

## 🔑 AUTHENTICATION HEADER

Tất cả API (trừ `/auth/login`, `/auth/register`) cần header:
```
Authorization: Bearer <your_jwt_token>
```

---

## 📱 ANDROID/iOS INTEGRATION

### Retrofit Interface Example:
```kotlin
interface WedeliApi {
    // Auth
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): ApiResponse<TokenResponse>
    
    @GET("auth/profile")
    suspend fun getProfile(): ApiResponse<UserProfile>
    
    // Companies
    @GET("partnerships/companies")
    suspend fun getCompanies(): ApiResponse<List<CompanyResponse>>
    
    @GET("partnerships/companies/{id}")
    suspend fun getCompanyDetails(@Path("id") id: Int): ApiResponse<CompanyResponse>
    
    // Orders
    @GET("orders")
    suspend fun getOrders(): ApiResponse<List<OrderResponse>>
    
    @POST("orders")
    suspend fun createOrder(@Body request: CreateOrderRequest): ApiResponse<OrderResponse>
    
    @PATCH("orders/{id}/status")
    suspend fun updateOrderStatus(
        @Path("id") id: Int, 
        @Body request: UpdateStatusRequest
    ): ApiResponse<Boolean>
}
```

### Data Classes (Kotlin):
```kotlin
// API Response wrapper
data class ApiResponse<T>(
    val success: Boolean,
    val message: String,
    val data: T?
)

// Company
data class CompanyResponse(
    val companyId: Int,
    val companyName: String,
    val address: String?,
    val phone: String?,
    val email: String?,
    val latitude: Double?,
    val longitude: Double?,
    val rating: Double,
    val isActive: Boolean
)

// Order
data class OrderResponse(
    val orderId: Int,
    val trackingCode: String,
    val customerId: Int,
    val driverId: Int?,
    val vehicleId: Int?,
    val companyId: Int,
    val originAddress: String,
    val destinationAddress: String,
    val packageDetails: String?,
    val weightKg: Double,
    val price: Double,
    val orderStatus: String,
    val paymentStatus: String,
    val createdAt: String
)
```

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **GPS Coordinates**: `latitude` và `longitude` có thể `null` nếu chưa geocode
2. **Pagination**: Hầu hết list endpoints hỗ trợ `?page=1&pageSize=10`
3. **Date Format**: ISO 8601 (`2024-01-15T10:30:00Z`)
4. **Nullable Fields**: Các trường nullable được đánh dấu `?` trong Kotlin
5. **Role Values**: `Admin`, `CompanyAdmin`, `Driver`, `Customer`

---

## 📋 DANH SÁCH ROLES VÀ QUYỀN TRUY CẬP

| Role | Mô tả | Quyền chính |
|------|-------|------------|
| `Admin` | Quản trị hệ thống | Toàn quyền |
| `CompanyAdmin` | Quản lý nhà xe | Quản lý công ty, tài xế, xe, tuyến, đơn |
| `Driver` | Tài xế | Xem/cập nhật chuyến, đơn được gán |
| `Customer` | Khách hàng | Tạo đơn, theo dõi đơn, đánh giá |

---

*Cập nhật: 2024-12-12*
