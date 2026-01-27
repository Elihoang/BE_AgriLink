# Market Price Management System - Hướng dẫn

## ✅ ĐÃ HOÀN THÀNH

### **1. Database Schema**
- ✅ Bảng `market_price_history` - Lưu lịch sử giá theo ngày
- ✅ Hỗ trợ nhiều sản phẩm (COFFEE, PEPPER, DURIAN, v.v.)
- ✅ Hỗ trợ nhiều khu vực (Đắk Lắk, Lâm Đồng, Gia Lai, Đắk Nông, ...)
- ✅ Tracking change và change_percent

### **2. Backend Services**
- ✅ `MarketPriceDbService` - Quản lý giá từ database
- ✅ `GetLatestPricesAsync()` - API lấy giá mới nhất
- ✅ `UpdatePriceAsync()` - Admin cập nhật giá
- ✅ `GetPriceHistoryAsync()` - Xem lịch sử

### **3. API Endpoints**

#### **Public Endpoints:**
```http
GET /api/MarketPrice
# Lấy giá thị trường mới nhất

GET /api/MarketPrice/history?productType=COFFEE&regionCode=DAK_LAK&limit=30
# Lấy lịch sử giá
```

#### **Admin Endpoints:**
```http
POST /api/MarketPrice/admin/update
Content-Type: application/json

{
  "productType": "COFFEE",
  "productName": "Cà phê Robusta",
  "regionCode": "DAK_LAK",
  "region": "Đắk Lắk",
  "price": 101500,
  "change": 600,
  "source": "giacaphe.com"
}

POST /api/MarketPrice/admin/batch-update
Content-Type: application/json

[
  {
    "productType": "COFFEE",
    "productName": "Cà phê Robusta",
    "regionCode": "DAK_LAK",
    "region": "Đắk Lắk",
    "price": 101500,
    "change": 600
  },
  {
    "productType": "COFFEE",
    "productName": "Cà phê Robusta",
    "regionCode": "LAM_DONG",
    "region": "Lâm Đồng",
    "price": 101200,
    "change": 700
  }
]
```

---

## 🚀 SETUP

### **Bước 1: Tạo bảng trong Database**

Chạy SQL script:

```bash
# Nếu dùng psql
psql -U postgres -d agrilink_db -f create_market_price_history.sql

# Hoặc copy SQL và chạy trực tiếp trong pgAdmin/DBeaver
```

### **Bước 2: Verify Table**

```sql
SELECT * FROM market_price_history ORDER BY recorded_date DESC LIMIT 10;
```

Kết quả mong đợi: 12 records (Cà phê + Hồ tiêu, 4 tỉnh, 2 ngày)

### **Bước 3: Test API**

```powershell
# Test lấy giá mới nhất
Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice"

# Test cập nhật giá (Admin)
$body = @{
    productType = "COFFEE"
    productName = "Cà phê Robusta"
    regionCode = "DAK_LAK"
    region = "Đắk Lắk"
    price = 102000
    change = 1100
    source = "Manual entry"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice/admin/update" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

# Test lịch sử
Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice/history?productType=COFFEE&regionCode=DAK_LAK&limit=10"
```

---

## 📊 USE CASES

### **1. Admin cập nhật giá hàng ngày**

**Workflow:**
1. Admin vào giacaphe.com xem giá mới
2. POST request đến `/admin/batch-update` với 4 tỉnh
3. System lưu vào DB với `recorded_date = today`
4. Frontend tự động cập nhật

**Example Script:**
```powershell
# Tạo file update-prices-today.ps1
$prices = @(
    @{ productType="COFFEE"; productName="Cà phê Robusta"; regionCode="DAK_LAK"; region="Đắk Lắk"; price=102000; change=1100 },
    @{ productType="COFFEE"; productName="Cà phê Robusta"; regionCode="LAM_DONG"; region="Lâm Đồng"; price=101700; change=1200 },
    @{ productType="COFFEE"; productName="Cà phê Robusta"; regionCode="GIA_LAI"; region="Gia Lai"; price=101900; change=1100 },
    @{ productType="COFFEE"; productName="Cà phê Robusta"; regionCode="DAK_NONG"; region="Đắk Nông"; price=102100; change=1100 }
)

$json = $prices | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice/admin/batch-update" `
    -Method POST -Body $json -ContentType "application/json"

Write-Host "✅ Cập nhật giá thành công!"
```

### **2. Xem biểu đồ lịch sử**

Frontend có thể call:
```javascript
// Lấy 30 ngày gần nhất
const history = await fetch('/api/MarketPrice/history?productType=COFFEE&regionCode=DAK_LAK&limit=30');
const data = await history.json();

// data.items chứa array { recordedDate, price, change }
// => Vẽ chart (Line chart, Candlestick, v.v.)
```

### **3. So sánh giá các tỉnh**

```javascript
const regions = ['DAK_LAK', 'LAM_DONG', 'GIA_LAI', 'DAK_NONG'];
const latestPrices = await fetch('/api/MarketPrice');

// latestPrices.data.regionalPrices chứa:
// [
//   { region: "Đắk Lắk", coffeePrice: 102000, change: 1100 },
//   { region: "Lâm Đồng", coffeePrice: 101700, change: 1200 },
//   ...
// ]
```

---

## 🔐 AUTHENTICATION (TODO)

Hiện tại endpoint `/admin/*` chưa có authentication. Cần thêm:

1. **JWT Authentication** (đã có sẵn trong project)
2. **Role-based authorization** (`[Authorize(Roles = "Admin")]`)

---

## 📝 DATABASE SCHEMA DETAILS

```sql
market_price_history (
    id: SERIAL PRIMARY KEY,
    product_type: VARCHAR(50) NOT NULL,        -- COFFEE, PEPPER, DURIAN
    product_name: VARCHAR(100) NOT NULL,       -- Cà phê Robusta, Hồ tiêu
    region: VARCHAR(50),                       -- Đắk Lắk, Lâm Đồng (NULL = National)
    region_code: VARCHAR(20),                  -- DAK_LAK, LAM_DONG (NULL = NATIONAL)
    price: DECIMAL(18,2) NOT NULL,            -- Giá (VND/kg)
    change: DECIMAL(18,2) NOT NULL DEFAULT 0, -- Thay đổi so với hôm trước
    change_percent: DECIMAL(18,2) NOT NULL DEFAULT 0,
    unit: VARCHAR(20) NOT NULL DEFAULT 'kg',
    recorded_date: TIMESTAMP NOT NULL,         -- Ngày ghi nhận
    source: VARCHAR(100),                      -- giacaphe.com, cafef.vn, Admin
    updated_by: VARCHAR(100),                  -- Admin username
    created_at: TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes: VARCHAR(500)
)
```

**Indexes:**
- `(product_type, region_code, recorded_date)` - Query optimization
- `(recorded_date)` - Time-series queries

---

## ⚠️ IMPORTANT NOTES

1. **Không auto-scraping**: Giá được cập nhật thủ công bởi Admin để tránh vấn đề bản quyền
2. **Lịch sử không xóa**: Data lịch sử nên giữ lại để phân tích xu hướng
3. **Product types mở rộng**: Có thể thêm DURIAN, RICE, v.v. mà không cần đổi schema
4. **Region mở rộng**: Có thể thêm tỉnh khác (Bình Phước, Kon Tum, v.v.)

---

## 🎯 NEXT STEPS

1. ✅ ~Tạo table trong DB~ (Done - chạy SQL script)
2. ✅ ~Test API endpoints~ (Cần chạy backend)
3. ⏳ Frontend integration
4. ⏳ Add admin panel UI để cập nhật giá
5. ⏳ Add authentication cho admin endpoints
6. ⏳ Vẽ chart lịch sử giá

---

**Tóm lại:** Hệ thống giờ lưu giá vào **database**, hỗ trợ **nhiều sản phẩm**, **nhiều tỉnh**, và **tracking lịch sử**. Admin có thể cập  nhật giá thủ công qua API. 🚀
