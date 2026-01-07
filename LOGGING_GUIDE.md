# 📝 Serilog Logging Configuration

## ✅ Đã cài đặt

Đã cấu hình Serilog với **FULL LOGGING** bao gồm:
- ✅ Debug, Info, Warning, **Error**, **Fatal**
- ✅ Exception stack traces đầy đủ
- ✅ 2 files riêng biệt: ALL logs + ERRORS only

---

## 📂 Cấu trúc Logs

```
AgriLink_DH.Api/
└── Logs/
    ├── all/
    │   └── agrilink-20251231.log          ← TẤT CẢ logs (Debug, Info, Warning, Error, Fatal)
    └── errors/
        └── error-20251231.log              ← CHỈ errors (Error, Fatal) - DỄ TÌM LỖI!
```

---

## 🎯 Log Levels

| Level | Khi nào dùng | Có trong file nào? |
|-------|--------------|-------------------|
| **Debug** | Chi tiết code, development | `all/` |
| **Information** | Thông tin quan trọng, API calls | `all/` |
| **Warning** | Cảnh báo, không phải lỗi | `all/` |
| **Error** ⚠️ | Lỗi có thể recover được | `all/` + `errors/` |
| **Fatal** 🔥 | Lỗi nghiêm trọng, app crash | `all/` + `errors/` |

---

## 📊 Ví dụ Logs

### **Console Output** (Real-time, có màu)

```
[21:40:15 INF] AgriLink_DH.Core.Services.WeatherService
    Calling OpenWeather API: https://api.openweathermap.org/data/2.5/weather?lat=10.8231&lon=106.6297&appid=***

[21:40:16 ERR] AgriLink_DH.Core.Services.WeatherService
    OpenWeather API error: Unauthorized - {"cod":401, "message": "Invalid API key..."}
```

### **File: `Logs/all/agrilink-20251231.log`**

```
2025-12-31 21:40:15.123 [INF] AgriLink_DH.Core.Services.WeatherService
    Calling OpenWeather API: https://api.openweathermap.org/data/2.5/weather?lat=10.8231&lon=106.6297&appid=***

2025-12-31 21:40:16.456 [ERR] AgriLink_DH.Core.Services.WeatherService
    OpenWeather API error: Unauthorized - {"cod":401, "message": "Invalid API key. Please see https://openweathermap.org/faq#error401 for more info."}
---
```

### **File: `Logs/errors/error-20251231.log`** (CHỈ ERRORS!)

```
2025-12-31 21:40:16.456 [ERR] AgriLink_DH.Core.Services.WeatherService
    OpenWeather API error: Unauthorized - {"cod":401, "message": "Invalid API key. Please see https://openweathermap.org/faq#error401 for more info."}
---

2025-12-31 22:15:30.789 [ERR] AgriLink_DH.Core.Services.FarmService
    Database connection failed
System.InvalidOperationException: Connection timeout
   at Npgsql.NpgsqlConnection.Open() in /home/...
---
```

---

## 🔍 Cách sử dụng trong Code

### **Inject Logger vào Service/Controller:**

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task DoSomething()
    {
        _logger.LogDebug("🐛 Debug info: Processing started");
        _logger.LogInformation("ℹ️ Processing item {ItemId}", itemId);
        _logger.LogWarning("⚠️ Item not found, using default");
        _logger.LogError("❌ Failed to save: {Error}", ex.Message);
        _logger.LogCritical(ex, "🔥 Critical: Database down!");
    }
}
```

---

## 🎨 Features

✅ **Console logs** - Màu sắc, real-time, dễ đọc  
✅ **File rotation** - Tự động tạo file mới mỗi ngày  
✅ **Separate error logs** - Dễ tìm lỗi, không lẫn với info  
✅ **Exception tracking** - Stack trace đầy đủ  
✅ **Structured logging** - ThreadId, MachineName, Context  
✅ **Performance** - Async, không blocking  

---

## 📁 Gitignore

File `.gitignore` đã có:

```gitignore
# Logs
Logs/
*.log
```

→ Logs **KHÔNG** được push lên GitHub

---

## 🚀 Test Logging

### **1. Chạy server:**

```bash
dotnet run --project AgriLink_DH.Api
```

**Console sẽ hiện:**
```
[21:40:00 INF]
    🚀 Starting AgriLink API...
[21:40:00 INF]
    📝 Logging level: Debug (All logs including errors)
[21:40:01 INF]
    📂 All logs: Logs/all/agrilink-YYYYMMDD.log
[21:40:01 INF]
    ❌ Errors only: Logs/errors/error-YYYYMMDD.log
```

### **2. Gọi API để test:**

```bash
GET http://localhost:5000/api/weather/coordinates?lat=10.8231&lon=106.6297
```

### **3. Kiểm tra logs:**

**Console:** Sẽ thấy ngay lập tức  
**File all:** `Logs/all/agrilink-20251231.log`  
**File errors:** `Logs/errors/error-20251231.log` (nếu có lỗi)

---

## 🔧 Troubleshooting

### **Không thấy logs trong console?**
- Đảm bảo đang chạy `dotnet run` trong terminal
- Không dùng `dotnet build` (build không chạy app)

### **File log không được tạo?**
- Kiểm tra quyền write vào folder `Logs/`
- Chạy lại server, folder sẽ tự động tạo

### **Quá nhiều logs?**
- Đổi `MinimumLevel.Debug()` → `MinimumLevel.Information()`
- Giảm logs từ EF Core: `.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)`

---

## 📚 Best Practices

1. **Development:** Dùng `Debug` level để thấy chi tiết
2. **Production:** Dùng `Information` level để giảm logs
3. **Errors:** Luôn log exceptions với `LogError(ex, "message")`
4. **Sensitive data:** KHÔNG log password, API keys, tokens
5. **Structured logging:** Dùng `{PropertyName}` thay vì string interpolation

**Ví dụ ĐÚNG:**
```csharp
_logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress);
```

**Ví dụ SAI:**
```csharp
_logger.LogInformation($"User {userId} logged in from {ipAddress}"); // ❌ Không structured
```

---

**Created by:** Antigravity AI Assistant  
**Date:** 31/12/2025  
**Version:** 2.0 (Enhanced with error-only logs)
