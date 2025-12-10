# WeDeli API - Tổng hợp Endpoints cho App

**Base URL:** `https://your-server/api/v1`

---

## 🔐 AUTHENTICATION

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/auth/register` | Đăng ký tài khoản mới |
| POST | `/auth/login` | Đăng nhập, nhận JWT token |
| POST | `/auth/refresh-token` | Làm mới access token |
| POST | `/auth/logout` | Đăng xuất |
| GET | `/auth/profile` | Lấy thông tin user hiện tại |

---

## 🏢 TRANSPORT COMPANIES (Nhà xe)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/partnerships/companies` | Danh sách nhà xe (có tọa độ GPS) |
| GET | `/partnerships/companies/{id}` | Chi tiết nhà xe |

### Response mẫu:
```json
{
  "companyId": 1,
  "companyName": "Nhà xe Thành Bưởi",
  "address": "266 Lê Hồng Phong...",
  "phone": "1900 6067",
  "email": "contact@thanhbuoi.vn",
  "latitude": 10.7567890,
  "longitude": 106.6789012,
  "rating": 4.5,
  "isActive": true
}
```

---

## 📦 ORDERS (Đơn hàng)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/orders` | Danh sách đơn hàng |
| GET | `/orders/{id}` | Chi tiết đơn hàng |
| POST | `/orders` | Tạo đơn hàng mới |
| PUT | `/orders/{id}` | Cập nhật đơn hàng |
| DELETE | `/orders/{id}` | Xóa đơn hàng |
| PUT | `/orders/{id}/status` | Cập nhật trạng thái |
| GET | `/orders/{id}/tracking` | Theo dõi đơn hàng |
| GET | `/orders/customer/{customerId}` | Đơn hàng theo khách hàng |
| GET | `/orders/driver/{driverId}` | Đơn hàng theo tài xế |

---

## 🚗 VEHICLES (Xe)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/vehicles` | Danh sách xe |
| GET | `/vehicles/{id}` | Chi tiết xe |
| POST | `/vehicles` | Thêm xe mới |
| PUT | `/vehicles/{id}` | Cập nhật xe |
| DELETE | `/vehicles/{id}` | Xóa xe |
| GET | `/vehicles/company/{companyId}` | Xe theo công ty |
| GET | `/vehicles/available` | Xe còn trống |

---

## 🛣️ ROUTES (Tuyến đường)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/routes` | Danh sách tuyến |
| GET | `/routes/{id}` | Chi tiết tuyến |
| POST | `/routes` | Thêm tuyến mới |
| PUT | `/routes/{id}` | Cập nhật tuyến |
| DELETE | `/routes/{id}` | Xóa tuyến |
| GET | `/routes/company/{companyId}` | Tuyến theo công ty |

---

## 👤 CUSTOMERS (Khách hàng)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/customers` | Danh sách khách hàng |
| GET | `/customers/{id}` | Chi tiết khách hàng |
| POST | `/customers` | Thêm khách hàng |
| PUT | `/customers/{id}` | Cập nhật khách hàng |
| GET | `/customers/{id}/addresses` | Địa chỉ của khách hàng |

---

## 🚚 DRIVERS (Tài xế)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/drivers` | Danh sách tài xế |
| GET | `/drivers/{id}` | Chi tiết tài xế |
| POST | `/drivers` | Thêm tài xế |
| PUT | `/drivers/{id}` | Cập nhật tài xế |
| GET | `/drivers/company/{companyId}` | Tài xế theo công ty |
| GET | `/drivers/available` | Tài xế còn trống |

---

## 💰 COD (Thu hộ)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/cod/transactions` | Danh sách giao dịch COD |
| GET | `/cod/transactions/{id}` | Chi tiết giao dịch |
| POST | `/cod/collect` | Thu tiền COD |
| POST | `/cod/transfer` | Chuyển tiền về công ty |

---

## 📊 REPORTS (Báo cáo)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/reports/dashboard` | Dashboard tổng quan |
| GET | `/reports/revenue` | Báo cáo doanh thu |
| GET | `/reports/orders` | Báo cáo đơn hàng |

---

## 🔔 NOTIFICATIONS (Thông báo)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/notifications` | Danh sách thông báo |
| PUT | `/notifications/{id}/read` | Đánh dấu đã đọc |
| PUT | `/notifications/read-all` | Đọc tất cả |

---

## 🔑 AUTHENTICATION HEADER

Tất cả API (trừ login/register) cần header:
```
Authorization: Bearer <your_jwt_token>
```

---

## 📱 ANDROID/iOS INTEGRATION

### Retrofit Interface Example:
```kotlin
interface WedeliApi {
    @GET("partnerships/companies")
    suspend fun getCompanies(): ApiResponse<List<CompanyResponse>>
    
    @GET("partnerships/companies/{id}")
    suspend fun getCompanyDetails(@Path("id") id: Int): ApiResponse<CompanyResponse>
    
    @GET("orders")
    suspend fun getOrders(): ApiResponse<List<OrderResponse>>
}
```

### CompanyResponse DTO:
```kotlin
data class CompanyResponse(
    val companyId: Int,
    val companyName: String,
    val address: String?,
    val phone: String?,
    val email: String?,
    val latitude: Double?,   // Tọa độ GPS
    val longitude: Double?,  // Tọa độ GPS
    val rating: Double,
    val isActive: Boolean
)
```

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **GPS Coordinates**: `latitude` và `longitude` có thể null nếu chưa geocode
2. **Pagination**: Hầu hết list endpoints hỗ trợ `?page=1&pageSize=10`
3. **Error Response**: 
```json
{
  "success": false,
  "message": "Error message here",
  "data": null
}
```
4. **Success Response**:
```json
{
  "success": true,
  "message": "Success message",
  "data": { ... }
}
```
