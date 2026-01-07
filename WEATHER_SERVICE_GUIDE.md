# 🌤️ Weather Service - Hướng Dẫn Hoàn Chỉnh

## ✅ Đã hoàn thành

### 1. **Database Schema**
- ✅ Thêm `latitude` (numeric 10,7) vào bảng `farms`
- ✅ Thêm `longitude` (numeric 10,7) vào bảng `farms`
- ✅ Migration đã được apply thành công

### 2. **Models & DTOs**
- ✅ `Farm.cs` - Thêm Latitude, Longitude
- ✅ `FarmDto.cs` - Thêm Latitude, Longitude
- ✅ `CreateFarmDto.cs` - Thêm Latitude, Longitude
- ✅ `UpdateFarmDto.cs` - Thêm Latitude, Longitude
- ✅ `OpenWeatherResponseDto.cs` - Map response từ OpenWeather API
- ✅ `WeatherForecastDto.cs` - DTO đơn giản cho app

### 3. **Services**
- ✅ `WeatherService.cs` - Gọi OpenWeather API
  - ✅ Ưu tiên dùng Latitude/Longitude (map selection)
  - ✅ Fallback parse AddressGps (backward compatibility)
  - ✅ Tự động đưa ra lời khuyên cho nông dân

### 4. **Controllers**
- ✅ `WeatherController.cs`
  - ✅ `GET /api/weather/farm/{farmId}` - Dự báo theo farm
  - ✅ `GET /api/weather/coordinates?lat={lat}&lon={lon}` - Test trực tiếp

---

## 🔑 Bước 1: Cấu hình OpenWeather API Key

### 1.1. Đăng ký API Key (MIỄN PHÍ)

1. Truy cập: https://openweathermap.org/api
2. Đăng ký tài khoản
3. Vào **API keys** tab
4. Copy API key của bạn

**Free tier:**
- 60 calls/phút
- 1,000,000 calls/tháng
- Hoàn toàn đủ dùng cho ứng dụng!

### 1.2. Thêm vào `appsettings.json`

Bạn cần file này bị gitignore. Hỏi admin hoặc bạn tạo `appsettings.Development.json`:

```json
{
  "OpenWeather": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

**Lưu ý:** Thay `YOUR_API_KEY_HERE` bằng API key thực!

---

## 📱 Bước 2: Workflow App - Người dùng chọn vị trí

### 2.1. **Frontend Flow (Mobile/Web)**

1. **Tạo Farm mới:**
   ```
   Màn hình tạo Farm
   ├── Nhập tên: "Rẫy Đắk Mil"
   ├── Nhập diện tích: 2.5 (hecta)
   └── Chọn vị trí trên bản đồ 🗺️
       ├── Hiển thị Google Maps / Mapbox
       ├── Người dùng chọn pin 📍
       └── Lưu Latitude & Longitude
   ```

2. **API Call:**
   ```http
   POST /api/farms
   {
     "name": "Rẫy Đắk Mil",
     "areaSize": 2.5,
     "latitude": 12.6667,
     "longitude": 108.0500
   }
   ```

3. **Xem thời tiết:**
   ```http
   GET /api/weather/farm/{farmId}
   ```

### 2.2. **Backend Logic (WeatherService)**

```
Priority 1: Dùng Latitude & Longitude (từ map)
           ├── Nếu có → Gọi OpenWeather API
           └── Nếu không có → Check AddressGps

Priority 2: Parse AddressGps (backward compatibility)
           ├── Parse "12.6667, 108.0500"
           └── Gọi OpenWeather API

Kết quả: Trả về WeatherForecastDto với lời khuyên
```

---

## 🧪 Bước 3: Test API

### Test 1: Test với tọa độ trực tiếp (KHÔNG cần DB)

```bash
curl "http://localhost:5000/api/weather/coordinates?lat=10.8231&lon=106.6297"
```

**Tọa độ mẫu:**
| Địa điểm | Latitude | Longitude |
|----------|----------|-----------|
| TP.HCM | 10.8231 | 106.6297 |
| Đắk Lắk | 12.6667 | 108.0500 |
| Lâm Đồng | 11.9404 | 108.4583 |
| Hà Nội | 21.0285 | 105.8542 |

### Test 2: Test với Farm (CẦN có farm trong DB)

**Bước 1: Tạo farm với tọa độ**
```http
POST /api/farms
{
  "name": "Rẫy Test",
  "areaSize": 1.5,
  "latitude": 12.6667,
  "longitude": 108.0500
}
```

**Bước 2: Lấy weather**
```http
GET /api/weather/farm/{farmId}
```

---

## 📊 Response Mẫu

```json
{
  "success": true,
  "message": "Lấy dự báo thời tiết thành công",
  "data": {
    "locationName": "Buon Ma Thuot",
    "latitude": 12.6667,
    "longitude": 108.05,
    "forecastTime": "2025-12-31T13:35:00Z",
    "condition": "Clouds",
    "description": "mây đen u ám",
    "temperature": 25.3,
    "feelsLike": 25.8,
    "tempMin": 24.0,
    "tempMax": 27.0,
    "humidity": 78,
    "rainfallMm": null,
    "windSpeed": 2.5,
    "cloudCoverage": 85,
    "willRain": false,
    "advice": "✅ Thời tiết bình thường, phù hợp làm việc"
  }
}
```

### Giải thích các field quan trọng:

- **`willRain`**: `true` nếu có mưa, `false` nếu không
- **`rainfallMm`**: Lượng mưa (mm) - `null` nếu không mưa
- **`advice`**: Lời khuyên tự động cho nông dân

---

## 🎯 Lời Khuyên Tự Động

Service sẽ tự động phân tích thời tiết và đưa lời khuyên:

| Điều kiện | Lời khuyên |
|-----------|-----------|
| **Mưa lớn** (>10mm) | ⚠️ Mưa lớn - Tránh phun thuốc, hoãn thu hoạch |
| **Mưa vừa** (5-10mm) | 🌧️ Mưa vừa - Nên hoãn hoạt động ngoài đồng |
| **Mưa nhỏ** (<5mm) | 🌦️ Mưa nhỏ - Có thể làm việc nhưng cẩn thận |
| **Nắng gắt** (>35°C) | 🔥 Nắng gắt - Tưới nước buổi sáng sớm hoặc chiều mát |
| **Độ ẩm cao** (>80%) + Nhiệt (>28°C) | 🦠 Độ ẩm cao - Chú ý phòng bệnh nấm |
| **Gió mạnh** (>10 m/s) | 💨 Gió mạnh - Không nên phun thuốc |

---

## 🚀 Next Steps - Tính năng mở rộng

### 1. **Lưu lịch sử thời tiết** (Đề xuất cao!)
- Mỗi ngày tự động gọi API → Lưu vào `weather_logs`
- Phân tích sau: "Năm ngoái tháng 4 mưa nhiều → Cà ra hoa kém"

### 2. **Background Job** (Cron Job)
- Mỗi sáng 6h tự động lấy thời tiết
- Push notification: "Hôm nay có mưa, hoãn phun thuốc"

### 3. **7-day Forecast**
- Dùng OpenWeather API `/forecast` (16-day forecast)
- Hiển thị dự báo 7 ngày tới

### 4. **Weather Alerts**
- Cảnh báo mưa bão
- Thông báo nắng hạn kéo dài

---

## 🗺️ Frontend Integration - Gợi ý

### Google Maps Example (React Native)

```javascript
import MapView, { Marker } from 'react-native-maps';

function FarmLocationSelect({ onLocationSelect }) {
  const [selectedLocation, setSelectedLocation] = useState(null);

  const handleMapPress = (event) => {
    const { latitude, longitude } = event.nativeEvent.coordinate;
    setSelectedLocation({ latitude, longitude });
    onLocationSelect(latitude, longitude);
  };

  return (
    <MapView
      initialRegion={{
        latitude: 12.6667, // Đắk Lắk
        longitude: 108.0500,
        latitudeDelta: 0.5,
        longitudeDelta: 0.5,
      }}
      onPress={handleMapPress}
    >
      {selectedLocation && (
        <Marker coordinate={selectedLocation} />
      )}
    </MapView>
  );
}
```

### Call API sau khi chọn:

```javascript
const createFarm = async (farmData) => {
  const response = await fetch('/api/farms', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: farmData.name,
      areaSize: farmData.areaSize,
      latitude: farmData.latitude,  // Từ MapView
      longitude: farmData.longitude // Từ MapView
    })
  });
  
  return response.json();
};
```

---

## 🐛 Troubleshooting

### Lỗi: "OpenWeather API Key not configured"
**Giải pháp:** Thêm API key vào `appsettings.json`:
```json
{
  "OpenWeather": {
    "ApiKey": "your_key_here"
  }
}
```

### Lỗi: "Farm has no GPS coordinates"
**Giải pháp:** Farm cần có:
- Hoặc `latitude` & `longitude` (ưu tiên)
- Hoặc `address_gps` với format: "12.6667, 108.0500"

### Lỗi: "401 Unauthorized" từ OpenWeather
**Giải pháp:** 
- Kiểm tra API key có đúng không
- API key mới có thể mất vài phút để activate

---

## 📝 File cấu trúc

```
AgriLink_DH/
├── AgriLink_DH.Domain/
│   └── Models/
│       └── Farm.cs (✅ Đã có Latitude, Longitude)
├── AgriLink_DH.Share/
│   └── DTOs/
│       ├── Farm/
│       │   ├── FarmDto.cs (✅ Đã có Latitude, Longitude)
│       │   ├── CreateFarmDto.cs (✅ Đã có Latitude, Longitude)
│       │   └── UpdateFarmDto.cs (✅ Đã có Latitude, Longitude)
│       └── Weather/
│           ├── OpenWeatherResponseDto.cs (✅ Mới tạo)
│           └── WeatherForecastDto.cs (✅ Mới tạo)
├── AgriLink_DH.Core/
│   └── Services/
│       └── WeatherService.cs (✅ Mới tạo)
└── AgriLink_DH.Api/
    ├── Controllers/
    │   └── WeatherController.cs (✅ Mới tạo)
    └── Migrations/
        └── 20251231133532_AddLatitudeLongitudeToFarm.cs (✅ Đã apply)
```

---

## ✅ Checklist

- [x] Database migration thành công
- [x] Farm model có Latitude/Longitude
- [x] DTOs đã updated
- [x] WeatherService đã tạo
- [x] WeatherController đã tạo
- [x] Service đã register trong DI
- [x] Build thành công
- [ ] **Thêm OpenWeather API Key** ← BẠN CẦN LÀM
- [ ] **Test API** ← BẠN CẦN LÀM
- [ ] **Integrate Frontend (Map selection)** ← TIẾP THEO

---

**Tạo bởi:** Antigravity AI Assistant  
**Ngày:** 31/12/2025  
**Version:** 1.0
