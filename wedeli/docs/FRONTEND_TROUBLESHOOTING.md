# 🔧 Troubleshooting Guide - WeDeli Android App
## Dành cho Frontend Team

---

## 📋 THÔNG TIN BACKEND

### Base URL
```
Production: https://your-server/api/v1
Local Dev:  http://localhost:5000/api/v1
            http://10.0.2.2:5000/api/v1 (Android Emulator)
```

### Test Account (Seed Data)
```
Username: thanhbuoi_admin_seed
Password: Password123!
CompanyId: 1
CompanyName: Nhà xe Thành Bưởi
```

---

## ✅ BACKEND DATA STATUS

| Bảng | Có Data | Ghi chú |
|------|---------|---------|
| TransportCompany | ✅ 3 records | Có `latitude`, `longitude` |
| Vehicle | ✅ 5 records | CompanyId 1 có 2 xe |
| Driver | ✅ 3 records | CompanyId 1 có 2 tài xế |
| VehicleLocation | ⚠️ EMPTY | Cần gọi API update để có data |
| Order | ✅ có data | Seed data có sẵn |

---

## 🚨 VẤN ĐỀ THƯỜNG GẶP

### 1. Không lấy được Dashboard
**Endpoint đúng:** `/api/Dashboard/company/{companyId}` (KHÔNG có `/v1/`)

```java
// SAI
@GET("Dashboard/company/{companyId}")

// ĐÚNG - dùng relative path
@GET("../Dashboard/company/{companyId}")

// HOẶC tạo riêng Retrofit cho Dashboard
String DASHBOARD_URL = "https://your-server/api/";
```

### 2. Không lấy được vị trí nhà xe
**Endpoint:** `GET /partnerships/companies/{id}`

```java
// Response chứa latitude, longitude
{
  "companyId": 1,
  "companyName": "Nhà xe Thành Bưởi",
  "latitude": 10.7567890,  // ← Tọa độ nhà xe
  "longitude": 106.6789012,
  ...
}
```

### 3. Không lấy được danh sách xe
**Endpoint:** `GET /vehicles?companyId=1`

```java
// Phải truyền companyId trong query
@GET("vehicles")
Call<ApiResponse<List<Vehicle>>> getVehicles(@Query("companyId") int companyId);
```

### 4. Không lấy được vị trí xe (VehicleLocation)
**⚠️ VẤN ĐỀ:** Bảng `VehicleLocation` KHÔNG có seed data!

**Endpoint:** `GET /vehicle-locations/company/{companyId}`

**Giải pháp:**
1. Driver app phải gọi API cập nhật vị trí trước:
```json
POST /vehicle-locations/update
{
  "vehicleId": 1,
  "latitude": 10.8231,
  "longitude": 106.6297,
  "speed": 45.5
}
```

2. Sau đó mới có data để lấy:
```json
GET /vehicle-locations/company/1

// Response
{
  "success": true,
  "data": {
    "vehicles": [
      {
        "vehicleId": 1,
        "latitude": 10.8231,
        "longitude": 106.6297
      }
    ]
  }
}
```

### 5. Không lấy được thông tin cá nhân
**Endpoint:** `GET /auth/profile`
**Headers:** `Authorization: Bearer <accessToken>`

```java
@GET("auth/profile")
Call<ApiResponse<UserProfile>> getProfile();
```

---

## 🔍 DEBUG CHECKLIST

### Bước 1: Kiểm tra companyId
```java
// Trong Android, log ra companyId
Log.d("DEBUG", "CompanyId: " + SharedPrefsManager.getCompanyId());
```

**⚠️ Nếu companyId = -1 hoặc 0:**
- Chưa lưu sau khi login
- Cần gọi `/auth/profile` và lưu `companyId`

### Bước 2: Kiểm tra Authorization header
```java
// Log request headers
HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
logging.setLevel(HttpLoggingInterceptor.Level.HEADERS);
```

### Bước 3: Filter Logcat
```
Tag: OkHttp      - Xem request/response
Tag: Retrofit    - Xem network calls
Tag: HomeViewModel
Tag: HomeFragment
```

---

## 📱 ANDROID CODE FIX

### Fix Dashboard call
```java
// Tạo base URL riêng cho Dashboard
public class DashboardClient {
    private static final String BASE_URL = "https://your-server/api/";
    
    public interface DashboardApi {
        @GET("Dashboard/company/{companyId}")
        Call<DashboardStats> getCompanyDashboard(@Path("companyId") int id);
    }
}
```

### Fix Vehicle Location call
```java
// Đảm bảo route đúng: vehicle-locations (có dấu gạch)
@GET("vehicle-locations/company/{companyId}")
Call<ApiResponse<CompanyVehicleLocations>> getCompanyVehicleLocations(
    @Path("companyId") int companyId
);
```

### Fix sau Login
```java
// Sau khi login thành công
RetrofitClient.setAuthToken(response.getAccessToken());
SharedPrefsManager.saveCompanyId(response.getCompanyId());
SharedPrefsManager.saveCompanyName(response.getCompanyName());

// Sau đó gọi profile để confirm
RetrofitClient.getApiService().getProfile().enqueue(...);
```

---

## 🧪 TEST ENDPOINTS VỚI CURL

```bash
# 1. Login
curl -X POST https://your-server/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrUsername":"thanhbuoi_admin_seed","password":"Password123!"}'

# 2. Company Details (có tọa độ)
curl -X GET https://your-server/api/v1/partnerships/companies/1 \
  -H "Authorization: Bearer <token>"

# 3. Vehicles
curl -X GET "https://your-server/api/v1/vehicles?companyId=1" \
  -H "Authorization: Bearer <token>"

# 4. Vehicle Locations
curl -X GET https://your-server/api/v1/vehicle-locations/company/1 \
  -H "Authorization: Bearer <token>"

# 5. Dashboard (LƯU Ý: không có v1)
curl -X GET https://your-server/api/Dashboard/company/1 \
  -H "Authorization: Bearer <token>"
```

---

## 📊 EXPECTED RESPONSES

### /partnerships/companies/1
```json
{
  "success": true,
  "data": {
    "companyId": 1,
    "companyName": "Nhà xe Thành Bưởi",
    "address": "266 Lê Hồng Phong, Phường 4, Quận 5, TP.HCM",
    "latitude": 10.7567890,
    "longitude": 106.6789012,
    "rating": 4.5,
    "isActive": true
  }
}
```

### /vehicles?companyId=1
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "vehicleId": 1,
        "licensePlate": "51A-12345",
        "vehicleType": "truck",
        "currentStatus": "available"
      },
      {
        "vehicleId": 2,
        "licensePlate": "51A-23456",
        "vehicleType": "van",
        "currentStatus": "in_transit"
      }
    ]
  }
}
```

### /Dashboard/company/1
```json
{
  "totalOrders": 10,
  "pendingOrders": 2,
  "inTransitOrders": 3,
  "deliveredOrders": 5,
  "totalRevenue": 5000000.0,
  "todayRevenue": 500000.0,
  "activeVehicles": 2,
  "activeDrivers": 2
}
```

---

## ❓ CẦN HỖ TRỢ

Nếu vẫn gặp lỗi, cung cấp:
1. **Logcat output** với filter `OkHttp` hoặc `HomeViewModel`
2. **HTTP Status Code** (404, 401, 500, etc.)
3. **Response body** từ server
4. **CompanyId** đang dùng

---

*Cập nhật: 2024-12-13*
