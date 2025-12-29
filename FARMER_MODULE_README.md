# 🌾 FARMER MODULE - MODELS DOCUMENTATION

## 📋 Tổng quan
Đây là **13 models** hoàn chỉnh cho **Phân hệ Nông Dân (Farmer Module)** của dự án **AgriLink (Rẫy Số)**.

### ✅ Điểm quan trọng
- ✅ **Tất cả Primary Key & Foreign Key đều dùng `Guid`** (không dùng INT/SERIAL)
- ✅ **Snake_case naming** cho tất cả bảng và cột (PostgreSQL convention)
- ✅ **Data Annotations** đầy đủ cho EF Core Migration
- ✅ **Navigation Properties** đã được thiết lập đầy đủ
- ✅ **Enum to String conversion** cho PostgreSQL
- ✅ **Indexes & Unique Constraints** đã được cấu hình
- ✅ **Seed Data** cho 3 loại cây trồng mặc định

---

## 📦 Cấu trúc Models (13 Bảng)

### **NHÓM I: HỆ THỐNG & CẤU HÌNH (3 Bảng)**

#### 1️⃣ `Product` (products)
**Mục đích:** Danh mục cây trồng hệ thống (Dữ liệu tĩnh, Admin tạo sẵn)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `name` | string(100) | Tên cây trồng: "Cà phê Robusta", "Hồ Tiêu" |
| `unit` | string(20) | Đơn vị: "kg" (default) |
| `code` | string(20) | Mã code: "CF_ROBUSTA", "PEPPER" (UNIQUE) |

**Seed Data:**
- Cà phê Robusta (`CF_ROBUSTA`)
- Hồ Tiêu (`PEPPER`)
- Sầu Riêng (`DURIAN`)

---

#### 2️⃣ `Farm` (farms)
**Mục đích:** Hồ sơ Vườn/Rẫy - Tài sản đất đai của nông dân

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `owner_user_id` | Guid | FK → Users (Tài khoản App) |
| `name` | string(100) | Tên rẫy: "Rẫy Đắk Mil", "Vườn Sau Nhà" |
| `area_size` | decimal(10,2) | Diện tích (Hecta): 2.5 |
| `address_gps` | string(50) | GPS: "12.3456, 108.4567" |
| `created_at` | DateTime | Ngày tạo |

**Index:** `ix_farms_owner_user_id` trên `owner_user_id`

---

#### 3️⃣ `TaskType` (task_types)
**Mục đích:** Đơn giá công việc mẫu (Giúp nhập liệu nhanh)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `farm_id` | Guid | FK → farms |
| `name` | string(100) | Tên công việc: "Làm cành", "Hái khoán" |
| `default_unit` | string(20) | Đơn vị: "CONG", "KG", "GOC" |
| `default_price` | decimal(15,2) | Giá gợi ý: 350000 |

---

### **NHÓM II: SẢN XUẤT & CHI PHÍ (6 Bảng)**

#### 4️⃣ `CropSeason` (crop_seasons) ⭐
**Mục đích:** Niên vụ - **TRÁI TIM CỦA HỆ THỐNG**. Tách bạch chi phí xen canh.

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `farm_id` | Guid | FK → farms |
| `product_id` | Guid | FK → products (⚠️ QUAN TRỌNG: Vụ này của cây gì?) |
| `name` | string(100) | Tên vụ: "Vụ Cà 2025", "Vụ Tiêu 2025" |
| `start_date` | DateTime? | Ngày bắt đầu |
| `end_date` | DateTime? | Ngày kết thúc |
| `status` | SeasonStatus | 'ACTIVE', 'CLOSED' (Enum → String) |
| `note` | string? | Ghi chú |

**Index:** `ix_crop_seasons_farm_product` trên `(farm_id, product_id)`

**💡 Lưu ý:** Một farm có thể có nhiều vụ chạy song song (xen canh)!

---

#### 5️⃣ `Worker` (workers)
**Mục đích:** Danh sách nhân công

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `farm_id` | Guid | FK → farms |
| `full_name` | string(100) | Tên: "Chú Bảy", "Tèo" |
| `phone` | string(20) | SĐT |
| `worker_type` | WorkerType | 'PERMANENT' (thợ ruột), 'SEASONAL' (thời vụ) |
| `is_active` | bool | Còn làm việc? |

**Index:** `ix_workers_farm_active` trên `(farm_id, is_active)`

---

#### 6️⃣ `WorkSession` (work_sessions)
**Mục đích:** Nhật ký làm việc - **HEADER** (Ghi nhận: Hôm nay làm gì?)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `season_id` | Guid | FK → crop_seasons |
| `work_date` | DateTime | Ngày làm việc |
| `task_name` | string(100) | Tên công việc: "Làm cành đợt 1" |
| `total_cost` | decimal(15,2) | **AUTO SUM** từ chi tiết |
| `note` | string? | Ghi chú: "Làm khu vực suối" |

**Index:** `ix_work_sessions_season_date` trên `(season_id, work_date)`

**⚠️ Trigger:** Tính `total_cost` khi thêm/sửa/xóa `work_session_details`

---

#### 7️⃣ `WorkSessionDetail` (work_session_details)
**Mục đích:** Chi tiết lương - **LINES** (Ai làm? Trả bao nhiêu?)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `session_id` | Guid | FK → work_sessions (CASCADE DELETE) |
| `worker_id` | Guid | FK → workers |
| `payment_method` | PaymentMethod | 'DAILY' (công nhật), 'PRODUCT' (khoán) |
| `quantity` | decimal(10,2) | 1 (ngày), 0.5 (buổi), 200 (kg) |
| `unit_price` | decimal(15,2) | Giá đơn vị |
| `total_amount` | decimal(15,2) | = quantity × unit_price |

**Cascade Delete:** Xóa WorkSession → Xóa tất cả details

---

#### 8️⃣ `WorkerAdvance` (worker_advances)
**Mục đích:** Sổ ứng lương - Quản lý công nợ thợ

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `worker_id` | Guid | FK → workers |
| `season_id` | Guid | FK → crop_seasons |
| `amount` | decimal(15,2) | Số tiền ứng: 500,000 |
| `advance_date` | DateTime | Ngày ứng |
| `is_deducted` | bool | Đã trừ lương? |
| `note` | string? | Ghi chú |

---

#### 9️⃣ `MaterialUsage` (material_usages)
**Mục đích:** Vật tư (Phân/Thuốc) - Nhật ký sử dụng

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `season_id` | Guid | FK → crop_seasons |
| `usage_date` | DateTime | Ngày bón |
| `material_name` | string(150) | Tên: "NPK 16-16-8 Đầu Trâu" |
| `quantity` | decimal(10,2) | Số lượng: 5.5 |
| `unit` | string(20) | Đơn vị: "Bao", "Lít" |
| `unit_price` | decimal(15,2) | Giá mua vào |
| `total_cost` | decimal(15,2) | Thành tiền |
| `note` | string? | Ghi chú |

---

### **NHÓM III: DOANH THU & KẾT QUẢ (4 Bảng)**

#### 🔟 `HarvestSession` (harvest_sessions)
**Mục đích:** Phiếu cân thu hoạch - **HEADER** (Tổng kết sản lượng ngày hái)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `season_id` | Guid | FK → crop_seasons |
| `harvest_date` | DateTime | Ngày hái |
| `total_bags` | int | **AUTO** tổng số bao |
| `total_weight` | decimal(10,2) | **AUTO** tổng kg |
| `storage_location` | string(50) | Nơi chứa: "KHO_NHA", "DAI_LY_A" |

**Index:** `ix_harvest_sessions_season_date` trên `(season_id, harvest_date)`

**⚠️ Trigger:** Tính `total_bags`, `total_weight` từ `harvest_bag_details`

---

#### 1️⃣1️⃣ `HarvestBagDetail` (harvest_bag_details)
**Mục đích:** Chi tiết bao - **LINES** (Cân từng bao tại rẫy)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `session_id` | Guid | FK → harvest_sessions (CASCADE DELETE) |
| `bag_index` | int | STT bao: 1, 2, 3 |
| `gross_weight` | decimal(10,2) | Cân cả bì: 50.5 |
| `deduction` | decimal(10,2) | Trừ bì: 0.5 (default) |
| `net_weight` | decimal(10,2) | = gross_weight - deduction |

**Cascade Delete:** Xóa HarvestSession → Xóa tất cả bags

---

#### 1️⃣2️⃣ `FarmSale` (farm_sales)
**Mục đích:** Sổ bán hàng - Doanh thu (Cash flow thực tế)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `season_id` | Guid | FK → crop_seasons |
| `sale_date` | DateTime | Ngày bán |
| `buyer_name` | string(100) | Tên khách: "Đại lý Tuấn" |
| `quantity_sold` | decimal(10,2) | Bán: 1000 kg |
| `price_per_kg` | decimal(15,2) | Giá: 100,000 |
| `total_revenue` | decimal(15,2) | Tổng thu: 100,000,000 |
| `note` | string? | Ghi chú: "Tiền mặt", "Chuyển khoản" |

**Index:** `ix_farm_sales_season_date` trên `(season_id, sale_date)`

---

#### 1️⃣3️⃣ `WeatherLog` (weather_logs)
**Mục đích:** Nhật ký thời tiết - Lưu lịch sử môi trường

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `id` | Guid | Primary Key |
| `farm_id` | Guid | FK → farms |
| `log_date` | DateTime | Ngày ghi nhận |
| `condition` | WeatherCondition | 'NANG', 'MUA', 'AM_U' (Enum → String) |
| `rainfall_mm` | decimal(5,2) | Lượng mưa (mm) |
| `note` | string? | Ghi chú: "Mưa đá rụng trái" |

---

## 🔧 Enums Đã Tạo

```csharp
// 1. SeasonStatus
public enum SeasonStatus { ACTIVE, CLOSED }

// 2. WorkerType
public enum WorkerType { PERMANENT, SEASONAL }

// 3. PaymentMethod
public enum PaymentMethod { DAILY, PRODUCT }

// 4. WeatherCondition
public enum WeatherCondition { NANG, MUA, AM_U }
```

**⚠️ Lưu ý:** Tất cả Enum đều được convert sang STRING trong PostgreSQL.

---

## 📊 Quan hệ quan trọng

```
Farm (1) ──→ (N) CropSeason
Product (1) ──→ (N) CropSeason
CropSeason (1) ──→ (N) WorkSession ──→ (N) WorkSessionDetail
CropSeason (1) ──→ (N) HarvestSession ──→ (N) HarvestBagDetail
CropSeason (1) ──→ (N) FarmSale
CropSeason (1) ──→ (N) MaterialUsage
CropSeason (1) ──→ (N) WorkerAdvance

Farm (1) ──→ (N) Worker ──→ (N) WorkSessionDetail
Farm (1) ──→ (N) WeatherLog
Farm (1) ──→ (N) TaskType
```

---

## 🎯 Hướng dẫn chạy Migration

### **Bước 1: Kiểm tra cấu hình `appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=agrilink_db;Username=postgres;Password=yourpassword"
  }
}
```

### **Bước 2: Add Migration**
```bash
dotnet ef migrations add InitialFarmerModule --project AgriLink_DH.Core --startup-project AgriLink_DH.Api
```

### **Bước 3: Update Database**
```bash
dotnet ef database update --project AgriLink_DH.Core --startup-project AgriLink_DH.Api
```

### **Bước 4: Kiểm tra Seed Data**
Sau khi migration, check bảng `products`:
```sql
SELECT * FROM products;
```

Kết quả mong đợi:
| id | name | unit | code |
|----|------|------|------|
| 00000000-0000-0000-0000-000000000001 | Cà phê Robusta | kg | CF_ROBUSTA |
| 00000000-0000-0000-0000-000000000002 | Hồ Tiêu | kg | PEPPER |
| 00000000-0000-0000-0000-000000000003 | Sầu Riêng | kg | DURIAN |

---

## 💡 Lưu ý cho DEV

### **1. Tính toán tự động**
- `WorkSession.total_cost`: Tính bằng **Trigger** hoặc **Code Backend** khi thêm/sửa/xóa `WorkSessionDetail`
- `HarvestSession.total_bags`, `total_weight`: Tính từ `HarvestBagDetail`
- `HarvestBagDetail.net_weight`: = `gross_weight - deduction`

### **2. Đa tiền tệ**
Tất cả cột `decimal(15,2)` mặc định là **VNĐ**. Nếu muốn mở rộng, thêm cột `currency` sau.

### **3. Quan hệ Season-Product**
Khi tạo `CropSeason`, **BẮT BUỘC** phải chọn `product_id`. Điều này giúp query báo cáo cực kỳ dễ dàng:
```csharp
// Tổng chi phí cây Cà phê trong 5 năm
var totalCost = await _context.CropSeasons
    .Where(cs => cs.Product.Code == "CF_ROBUSTA" 
              && cs.StartDate >= DateTime.Now.AddYears(-5))
    .SelectMany(cs => cs.WorkSessions)
    .SumAsync(ws => ws.TotalCost);
```

### **4. Cascade Delete**
- Xóa `WorkSession` → Tự động xóa tất cả `WorkSessionDetail`
- Xóa `HarvestSession` → Tự động xóa tất cả `HarvestBagDetail`

### **5. Performance Tips**
- Đã tạo **8 indexes** quan trọng cho query nhanh
- Unique constraint trên `products.code`
- Composite indexes trên các cặp `(farm_id, product_id)`, `(season_id, work_date)`, etc.

---

## ✅ Checklist hoàn thành

- [x] 13 Models với Guid primary keys
- [x] 4 Enums với string conversion
- [x] Data Annotations đầy đủ (snake_case)
- [x] Navigation Properties
- [x] DbContext với indexes & constraints
- [x] Seed data cho Products
- [x] Cascade delete configuration
- [x] README documentation

---

## 📞 Support

Nếu có vấn đề gì, liên hệ team Backend để debug migration! 🚀

**Happy Farming! 🌾☕🌶️**
