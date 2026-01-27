# 🚀 MARKET PRICE API - QUICK START

## ✅ ĐÃ HOÀN TẤT

- ✅ Database table `market_price_history` đã tạo
- ✅ API endpoints sẵn sàng
- ✅ Service layer hoàn chỉnh

---

## 📌 API ENDPOINTS

### **1. GET - Lấy giá mới nhất**
```http
GET http://localhost:5000/api/MarketPrice
```

### **2. POST - Admin cập nhật 1 giá**
```http
POST http://localhost:5000/api/MarketPrice/admin/update
Content-Type: application/json

{
  "productId": "00000000-0000-0000-0000-000000000001",
  "productName": "Cà phê Robusta",
  "regionCode": "DAK_LAK",  
  "region": "Đắk Lắk",
  "price": 101500,
  "change": 600,
  "source": "giacaphe.com"
}
```

### **3. POST - Admin cập nhật nhiều giá (Batch)**
```http
POST http://localhost:5000/api/MarketPrice/admin/batch-update
Content-Type: application/json

[
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "DAK_LAK",
    "region": "Đắk Lắk",
    "price": 100900,
    "change": 1700
  },
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "LAM_DONG",
    "region": "Lâm Đồng",
    "price": 100500,
    "change": 1700
  },
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "GIA_LAI",
    "region": "Gia Lai",
    "price": 100800,
    "change": 1600
  },
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "DAK_NONG",
    "region": "Đắk Nông",
    "price": 101000,
    "change": 1700
  }
]
```

### **4. GET - Lịch sử giá**
```http
GET http://localhost:5000/api/MarketPrice/history?productId=00000000-0000-0000-0000-000000000001&regionCode=DAK_LAK&limit=30
```

---

## 🧪 TEST NHANH (PowerShell)

```powershell
# 1. Test GET
Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice"

# 2. Test POST - Nhập giá hôm nay (copy paste vào terminal)
$data = @{
    productId = "00000000-0000-0000-0000-000000000001"
    productName = "Cà phê Robusta"
    regionCode = "DAK_LAK"
    region = "Đắk Lắk"
    price = 101500
    change = 600
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice/admin/update" `
    -Method POST -Body $data -ContentType "application/json"

# 3. Test Batch - Nhập 4 tỉnh cùng lúc
$batch = @(
    @{ productId="00000000-0000-0000-0000-000000000001"; productName="Cà phê Robusta"; regionCode="DAK_LAK"; region="Đắk Lắk"; price=100900; change=1700 },
    @{ productId="00000000-0000-0000-0000-000000000001"; productName="Cà phê Robusta"; regionCode="LAM_DONG"; region="Lâm Đồng"; price=100500; change=1700 },
    @{ productId="00000000-0000-0000-0000-000000000001"; productName="Cà phê Robusta"; regionCode="GIA_LAI"; region="Gia Lai"; price=100800; change=1600 },
    @{ productId="00000000-0000-0000-0000-000000000001"; productName="Cà phê Robusta"; regionCode="DAK_NONG"; region="Đắk Nông"; price=101000; change=1700 }
) | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice/admin/batch-update" `
    -Method POST -Body $batch -ContentType "application/json"
```

---

## 📂 FILES QUAN TRỌNG

### **Controller**
`AgriLink_DH.Api/Controllers/MarketPriceController.cs`

### **Service**
`AgriLink_DH.Core/Services/MarketPriceDbService.cs`

### **Model**
`AgriLink_DH.Domain/Models/MarketPriceHistory.cs`

### **DTO**
`AgriLink_DH.Share/DTOs/MarketPrice/UpdateMarketPriceRequest.cs`

---

## 💡 SỬ DỤNG

### **Workflow hàng ngày:**

1. Admin vào https://giacaphe.com/gia-ca-phe-noi-dia/
2. Xem giá 4 tỉnh
3. POST batch update vào API
4. Frontend tự động hiển thị giá mới

### **Example data (Copy paste):**

```json
[
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "DAK_LAK",
    "region": "Đắk Lắk",
    "price": 100900,
    "change": 1700,
    "source": "giacaphe.com"
  },
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "LAM_DONG",
    "region": "Lâm Đồng",
    "price": 100500,
    "change": 1700,
    "source": "giacaphe.com"
  },
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "GIA_LAI",
    "region": "Gia Lai",
    "price": 100800,
    "change": 1600,
    "source": "giacaphe.com"
  },
  {
    "productId": "00000000-0000-0000-0000-000000000001",
    "productName": "Cà phê Robusta",
    "regionCode": "DAK_NONG",
    "region": "Đắk Nông",
    "price": 101000,
    "change": 1700,
    "source": "giacaphe.com"
  },
  {
    "productId": "00000000-0000-0000-0000-000000000002",
    "productName": "Hồ tiêu",
    "regionCode": "DAK_LAK",
    "region": "Đắk Lắk",
    "price": 149000,
    "change": 0,
    "source": "giacaphe.com"
  }
]
```

---

## ⚡ START BACKEND

```bash
cd AgriLink_DH
dotnet run --project AgriLink_DH.Api
```

Server chạy ở: **http://localhost:5000**

---

## 🎯 KẾT LUẬN

Hệ thống đã sẵn sàng! Bạn có thể:

1. ✅ Lấy giá qua GET endpoint
2. ✅ Admin nhập giá thủ công qua POST
3. ✅ Xem lịch sử giá theo ngày/tỉnh
4. ✅ Mở rộng cho thêm sản phẩm (sầu riêng, lúa, v.v.)

**Không cần scraping, không vi phạm bản quyền!** 🎉
