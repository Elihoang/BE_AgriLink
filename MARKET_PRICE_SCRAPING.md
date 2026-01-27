# Market Price Scraping - Hướng dẫn

## 📚 Tổng quan

AgriLink hiện hỗ trợ **4 phương án** lấy giá cà phê:

| Phương án | Nguồn | Độ chính xác | Tốc độ | Hợp pháp | Ghi chú |
|-----------|-------|--------------|--------|----------|---------|
| **1. Alpha Vantage API** | Alpha Vantage | ⭐⭐⭐ (Global) | ⚡⚡⚡ | ✅ | Quy đổi về VN |
| **2. Twelve Data API** | Twelve Data | ⭐⭐⭐ (Futures) | ⚡⚡⚡ | ✅ | Coffee Futures |
| **3. Puppeteer Scraping** | giacaphe.com | ⭐⭐⭐⭐⭐ (VN) | ⚡ | ⚠️ | **MỚI - JS Rendering** |
| **4. Regex Scraping** | chocaphe.vn | ⭐ | ⚡⚡ | ⚠️ | Không hoạt động |

---

## 🆕 Puppeteer Scraping (giacaphe.com)

### Cách hoạt động

1. **Khởi chạy Headless Chrome** (Chromium)
2. **Navigate** tới `https://giacaphe.com/gia-ca-phe-noi-dia/`
3. **Chờ JavaScript render** xong bảng giá
4. **Extract data** từ HTML đã render bằng JavaScript
5. **Parse** và trả về data

### API Endpoint

```http
GET /api/MarketPrice/regional/puppeteer
```

### Response Example

```json
{
  "success": true,
  "message": "Lấy giá từ giacaphe.com thành công",
  "data": {
    "fetchedAt": "2026-01-25T19:30:00",
    "isFromCache": false,
    "commodities": [
      {
        "name": "Cà phê Nhân xô (Việt Nam)",
        "code": "ROBUSTA_VN",
        "currentPrice": 101000,
        "change": 1800,
        "changePercent": 1.81,
        "unit": "VND/kg",
        "source": "giacaphe.com (Puppeteer)"
      },
      {
        "name": "Hồ tiêu (Việt Nam)",
        "code": "PEPPER_VN",
        "currentPrice": 149000,
        "change": 0,
        "unit": "VND/kg"
      },
      {
        "name": "Tỷ giá USD/VND",
        "code": "USD_VND",
        "currentPrice": 26031,
        "change": 0,
        "unit": "VND"
      }
    ],
    "regionalPrices": [
      { "region": "Đắk Lắk", "coffeePrice": 101000, "change": 1800 },
      { "region": "Lâm Đồng", "coffeePrice": 100500, "change": 1700 },
      { "region": "Gia Lai", "coffeePrice": 100800, "change": 1600 },
      { "region": "Đắk Nông", "coffeePrice": 101000, "change": 1700 }
    ]
  }
}
```

---

## ⚙️ Setup & Configuration

### Lần đầu chạy

Khi bạn gọi endpoint `/regional/puppeteer` lần đầu tiên, PuppeteerSharp sẽ tự động:

1. **Download Chromium** (~150MB) vào `~/.local/share/puppeteer/` (Linux/Mac) hoặc `%USERPROFILE%/.local-chromium/` (Windows)
2. **Giải nén** và cấu hình
3. **Khởi chạy** browser lần đầu

⏱️ **Lần đầu có thể mất 30-60 giây** để download Chromium. Các lần sau sẽ nhanh hơn (3-5 giây).

### Kiểm tra

Test endpoint bằng:

```bash
# PowerShell
Invoke-WebRequest -Uri "http://localhost:5000/api/MarketPrice/regional/puppeteer" | ConvertFrom-Json

# Hoặc browser
http://localhost:5000/api/MarketPrice/regional/puppeteer
```

---

## 🚀 Tối ưu hóa

### 1. Cache kết quả (Redis)

Update `appsettings.json` để tăng cache time:

```json
{
  "MarketPrice": {
    "CacheMinutes": 120,  // Cache 2 giờ thay vì 30 phút
    "Provider": "giacaphe" // Chọn provider mặc định
  }
}
```

### 2. Chạy Background Job

Thay vì scrape khi user request, nên:

- Tạo **Background Job** (Hangfire/Quartz)
- Chạy **1-2 lần/ngày**
- Lưu vào **Database** hoặc **Redis**
- API chỉ cần đọc từ cache

### 3. Timeout & Error Handling

Method hiện tại đã có:

- **Connection timeout**: 30 giây
- **Selector wait**: 10 giây
- **Error logging** đầy đủ

---

## 📊 So sánh hiệu suất

| Endpoint | Thời gian trung bình | Bandwidth | Độ chính xác |
|----------|---------------------|-----------|--------------|
| `/regional/puppeteer` | **3-5s** (sau lần đầu) | ~2MB | ⭐⭐⭐⭐⭐ |
| `/regional/conversion` | **0.5-1s** | ~5KB | ⭐⭐⭐ |
| `/regional/manual` | **<0.1s** | 0 | ⭐⭐ |

---

## ⚠️ Lưu ý quan trọng

### 1. Legal & Ethics

Web scraping có thể vi phạm:
- **Terms of Service** của website
- **robots.txt** rules
- **Bản quyền nội dung**

👉 **Chỉ dùng cho mục đích học tập / demo**. Với production app, nên:
- Liên hệ giacaphe.com xin phép
- Hoặc dùng API chính thống (Alpha Vantage, Twelve Data)

### 2. Performance

- **Puppeteer nặng**: ~150MB Chromium + 100-200MB RAM khi chạy
- **Không scale tốt**: Với 1000 concurrent requests sẽ crash server
- **Nên dùng cache** và background jobs

### 3. Maintenance

Website **giacaphe.com có thể thay đổi**:
- HTML structure
- CSS classes (`.price-table`)
- JavaScript logic

👉 Cần **kiểm tra & update** code định kỳ.

---

## 🔧 Troubleshooting

### Lỗi: "Could not find Chrome"

```bash
# Xóa cache Chromium và download lại
rm -rf ~/.local-chromium/  # Linux/Mac
rmdir /s %USERPROFILE%\.local-chromium\  # Windows
```

### Lỗi: Timeout

Tăng timeout trong code:

```csharp
Timeout = 60000  // 60 giây
```

### Lỗi: Failed to launch browser

Check firewall hoặc antivirus blocking Chromium.

---

## 🎯 Khuyến nghị

### Cho Development/Testing
✅ Dùng **Puppeteer** (`/regional/puppeteer`)
- Data thật từ giacaphe.com
- Dễ debug

### Cho Production
✅ Dùng **Alpha Vantage API** (`/regional/conversion`)
- Hợp pháp
- Nhanh, ổn định
- Có support

### Cho POC/Demo nhanh
✅ Dùng **Manual Mock** (`/regional/manual`)
- Instant response
- Zero dependencies

---

## 📞 Support

Có vấn đề? Check logs:

```bash
# Xem logs của MarketPriceService
tail -f AgriLink_DH.Api/Logs/errors/*.log
```

Hoặc enable debug logging:

```json
{
  "Logging": {
    "LogLevel": {
      "AgriLink_DH.Core.Services.MarketPriceService": "Debug"
    }
  }
}
```
