# 📋 TRẢ LỜI CÂU HỎI FRONTEND

## 1. Liên kết User-Company

**User CompanyAdmin được liên kết với TransportCompany qua field `userId` trong bảng `transport_companies`.**

```
TransportCompany
├── company_id (PK)
├── company_name
├── user_id (FK) ← Liên kết với bảng Users
├── latitude
├── longitude
└── ...
```

**KHÔNG phải** qua companyId trong bảng Users (bảng Users không có companyId).

---

## 2. Nếu Login trả về companyId: null

### ✅ ĐÃ SỬA - Backend sẽ trả về CompanyId khi login

**Thay đổi đã thực hiện:**
- Thêm `GetByUserIdAsync()` vào `TransportCompanyRepository`
- Cập nhật `AuthService.LoginAsync()` để lấy CompanyId từ TransportCompany

**Login Response mới (CompanyAdmin):**
```json
{
  "success": true,
  "data": {
    "userId": 15,
    "username": "thanhbuoi_admin_seed",
    "roleName": "CompanyAdmin",
    "roleId": 2,
    "companyId": 1,           // ← ĐƯỢC TRẢ VỀ
    "companyName": "Nhà xe Thành Bưởi",  // ← ĐƯỢC TRẢ VỀ
    "accessToken": "...",
    "refreshToken": "..."
  }
}
```

### API thay thế nếu companyId vẫn null

Dùng endpoint profile:
```
GET /api/v1/users/me
Authorization: Bearer <accessToken>
```

Response có chứa `companyId` và `companyName`.

---

## 3. Seed Data

### Có đúng! Seed data liên kết chính xác:

| User | userId | username | companyId | companyName |
|------|--------|----------|-----------|-------------|
| CompanyAdmin 1 | 15 | `thanhbuoi_admin_seed` | 1 | Nhà xe Thành Bưởi |
| CompanyAdmin 2 | 16 | `phuongtrang_admin_seed` | 2 | Nhà xe Phương Trang |
| CompanyAdmin 3 | 17 | `hoanglong_admin_seed` | 3 | Nhà xe Hoàng Long |

### Test Account:
```
Username: thanhbuoi_admin_seed
Password: Password123!
Expected companyId: 1
Expected companyName: "Nhà xe Thành Bưởi"
```

---

## 4. API để lấy company nếu companyId là null

### Option 1: Endpoint Profile (khuyến nghị)
```
GET /api/v1/users/me
```
Response chứa `companyId` và `companyName`.

### Option 2: Lấy tất cả companies và match userId
```
GET /api/v1/partnerships/companies
```
Response:
```json
{
  "data": [
    {
      "companyId": 1,
      "companyName": "Nhà xe Thành Bưởi",
      "userId": 15,   // ← Match với userId của user đang login
      "latitude": 10.7567890,
      "longitude": 106.6789012
    }
  ]
}
```

---

## 📝 TÓM TẮT THAY ĐỔI BACKEND

### Files đã sửa:

1. **`ITransportCompanyRepository.cs`** - Thêm interface method
```csharp
Task<TransportCompany?> GetByUserIdAsync(int userId);
```

2. **`TransportCompanyRepository.cs`** - Implement method
```csharp
public async Task<TransportCompany?> GetByUserIdAsync(int userId)
{
    return await _context.TransportCompanies
        .FirstOrDefaultAsync(c => c.UserId == userId);
}
```

3. **`AuthService.cs`** - Populate companyId khi login
```csharp
// Get company info if user is CompanyAdmin (RoleId = 2)
int? companyId = null;
string? companyName = null;
if (user.RoleId == 2) // CompanyAdmin
{
    var company = await _companyRepository.GetByUserIdAsync(user.UserId);
    if (company != null)
    {
        companyId = company.CompanyId;
        companyName = company.CompanyName;
    }
}
```

---

## ⚠️ HÀNH ĐỘNG CẦN THIẾT

1. **Restart backend server** để áp dụng thay đổi
2. **Test login** với account `thanhbuoi_admin_seed`
3. **Kiểm tra response** có `companyId: 1` không

---

*Cập nhật: 2024-12-13*
