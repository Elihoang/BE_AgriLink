# Plant Position Module - Redis Caching Strategy

## 📊 Module Created

PlantPosition - Quản lý vị trí từng cây trong rẫy (grid layout như ghế rạp)

## 🎯 Key Features

1. **Grid Layout**: Mỗi cây có vị trí (row, col)
2. **Product FK**: Loại cây lấy từ bảng Products (Cà phê, Sầu riêng...)
3. **Health Tracking**: Enum PlantHealthStatus (Healthy, Sick, Dead, Removed, NewlyPlanted)
4. **Redis Caching**: Cache toàn bộ grid 1 season để query nhanh

## ⚡ Redis Caching Strategy

### Cache Key Pattern
```
plant_positions:season:{seasonId}
```

### TTL
- **1 hour** (vì layout ít thay đổi)

### Cache Flow
1. **GET all positions** → Check cache first
2. **Cache MISS** → Query DB + Store in Redis
3. **CREATE/UPDATE/DELETE** → Invalidate cache

### Why Redis?
- 1 rẫy có thể 50-100 cây
- Layout query rất thường xuyên (hiển thị sơ đồ)
- Dữ liệu ít thay đổi

## 📋 API Endpoints

```http
GET    /api/plantpositions/season/{seasonId}           # Lấy tất cả (CÓ CACHE)
GET    /api/plantpositions/season/{seasonId}/summary   # Tổng quan (coffee: 30, durian: 20)
POST   /api/plantpositions                             # Thêm 1 cây
POST   /api/plantpositions/bulk                        # Thêm nhiều cây (bulk)
PUT    /api/plantpositions/{id}                        # Cập nhật
DELETE /api/plantpositions/{id}                        # Xóa
```

## 🔧 Next Steps

1. **Register DI** in ServiceCollectionExtensions
2. **Add DbSet** to ApplicationDbContext
3. **Create Migration** to add `plant_positions` table
4. **Test Redis** caching performance

## 📦 Database Schema

```sql
CREATE TABLE plant_positions (
    id UUID PRIMARY KEY,
    season_id UUID NOT NULL REFERENCES crop_seasons(id),
    row_number INT NOT NULL,
    column_number INT NOT NULL,
    product_id UUID NOT NULL REFERENCES products(id),
    plant_date TIMESTAMP,
    health_status INT NOT NULL DEFAULT 0, -- Enum: 0=Healthy, 1=Sick, 2=Dead...
    estimated_yield DECIMAL(10,2),
    note TEXT,
    UNIQUE(season_id, row_number, column_number) -- Mỗi vị trí chỉ 1 cây
);
```

---
**Created**: 2025-12-30
**Redis Config**: From appsettings.json (cloud Redis)
