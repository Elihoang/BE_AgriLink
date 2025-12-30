# 🔍 XEM DỮ LIỆU REDIS

## 📍 **Redis Ở Đâu?**

Redis của bạn là **CLOUD** (Aiven), không lưu local:

```
Host: agrilinkredis-agrilink81.h.aivencloud.com:18659
Password: AVNS_8sdQpViKkdc81jwbgrz
SSL: true
```

Dữ liệu lưu trên server Aiven ở cloud, không phải máy local.

---

## 🔧 **Cách 1: Dùng Redis CLI (Recommended)**

### Install Redis CLI:
```bash
# Windows (dùng Chocolatey)
choco install redis-64

# Hoặc download từ: https://github.com/microsoftarchive/redis/releases
```

### Connect to Redis:
```bash
redis-cli -h agrilinkredis-agrilink81.h.aivencloud.com -p 18659 --tls -a AVNS_8sdQpViKkdc81jwbgrz
```

### Xem keys:
```bash
# List all keys
KEYS *

# List PlantPosition cache keys
KEYS plant_positions:*

# List refresh token keys
KEYS refresh_token:*

# Get value của 1 key
GET plant_positions:season:abc-123-xyz

# Check TTL (thời gian còn lại)
TTL plant_positions:season:abc-123-xyz

# Delete key
DEL plant_positions:season:abc-123-xyz
```

---

## 🖥️ **Cách 2: Dùng RedisInsight (GUI App)**

### Download:
https://redis.io/insight/

### Connect:
```
Host: agrilinkredis-agrilink81.h.aivencloud.com
Port: 18659
Password: AVNS_8sdQpViKkdc81jwbgrz
✅ Use TLS/SSL
```

Giao diện đẹp, xem key dễ dàng hơn CLI.

---

## 🧪 **Cách 3: Test Trong Code**

Tạo test endpoint để xem cache:

```csharp
[HttpGet("debug/redis-keys")]
public async Task<ActionResult<ApiResponse<List<string>>>> GetRedisKeys()
{
    var server = _redis.GetServer(_redis.GetEndPoints().First());
    var keys = server.Keys(pattern: "*").Select(k => k.ToString()).ToList();
    return Ok(ApiResponse<List<string>>.SuccessResponse(keys));
}

[HttpGet("debug/redis-get/{key}")]
public async Task<ActionResult<ApiResponse<string>>> GetRedisValue(string key)
{
    var value = await _database.StringGetAsync(key);
    return Ok(ApiResponse<string>.SuccessResponse(value.ToString()));
}
```

---

## 📊 **Cache Keys Trong Hệ Thống**

### PlantPosition Cache:
```
Key: plant_positions:season:{seasonId}
Value: JSON array of PlantPositionDto
TTL: 3600 seconds (1 hour)

Example:
plant_positions:season:a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

### Refresh Token Cache:
```
Key: refresh_token:{userId}
Value: JWT refresh token string
TTL: 7 days
```

---

## 🔍 **Xem Cache Hit/Miss**

Bật logging trong PlantPositionService:

```csharp
public async Task<IEnumerable<PlantPositionDto>> GetBySeasonAsync(Guid seasonId)
{
    var cacheKey = $"{REDIS_KEY_PREFIX}{seasonId}";

    var cached = await _redisService.GetAsync<List<PlantPositionDto>>(cacheKey);
    if (cached != null)
    {
        Console.WriteLine($"✅ CACHE HIT: {cacheKey}");
        return cached;
    }

    Console.WriteLine($"❌ CACHE MISS: {cacheKey} - Querying DB...");
    // ... rest of code
}
```

---

## 🎯 **Quick Test**

```bash
# 1. Connect
redis-cli -h agrilinkredis-agrilink81.h.aivencloud.com -p 18659 --tls -a AVNS_8sdQpViKkdc81jwbgrz

# 2. Test set/get
SET test:key "Hello Redis"
GET test:key

# 3. See all keys
KEYS *

# 4. Clear all (CAREFUL!)
FLUSHDB  # ⚠️ Xóa hết cache
```

---

**Tóm lại:** Redis lưu trên **cloud Aiven**, dùng **RedisInsight** hoặc **redis-cli** để xem data.
