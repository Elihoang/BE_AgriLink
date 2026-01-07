# Weather Service - OpenWeather API Integration

## 📦 Đã tạo

1. **DTOs**
   - `OpenWeatherResponseDto.cs` - Response từ OpenWeather API
   - `WeatherForecastDto.cs` - DTO đơn giản cho app

2. **Service**
   - `WeatherService.cs` - Service gọi OpenWeather API

3. **Controller**
   - `WeatherController.cs` - API endpoints

## 🔑 Cấu hình API Key

### Bước 1: Đăng ký OpenWeather API Key (MIỄN PHÍ)

1. Truy cập: https://openweathermap.org/api
2. Đăng ký tài khoản miễn phí
3. Tạo API Key (Free tier: 60 calls/minute, đủ dùng)

### Bước 2: Thêm vào `appsettings.json` hoặc `appsettings.Development.json`

```json
{
  "OpenWeather": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

## 🧪 Test API

### 1. Test với tọa độ trực tiếp (không cần Farm)

```bash
GET /api/weather/coordinates?lat=10.8231&lon=106.6297
```

**Ví dụ tọa độ:**
- TP.HCM: `lat=10.8231&lon=106.6297`
- Đắk Lắk: `lat=12.6667&lon=108.0500`
- Lâm Đồng (Đà Lạt): `lat=11.9404&lon=108.4583`

### 2. Test với Farm (cần có AddressGps trong DB)

```bash
GET /api/weather/farm/{farmId}
```

**Yêu cầu:** Farm phải có `address_gps` với format: `"12.6667, 108.0500"`

## 📊 Response Example

```json
{
  "success": true,
  "message": "Lấy dự báo thời tiết thành công",
  "data": {
    "locationName": "Ho Chi Minh City",
    "latitude": 10.8231,
    "longitude": 106.6297,
    "forecastTime": "2025-12-31T13:30:00Z",
    "condition": "Rain",
    "description": "mưa nhẹ",
    "temperature": 28.5,
    "feelsLike": 32.1,
    "tempMin": 27.0,
    "tempMax": 30.0,
    "humidity": 85,
    "rainfallMm": 2.5,
    "windSpeed": 3.2,
    "cloudCoverage": 75,
    "willRain": true,
    "advice": "🌧️ Mưa nhỏ - Có thể làm việc nhưng cẩn thận. 🦠 Độ ẩm cao - Chú ý phòng bệnh nấm"
  }
}
```

## 🎯 Tính năng

- ✅ Lấy dự báo thời tiết real-time từ OpenWeather
- ✅ Parse tọa độ từ Farm.AddressGps
- ✅ Dự đoán có mưa không (`willRain`)
- ✅ Tự động đưa ra lời khuyên cho nông dân dựa trên:
  - Lượng mưa
  - Nhiệt độ
  - Độ ẩm
  - Tốc độ gió
- ✅ Hỗ trợ tiếng Việt từ OpenWeather

## 📝 Lời khuyên tự động

Service sẽ tự động phân tích và đưa ra lời khuyên:

- **Mưa lớn (>10mm):** ⚠️ Tránh phun thuốc, hoãn thu hoạch
- **Mưa vừa (5-10mm):** 🌧️ Nên hoãn hoạt động ngoài đồng
- **Mưa nhỏ (<5mm):** 🌦️ Có thể làm việc nhưng cẩn thận
- **Nắng gắt (>35°C):** 🔥 Tưới nước buổi sáng sớm hoặc chiều mát
- **Độ ẩm cao + nhiệt độ cao:** 🦠 Chú ý phòng bệnh nấm
- **Gió mạnh (>10 m/s):** 💨 Không nên phun thuốc

## 🔄 Workflow

1. **Farm có tọa độ** → Lưu vào `address_gps` (format: "lat, lon")
2. **Gọi API** → WeatherService parse tọa độ → OpenWeather API
3. **Nhận kết quả** → Map sang DTO đơn giản → Trả về app
4. **App hiển thị** → Nông dân biết thời tiết hôm nay

## 🚀 Next Steps

Sau khi test xong, có thể:

1. **Lưu lịch sử thời tiết** vào `weather_logs` để phân tích sau
2. **Tự động lưu** mỗi ngày (Background Job)
3. **Cảnh báo** khi có mưa lớn hoặc thời tiết xấu
4. **Phân tích xu hướng** - Năm ngoái mưa nhiều tháng mấy?
