# WeDeli API Documentation - Complete Reference

> Tài liệu đầy đủ cho Android App tích hợp WeDeli Backend API

## 📌 Base URL
```
http://localhost:5000/api/v1
```

## 📦 Response Format
```json
{
  "success": true,
  "message": "string",
  "data": { ... },
  "errors": [],
  "timestamp": "2025-12-08T12:00:00Z",
  "statusCode": 200
}
```

---

# 🔐 1. AUTHENTICATION API
**Base:** `/api/v1/auth`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/register` | ❌ | Đăng ký tài khoản |
| POST | `/login` | ❌ | Đăng nhập |
| POST | `/refresh-token` | ❌ | Làm mới token |
| POST | `/logout` | ✅ | Đăng xuất |
| POST | `/forgot-password` | ❌ | Quên mật khẩu |
| POST | `/reset-password` | ❌ | Đặt lại mật khẩu |
| POST | `/change-password` | ✅ | Đổi mật khẩu |

### Login Request/Response
```json
// POST /auth/login
{
  "emailOrUsername": "string",
  "password": "string",
  "rememberMe": false
}

// Response
{
  "userId": 1,
  "username": "string",
  "fullName": "string",
  "email": "string",
  "phone": "string",
  "roleName": "Customer",
  "roleId": 5,
  "companyId": null,
  "companyName": null,
  "accessToken": "jwt_token",
  "refreshToken": "refresh_token",
  "tokenExpiration": "datetime",
  "refreshTokenExpiration": "datetime"
}
```

### Roles
| RoleId | Name |
|--------|------|
| 1 | SuperAdmin |
| 2 | Admin |
| 3 | CompanyAdmin |
| 4 | Driver |
| 5 | Customer |

---

# 👤 2. USERS API
**Base:** `/api/v1/users`

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| GET | `/me` | ✅ | All | Profile hiện tại |
| GET | `/{id}` | ✅ | All | User theo ID |
| GET | `/username/{username}` | ✅ | All | User theo username |
| GET | `/` | ✅ | Admin | Tất cả users (paging) |
| GET | `/role/{roleName}` | ✅ | Admin | Users theo role |
| PATCH | `/me` | ✅ | All | Cập nhật profile |
| GET | `/search` | ✅ | Admin | Tìm kiếm users |
| PATCH | `/{id}/status` | ✅ | Admin | Bật/tắt user |
| DELETE | `/{id}` | ✅ | Admin | Xóa user |

---

# 👥 3. CUSTOMERS API
**Base:** `/api/v1/customers`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/{id}` | ✅ | Customer theo ID |
| GET | `/{id}/detail` | ✅ | Chi tiết + địa chỉ + đơn hàng |
| GET | `/user/{userId}` | ✅ | Customer theo UserID |
| GET | `/phone/{phone}` | ✅ | Customer theo SĐT |
| GET | `/` | ✅ | Tất cả customers (paging) |
| GET | `/regular` | ✅ | Khách hàng thường xuyên |
| GET | `/search` | ✅ | Tìm kiếm |
| POST | `/` | ✅ | Tạo customer |
| PUT | `/{id}` | ✅ | Cập nhật |
| PATCH | `/{id}/regular-status` | ✅ | Cập nhật trạng thái VIP |
| PATCH | `/{id}/payment-privilege` | ✅ | Cập nhật quyền thanh toán |
| GET | `/{id}/statistics` | ✅ | Thống kê khách hàng |
| **Address Management** |
| GET | `/{id}/addresses` | ✅ | Danh sách địa chỉ |
| GET | `/{id}/addresses/default` | ✅ | Địa chỉ mặc định |
| POST | `/{id}/addresses` | ✅ | Thêm địa chỉ |
| PUT | `/{customerId}/addresses/{addressId}` | ✅ | Sửa địa chỉ |
| DELETE | `/{customerId}/addresses/{addressId}` | ✅ | Xóa địa chỉ |
| POST | `/{customerId}/addresses/{addressId}/default` | ✅ | Đặt mặc định |

---

# 🚚 4. DRIVERS API
**Base:** `/api/v1/drivers`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/{id}` | ✅ | Driver theo ID |
| GET | `/user/{userId}` | ✅ | Driver theo UserID |
| GET | `/company/{companyId}` | ✅ | Drivers theo công ty |
| GET | `/company/{companyId}/active` | ✅ | Drivers đang hoạt động |
| GET | `/company/{companyId}/top` | ✅ | Top tài xế hiệu suất cao |
| POST | `/` | ✅ | Tạo driver |
| PUT | `/{id}` | ✅ | Cập nhật driver |
| PATCH | `/{id}/status` | ✅ | Bật/tắt trạng thái |
| DELETE | `/{id}` | ✅ | Xóa driver |
| GET | `/{id}/performance` | ✅ | Hiệu suất driver |
| PUT | `/{id}/statistics` | ✅ | Cập nhật thống kê |
| PUT | `/{id}/rating` | ✅ | Cập nhật rating |

---

# 📦 5. ORDERS API
**Base:** `/api/v1/orders`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/{id}` | ✅ | Đơn hàng theo ID |
| GET | `/tracking/{trackingCode}` | ❌ | Tra cứu bằng mã |
| GET | `/` | ✅ Admin | Tất cả đơn hàng |
| GET | `/customer/{customerId}` | ✅ | Đơn theo khách hàng |
| GET | `/driver/{driverId}` | ✅ | Đơn theo tài xế |
| GET | `/status/{status}` | ✅ | Đơn theo trạng thái |
| GET | `/pending/list` | ✅ | Đơn chờ xử lý |
| GET | `/search` | ✅ | Tìm kiếm |
| POST | `/` | ✅ | Tạo đơn hàng |
| PUT | `/{id}` | ✅ | Cập nhật đơn hàng |
| POST | `/{id}/cancel` | ✅ | Hủy đơn |
| POST | `/{id}/assign-driver-vehicle` | ✅ | Gán tài xế/xe |
| PATCH | `/{id}/status` | ✅ | Cập nhật trạng thái |
| **Workflow Actions** |
| POST | `/{id}/confirm-pickup` | ✅ | Xác nhận lấy hàng |
| POST | `/{id}/in-transit` | ✅ | Đang vận chuyển |
| POST | `/{id}/out-for-delivery` | ✅ | Đang giao |
| POST | `/{id}/complete-delivery` | ✅ | Hoàn thành |
| POST | `/{id}/mark-returned` | ✅ | Đánh dấu hoàn |
| GET | `/track/{trackingCode}` | ❌ | Tracking chi tiết |
| **Photo Management** |
| POST | `/{id}/photos` | ✅ | Upload ảnh |
| GET | `/{id}/photos` | ✅ | Lấy danh sách ảnh |

### Order Status Values
`pending_pickup` → `picked_up` → `in_transit` → `out_for_delivery` → `delivered` / `returned` / `cancelled`

---

# 🚗 6. VEHICLES API
**Base:** `/api/v1/vehicles`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | ✅ | Danh sách xe (cần companyId) |
| GET | `/{id}` | ✅ | Chi tiết xe |
| GET | `/{id}/capacity` | ✅ | Thông tin tải trọng |
| GET | `/{id}/current-orders` | ✅ | Đơn hàng đang chở |
| GET | `/{id}/trips` | ✅ | Lịch sử chuyến |
| POST | `/` | ✅ Admin | Tạo xe |
| PUT | `/{id}` | ✅ Admin | Cập nhật xe |
| PATCH | `/{id}/status` | ✅ Admin | Cập nhật trạng thái |
| PUT | `/{id}/load` | ✅ | Cập nhật tải |
| POST | `/{id}/allow-overload` | ✅ Admin | Cho phép quá tải |
| GET | `/available` | ❌ | Xe khả dụng |
| GET | `/overloaded` | ✅ Admin | Xe quá tải |
| DELETE | `/{id}` | ✅ Admin | Xóa xe |

---

# 🗺️ 7. ROUTES API
**Base:** `/api/v1/routes`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/{routeId}` | ✅ | Chi tiết tuyến |
| GET | `/company/{companyId}` | ✅ | Tuyến theo công ty |
| GET | `/company/{companyId}/active` | ✅ | Tuyến đang hoạt động |
| GET | `/search` | ✅ | Tìm theo tỉnh |
| GET | `/optimal` | ✅ | Tuyến tối ưu |
| POST | `/` | ✅ Admin | Tạo tuyến |
| PUT | `/{routeId}` | ✅ Admin | Cập nhật |
| PATCH | `/{routeId}/status` | ✅ Admin | Bật/tắt |
| DELETE | `/{routeId}` | ✅ Admin | Xóa |

---

# 🚌 8. TRIPS API
**Base:** `/api/v1/trips`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/{id}` | ✅ | Chi tiết chuyến |
| GET | `/route/{routeId}` | ✅ | Chuyến theo tuyến |
| GET | `/vehicle/{vehicleId}` | ✅ | Chuyến theo xe |
| GET | `/driver/{driverId}` | ✅ | Chuyến theo tài xế |
| GET | `/date/{date}` | ✅ | Chuyến theo ngày |
| GET | `/status/{status}` | ✅ | Chuyến theo trạng thái |
| GET | `/active` | ✅ | Chuyến đang chạy |
| GET | `/return` | ✅ | Chuyến về |
| POST | `/` | ✅ Admin | Tạo chuyến |
| PUT | `/{id}` | ✅ Admin | Cập nhật |
| PATCH | `/{id}/status` | ✅ | Cập nhật trạng thái |
| POST | `/{id}/start` | ✅ | Bắt đầu chuyến |
| POST | `/{id}/complete` | ✅ | Hoàn thành |
| DELETE | `/{id}` | ✅ Admin | Xóa |
| POST | `/{tripId}/orders/{orderId}` | ✅ | Gán đơn vào chuyến |
| DELETE | `/{tripId}/orders/{orderId}` | ✅ | Bỏ đơn khỏi chuyến |
| GET | `/{tripId}/orders` | ✅ | Đơn trong chuyến |

### Trip Status Values
`scheduled` → `in_progress` → `completed` / `cancelled`

---

# 📊 9. DASHBOARD API
**Base:** `/api/v1/dashboard`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/admin` | ✅ Admin | Dashboard admin |
| GET | `/driver/{driverId}` | ✅ | Dashboard tài xế |
| GET | `/customer/{customerId}` | ✅ | Dashboard khách hàng |
| GET | `/company/{companyId}` | ✅ | Dashboard công ty |
| GET | `/role/{userRole}` | ✅ | Dashboard theo role |
| GET | `/summary` | ✅ | Tổng quan |

---

# 📈 10. REPORTS API
**Base:** `/api/v1/reports`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/daily/{date}` | ✅ | Báo cáo ngày |
| GET | `/daily/today` | ✅ | Báo cáo hôm nay |
| GET | `/daily/range` | ✅ | Báo cáo khoảng ngày |
| GET | `/daily/last/{days}` | ✅ | N ngày gần nhất |
| GET | `/monthly/current` | ✅ | Báo cáo tháng |
| POST | `/daily/{date}/generate` | ✅ Admin | Tạo báo cáo |
| GET | `/driver/{driverId}/performance` | ✅ | Hiệu suất tài xế |
| GET | `/driver/{driverId}/monthly` | ✅ | Hiệu suất tháng |
| GET | `/company/{companyId}/top-drivers` | ✅ | Top tài xế |
| GET | `/company/{companyId}/drivers` | ✅ | Tất cả tài xế |
| GET | `/daily/{date}/csv` | ✅ | Export CSV |
| GET | `/driver/{driverId}/pdf` | ✅ | Export PDF |

---

# 💵 11. COD API
**Base:** `/api/v1/cod`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/{transactionId}` | ✅ | COD transaction |
| GET | `/order/{orderId}` | ✅ | COD theo đơn |
| GET | `/driver/{driverId}` | ✅ | COD theo tài xế |
| GET | `/driver/{driverId}/pending` | ✅ | COD chờ thu |
| POST | `/collect` | ✅ | Thu COD |
| POST | `/submit` | ✅ | Nộp COD |
| GET | `/driver/{driverId}/pending-amount` | ✅ | Số tiền chờ nộp |
| POST | `/{transactionId}/confirm` | ✅ Admin | Xác nhận nhận COD |
| POST | `/transfer-to-sender` | ✅ Admin | Chuyển cho người gửi |
| GET | `/driver/{driverId}/summary/{date}` | ✅ | Tổng kết COD ngày |
| GET | `/reconciliations/pending` | ✅ | Chờ đối soát |
| POST | `/{summaryId}/reconcile` | ✅ Admin | Đối soát |
| POST | `/reconcile-all` | ✅ Admin | Đối soát tất cả |
| GET | `/dashboard` | ✅ | Dashboard COD |

---

# 💳 12. PAYMENTS API
**Base:** `/api/v1/payments`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | ✅ | Tạo thanh toán |
| GET | `/{id}` | ✅ | Chi tiết |
| GET | `/order/{orderId}` | ✅ | Theo đơn hàng |
| GET | `/customer/{customerId}` | ✅ | Theo khách hàng |
| GET | `/status/{status}` | ✅ | Theo trạng thái |
| POST | `/{id}/process` | ✅ | Xử lý thanh toán |
| PATCH | `/{id}/status` | ✅ | Cập nhật trạng thái |
| POST | `/{id}/refund` | ✅ Admin | Hoàn tiền |

---

# ⭐ 13. RATINGS API
**Base:** `/api/v1/ratings`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | ✅ | Tạo đánh giá |
| GET | `/{id}` | ✅ | Chi tiết |
| GET | `/order/{orderId}` | ✅ | Theo đơn hàng |
| GET | `/driver/{driverId}` | ✅ | Theo tài xế |
| GET | `/driver/{driverId}/average` | ✅ | Điểm trung bình |
| GET | `/driver/{driverId}/summary` | ✅ | Tổng kết |
| GET | `/customer/{customerId}` | ✅ | Theo khách hàng |
| PUT | `/{id}` | ✅ | Sửa đánh giá |
| DELETE | `/{id}` | ✅ | Xóa |

---

# 📝 14. COMPLAINTS API
**Base:** `/api/v1/complaints`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | ✅ | Tạo khiếu nại |
| GET | `/{id}` | ✅ | Chi tiết |
| GET | `/order/{orderId}` | ✅ | Theo đơn hàng |
| GET | `/customer/{customerId}` | ✅ | Theo khách hàng |
| GET | `/status/{status}` | ✅ | Theo trạng thái |
| GET | `/pending` | ✅ Admin | Chờ xử lý |
| POST | `/{id}/resolve` | ✅ Admin | Giải quyết |
| POST | `/{id}/reject` | ✅ Admin | Từ chối |

---

# 🔄 15. TRANSFERS API
**Base:** `/api/v1/transfers`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | ✅ | Chuyển đơn |
| GET | `/outgoing/{companyId}` | ✅ | Đơn gửi đi |
| GET | `/incoming/{companyId}` | ✅ | Đơn nhận về |
| GET | `/{id}` | ✅ | Chi tiết |
| POST | `/{id}/accept` | ✅ | Chấp nhận |
| POST | `/{id}/reject` | ✅ | Từ chối |
| POST | `/{id}/cancel` | ✅ | Hủy |
| GET | `/pending/{companyId}` | ✅ | Chờ xử lý |
| GET | `/order/{orderId}/history` | ✅ | Lịch sử chuyển |

---

# 🤝 16. PARTNERSHIPS API
**Base:** `/api/v1/partnerships`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | ✅ | Danh sách đối tác |
| GET | `/{id}` | ✅ | Chi tiết |
| POST | `/` | ✅ Admin | Tạo partnership |
| PUT | `/{id}` | ✅ Admin | Cập nhật |
| DELETE | `/{id}` | ✅ Admin | Xóa |
| PATCH | `/{id}/status` | ✅ Admin | Bật/tắt |
| PATCH | `/{id}/commission` | ✅ Admin | Cập nhật phí |
| PATCH | `/{id}/priority` | ✅ Admin | Cập nhật ưu tiên |
| GET | `/company/{companyId}/preferred` | ✅ | Đối tác ưu tiên |
| **Companies** |
| GET | `/companies` | ✅ | Danh sách công ty |
| GET | `/companies/{id}` | ✅ | Chi tiết công ty |
| **Transfers** |
| POST | `/transfers` | ✅ | Chuyển đơn |
| GET | `/transfers/{id}` | ✅ | Chi tiết |
| GET | `/transfers/company/{companyId}` | ✅ | Theo công ty |
| POST | `/transfers/{id}/accept` | ✅ | Chấp nhận |
| POST | `/transfers/{id}/reject` | ✅ | Từ chối |

---

# 🏢 17. COMPANIES API (Transport Companies)
**Base:** `/api/v1/partnerships/companies`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | ✅ | Danh sách nhà xe |
| GET | `/{id}` | ✅ | Chi tiết nhà xe |
| POST | `/` | ✅ Admin | Tạo nhà xe |
| PUT | `/{id}` | ✅ Admin | Cập nhật nhà xe |
| PATCH | `/{id}/status` | ✅ Admin | Bật/tắt |

### 🗺️ Company Response với GPS Coordinates (MỚI)
> **Tính năng mới (2025-12-10):** Backend tự động geocoding địa chỉ → tọa độ GPS khi tạo/sửa nhà xe

```json
// GET /partnerships/companies
{
  "success": true,
  "data": [
    {
      "companyId": 1,
      "companyName": "Nhà xe ABC",
      "businessLicense": "GP-123456",
      "address": "123 Nguyễn Văn Linh, Quận 7, TP.HCM",
      "phone": "0901234567",
      "email": "contact@nhaxeabc.vn",
      "isActive": true,
      "rating": 4.5,
      "latitude": 10.7323456,      // ← GPS Latitude (tự động từ address)
      "longitude": 106.7014789,    // ← GPS Longitude (tự động từ address)
      "createdAt": "2025-01-15T10:00:00Z"
    }
  ]
}
```

### 📍 Hiển thị nhà xe trên Google Maps (Android)
```java
// Lấy danh sách nhà xe từ API
Call<ApiResponse<List<CompanyResponse>>> getCompanies();

// Hiển thị trên map
for (CompanyResponse company : companies) {
    if (company.getLatitude() != null && company.getLongitude() != null) {
        LatLng position = new LatLng(
            company.getLatitude().doubleValue(),
            company.getLongitude().doubleValue()
        );
        googleMap.addMarker(new MarkerOptions()
            .position(position)
            .title(company.getCompanyName())
            .snippet(company.getAddress()));
    }
}
```

### � CompanyResponse DTO
```java
public class CompanyResponse {
    private int companyId;
    private String companyName;
    private String businessLicense;
    private String address;
    private String phone;
    private String email;
    private boolean isActive;
    private double rating;
    private Double latitude;     // Có thể null nếu geocoding thất bại
    private Double longitude;    // Có thể null nếu geocoding thất bại
    private String createdAt;
    // getters/setters
}
```

### ⚠️ Lưu ý về Coordinates
- `latitude`/`longitude` có thể **null** nếu:
  - Địa chỉ không hợp lệ hoặc không thể geocode
  - Nhà xe cũ chưa được geocode
- Luôn kiểm tra null trước khi hiển thị marker trên map
- Backend sẽ tự động re-geocode khi cập nhật địa chỉ

---

# �📱 Android Integration

## Retrofit Interface Example
```java
public interface WeDeliApiService {
    // Auth
    @POST("auth/login")
    Call<ApiResponse<LoginResponse>> login(@Body LoginRequest request);
    
    // Orders
    @GET("orders/{id}")
    Call<ApiResponse<OrderResponse>> getOrder(
        @Header("Authorization") String token,
        @Path("id") int orderId
    );
    
    @GET("orders/track/{trackingCode}")
    Call<ApiResponse<OrderTracking>> trackOrder(
        @Path("trackingCode") String code
    );
    
    // Vehicles
    @GET("vehicles")
    Call<ApiResponse<List<VehicleResponse>>> getVehicles(
        @Header("Authorization") String token,
        @Query("companyId") int companyId
    );
    
    // Companies (NEW - with GPS coordinates)
    @GET("partnerships/companies")
    Call<ApiResponse<List<CompanyResponse>>> getCompanies(
        @Header("Authorization") String token
    );
}
```

## Base Response Class
```java
public class ApiResponse<T> {
    private boolean success;
    private String message;
    private T data;
    private List<String> errors;
    private String timestamp;
    private int statusCode;
    // getters/setters
}
```

## Token Interceptor
```java
public class AuthInterceptor implements Interceptor {
    @Override
    public Response intercept(Chain chain) throws IOException {
        String token = SharedPrefs.getAccessToken();
        Request.Builder builder = chain.request().newBuilder();
        if (token != null) {
            builder.addHeader("Authorization", "Bearer " + token);
        }
        return chain.proceed(builder.build());
    }
}
```

## Auto Refresh Token
```java
// Trong Retrofit Authenticator
if (response.code() == 401) {
    RefreshTokenResponse newToken = refreshToken();
    if (newToken != null) {
        saveToken(newToken);
        return response.request().newBuilder()
            .header("Authorization", "Bearer " + newToken.accessToken)
            .build();
    }
}
```

---

**Tổng cộng: 17 Controllers, 160+ Endpoints**

---

# 📋 Changelog

## 2025-12-10
- **[NEW]** Thêm tự động Geocoding cho Transport Companies
  - Backend gọi Google Maps Geocoding API khi tạo/sửa nhà xe
  - Response bao gồm `latitude` và `longitude` sẵn sàng cho Google Maps
  - Files thay đổi: `TransportCompany.cs`, `CompanyService.cs`, `GeocodingService.cs`
  - Migration: `AddCompanyCoordinates`
