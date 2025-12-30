# 🔍 XEM REDIS QUA API (NHANH NHẤT)

## 🎯 Dùng Swagger hoặc Postman

### 1️⃣ **Xem TẤT CẢ keys**
```http
GET https://localhost:7032/api/redisdebug/keys

# Hoặc filter
GET https://localhost:7032/api/redisdebug/keys?pattern=plant_positions:*
```

**Response:**
```json
{
  "data": [
    "plant_positions:season:abc-123",
    "refresh_token:user-456",
    "refresh_token:user-789"
  ],
  "message": "Tìm thấy 3 keys"
}
```

---

### 2️⃣ **Xem VALUE của 1 key**
```http
GET https://localhost:7032/api/redisdebug/get/plant_positions:season:abc-123
```

**Response:**
```json
{
  "data": [
    {
      "id": "...",
      "rowNumber": 1,
      "columnNumber": 1,
      "productName": "Cà phê Arabica",
      "healthStatus": "Healthy"
    }
  ]
}
```

---

### 3️⃣ **Xem TTL (thời gian còn lại)**
```http
GET https://localhost:7032/api/redisdebug/ttl/plant_positions:season:abc-123
```

**Response:**
```json
{
  "data": {
    "key": "plant_positions:season:abc-123",
    "ttl": "00:45:30",
    "totalSeconds": 2730,
    "expiresAt": "2024-12-30T15:30:00Z"
  }
}
```

---

### 4️⃣ **Xóa 1 key**
```http
DELETE https://localhost:7032/api/redisdebug/delete/plant_positions:season:abc-123
```

---

### 5️⃣ **Xóa TẤT CẢ (CAREFUL!)**
```http
DELETE https://localhost:7032/api/redisdebug/flush-all
```

---

### 6️⃣ **Redis Server Info**
```http
GET https://localhost:7032/api/redisdebug/info
```

**Response:**
```json
{
  "data": {
    "endpoint": "agrilinkredis-...aivencloud.com:18659",
    "isConnected": true,
    "version": "7.2.0"
  }
}
```

---

## 🚀 Test Ngay Workflow

### Test PlantPosition Cache:

```http
### 1. Call API lần đầu (Cache MISS - Slow)
GET https://localhost:7032/api/plantpositions/season/{seasonId}

### 2. Xem key đã được cache chưa
GET https://localhost:7032/api/redisdebug/keys?pattern=plant_positions:*

### 3. Xem data trong cache
GET https://localhost:7032/api/redisdebug/get/plant_positions:season:{seasonId}

### 4. Xem TTL
GET https://localhost:7032/api/redisdebug/ttl/plant_positions:season:{seasonId}

### 5. Call API lần 2 (Cache HIT - Fast ⚡)
GET https://localhost:7032/api/plantpositions/season/{seasonId}

### 6. Xóa cache
DELETE https://localhost:7032/api/redisdebug/delete/plant_positions:season:{seasonId}

### 7. Call API lần 3 (Cache MISS again)
GET https://localhost:7032/api/plantpositions/season/{seasonId}
```

---

## 📊 Patterns Hữu Ích

```http
# Xem tất cả PlantPosition cache
GET /api/redisdebug/keys?pattern=plant_positions:*

# Xem tất cả refresh tokens
GET /api/redisdebug/keys?pattern=refresh_token:*

# Xem tất cả keys
GET /api/redisdebug/keys?pattern=*
```

---

**NGAY BÂY GIỜ:** Vào Swagger → `/api/redisdebug/keys` → Execute! 🚀
