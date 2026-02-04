# 🔐 Phân tích & Đề xuất Hệ thống Phân quyền - AgriLink

## 📊 Hiện trạng

### User Roles hiện có:
```csharp
public enum UserRole
{
    User,        // Người dùng thường
    Farmer,      // Nông dân - Chủ vườn
    Accountant,  // Kế toán
    Admin        // Quản trị viên
}
```

### Farm Model:
```csharp
public class Farm
{
    public Guid OwnerUserId { get; set; }  // ← Quan trọng: Mỗi farm thuộc 1 user
    // ... other fields
}
```

### Logic hiện tại (FarmService):
- `GetAllFarmsAsync()` - Lấy **TẤT CẢ** farms (không phân quyền)
- `GetFarmsByUserIdAsync(userId)` - Lấy farms của **1 user cụ thể**
- `GetFarmsWithActiveSeasonsAsync(userId)` - Lấy farms + seasons của **1 user**

---

## 🎯 Đề xuất Hệ thống Phân quyền

### Model phân quyền đề xuất:

```
┌─────────────┬──────────────────────────┬─────────────────────────┐
│    Role     │    Quyền xem Farms       │    Quyền thao tác       │
├─────────────┼──────────────────────────┼─────────────────────────┤
│ Admin       │ • TẤT CẢ farms           │ • CRUD tất cả farms     │
│             │ • Tất cả users           │ • Quản lý users         │
│             │                          │ • Xem reports tổng      │
├─────────────┼──────────────────────────┼─────────────────────────┤
│ Farmer      │ • Farms của mình         │ • CRUD farms của mình   │
│ (Owner)     │ • Farms được share cho   │ • Quản lý workers       │
│             │                          │ • Quản lý seasons       │
├─────────────┼──────────────────────────┼─────────────────────────┤
│ Accountant  │ • Farms được gán         │ • Xem chi phí           │
│             │                          │ • Xuất báo cáo tài chính│
│             │                          │ • KHÔNG sửa farm info   │
├─────────────┼──────────────────────────┼─────────────────────────┤
│ User        │ • Farms của mình (nếu có)│ • Xem readonly          │
│             │                          │ • Không tạo/sửa/xóa     │
└─────────────┴──────────────────────────┴─────────────────────────┘
```

---

## 🏗️ Triển khai

### 1. Cập nhật FarmService

#### Option 1: Thêm method mới với Role-Based Access
```csharp
public async Task<IEnumerable<FarmDto>> GetFarmsBasedOnRoleAsync(Guid userId, UserRole role)
{
    if (role == UserRole.Admin)
    {
        // Admin: Lấy TẤT CẢ farms
        return await GetAllFarmsAsync();
    }
    else
    {
        // Farmer/User/Accountant: Chỉ farms của họ
        return await GetFarmsByUserIdAsync(userId);
    }
}
```

#### Option 2: Tạo Authorization Policy
```csharp
// Trong FarmController
[Authorize(Roles = "Admin")]
public async Task<ActionResult<IEnumerable<FarmDto>>> GetAllFarms()
{
    var farms = await _farmService.GetAllFarmsAsync();
    return Ok(farms);
}

[Authorize] // Tất cả roles
public async Task<ActionResult<IEnumerable<FarmDto>>> GetMyFarms()
{
    var userId = GetCurrentUserId();
    var farms = await _farmService.GetFarmsByUserIdAsync(userId);
    return Ok(farms);
}
```

### 2. Cập nhật Models (Optional - Farm Sharing)

Nếu muốn **share farms** giữa users:

```csharp
[Table("farm_permissions")]
public class FarmPermission
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public Guid UserId { get; set; }
    public FarmPermissionType PermissionType { get; set; } // View, Edit, FullControl
    public DateTime GrantedAt { get; set; }
    
    public Farm Farm { get; set; }
    public User User { get; set; }
}

public enum FarmPermissionType
{
    View,        // Chỉ xem
    Edit,        // Xem + chỉnh sửa
    FullControl  // Tất cả quyền (như owner)
}
```

### 3. Cập nhật UI Routes

#### FarmsPage.jsx
```javascript
// Gọi API dựa trên role
const fetchFarms = async () => {
  const userRole = getUserRole(); // Lấy từ auth context
  
  if (userRole === 'Admin') {
    // Admin: Gọi endpoint lấy ALL farms
    const response = await farmService.getAllFarms();
  } else {
    // Farmer/User: Gọi endpoint lấy MY farms
    const response = await farmService.getMyFarms();
  }
};
```

---

## 📱 Trang Admin

### Nên có trang Admin riêng? **CÓ!**

#### Layout Structure:
```
/admin
├── /dashboard         ← Tổng quan toàn hệ thống
├── /users            ← Quản lý users
├── /farms            ← Quản lý TẤT CẢ farms
├── /reports          ← Báo cáo tổng hợp
└── /settings         ← Cài đặt hệ thống

/ (normal user)
├── /dashboard         ← Dashboard cá nhân
├── /farms            ← Farms của mình
├── /products         ← Sản phẩm
└── /profile          ← Thông tin cá nhân
```

#### Routing với Role:
```javascript
// MainRoutes.jsx
const routes = [
  {
    path: '/admin',
    element: <AdminLayout />,
    guard: <AdminGuard />, // Chỉ Admin
    children: [
      { path: 'dashboard', element: <AdminDashboard /> },
      { path: 'users', element: <UsersManagement /> },
      { path: 'farms', element: <AllFarmsPage /> },
    ]
  },
  {
    path: '/',
    element: <MainLayout />,
    guard: <AuthGuard />, // All authenticated users
    children: [
      { path: 'farms', element: <MyFarmsPage /> },
      { path: 'products', element: <ProductsPage /> },
    ]
  }
];
```

---

## 🔒 Security Best Practices

### Backend (.NET)
```csharp
// 1. Authorization Attribute
[Authorize(Roles = "Admin")]
[HttpGet("all")]
public async Task<ActionResult> GetAllFarms() { }

// 2. Policy-Based Authorization
[Authorize(Policy = "FarmOwnerPolicy")]
[HttpPut("{id}")]
public async Task<ActionResult> UpdateFarm(Guid id) { }

// 3. Check trong Service
public async Task<FarmDto> UpdateFarmAsync(Guid id, Guid userId, UpdateFarmDto dto)
{
    var farm = await _farmRepository.GetByIdAsync(id);
    
    // Kiểm tra quyền
    if (farm.OwnerUserId != userId && !IsAdmin(userId))
    {
        throw new UnauthorizedAccessException("Bạn không có quyền sửa farm này");
    }
    
    // ... update logic
}
```

### Frontend (React)
```javascript
// AuthContext.js
const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  return context;
};

// PrivateRoute.jsx
const AdminRoute = ({ children }) => {
  const { user } = useAuth();
  
  if (user?.role !== 'Admin') {
    return <Navigate to="/" />;
  }
  
  return children;
};

// Usage
<Route path="/admin/*" element={
  <AdminRoute>
    <AdminLayout />
  </AdminRoute>
} />
```

---

## 📋 Action Items

### Phase 1: Backend Authorization
- [ ] Thêm `[Authorize]` attributes vào controllers
- [ ] Tạo method `GetFarmsBasedOnRoleAsync()` trong FarmService
- [ ] Implement farm ownership check trong Update/Delete
- [ ] Tạo Admin endpoints riêng (`/api/admin/farms`)

### Phase 2: Frontend Routes
- [ ] Tạo `AuthContext` và `useAuth` hook
- [ ] Implement route guards (AdminRoute, PrivateRoute)
- [ ] Tạo AdminLayout riêng
- [ ] Điều hướng dựa trên role sau login

### Phase 3: Admin Dashboard
- [ ] Tạo trang `/admin/dashboard`
- [ ] Trang quản lý users (`/admin/users`)
- [ ] Trang xem tất cả farms (`/admin/farms`)
- [ ] Báo cáo tổng hợp (`/admin/reports`)

### Phase 4: Enhanced Features (Optional)
- [ ] Farm sharing/permissions system
- [ ] Activity logs (audit trail)
- [ ] Email notifications
- [ ] Role-based UI hiding (ẩn buttons dựa trên quyền)

---

## 🎯 Kết luận

### Câu trả lời cho câu hỏi của bạn:

1. **Có nên có trang Admin không?**
   - ✅ **CÓ** - Tạo layout riêng cho Admin tại `/admin`

2. **Admin được xem tất cả farms?**
   - ✅ **ĐÚNG** - Admin call `GetAllFarms()` để xem tất cả
   - Farmers chỉ xem farms của họ qua `GetMyFarms()`

3. **User thường chỉ xem farms của mình?**
   - ✅ **ĐÚNG** - Filter theo `OwnerUserId`
   - Có thể mở rộng với farm sharing nếu cần

### Recommended Architecture:
```
Admin Portal (/admin)     Farmer Portal (/)
     ↓                           ↓
GetAllFarms()            GetFarmsByUserId()
     ↓                           ↓
View ALL farms          View OWNED farms only
```

---

**Next Step**: Bạn muốn implement phần nào trước?
1. Backend Authorization (Recommend bắt đầu từ đây)
2. Frontend Route Guards
3. Admin Dashboard
