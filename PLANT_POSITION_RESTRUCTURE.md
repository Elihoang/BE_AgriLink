# PlantPosition Restructuring - Farm-Based Design

## 📋 Overview
Đã chuyển từ thiết kế **PlantPosition → Season** sang **PlantPosition → Farm (+ Optional Season)**.

## 🔄 Changes Made

### 1. **Model Changes** (`PlantPosition.cs`)
```csharp
// BEFORE
public Guid SeasonId { get; set; } // Required

// AFTER  
public Guid FarmId { get; set; }   // Required - Vị trí thuộc rẫy nào
public Guid? SeasonId { get; set; } // Optional - Vụ mùa đang sử dụng vị trí này
```

**Navigation Properties:**
- Added: `public virtual Farm Farm { get; set; }`
- Changed: `CropSeason` → nullable

---

### 2. **DTOs Changes**

#### `CreatePlantPositionDto.cs`
```csharp
[Required]
public Guid FarmId { get; set; }     // NEW - Required

public Guid? SeasonId { get; set; }  // CHANGED - Now optional
```

#### `PlantPositionDto.cs`
```csharp
public Guid FarmId { get; set; }
public string FarmName { get; set; }      // NEW
public Guid? SeasonId { get; set; }       // CHANGED - nullable
public string? SeasonName { get; set; }   // NEW
```

---

### 3. **Repository Changes** (`IPlantPositionRepository` + Implementation)

**New Methods (Farm-based):**
```csharp
Task<IEnumerable<PlantPosition>> GetByFarmIdAsync(Guid farmId);
Task<PlantPosition?> GetByPositionAsync(Guid farmId, int row, int col);
Task<bool> PositionExistsAsync(Guid farmId, int row, int col);
```

**Existing Methods (Season-based):**
- `GetBySeasonIdAsync()` - Kept for querying by season
- `GetByProductIdAsync()`, `GetPlantSummaryAsync()` - Unchanged

---

### 4. **Service Changes** (`PlantPositionService.cs`)

**New Method:**
```csharp
public async Task<IEnumerable<PlantPositionDto>> GetByFarmAsync(Guid farmId)
```
- Lấy tất cả vị trí cây trong 1 rẫy
- Hỗ trợ Redis caching với key: `plant_positions:farm:{farmId}`

**Updated Methods:**

#### `AddPlantAsync()`
- Validate `FarmId` (required)
- Validate `SeasonId` nếu có (optional)
- Check position exists by `FarmId` (not SeasonId)

#### `UpdatePlantAsync()`
- Cache invalidation chỉ khi `SeasonId.HasValue`

#### `RemovePlantAsync()`
- Cache invalidation chỉ khi `SeasonId.HasValue`

#### `BulkCreatePlantsAsync(Guid farmId, ...)`
- Changed parameter from `seasonId` → `farmId`
- Validate farm instead of season
- Check duplicates by `FarmId`
- Invalidate both farm và season caches

#### `MapToDto()`
```csharp
FarmId = position.FarmId,
FarmName = position.Farm?.Name ?? string.Empty,
SeasonId = position.SeasonId,              // nullable
SeasonName = position.CropSeason?.Name,    // nullable
```

---

## 🎯 Benefits

### ✅ **Flexible Design**
- Rẫy có sơ đồ cố định (farm-based)
- Vụ mùa optional cho tracking chi phí
- Hỗ trợ cả cây lâu năm (cà phê, tiêu) và ngắn ngày

### ✅ **Data Consistency**
- Vị trí cây không bị duplicate giữa các vụ
- Một lần setup sơ đồ rẫy, dùng mãi

### ✅ **Backward Compatible**
- Vẫn giữ các method `GetBySeasonAsync()`, `GetPlantSummaryAsync()`
- Season-based queries vẫn hoạt động bình thường

---

## 📊 Database Migration Needed

```sql
ALTER TABLE plant_positions
ADD COLUMN farm_id UUID NOT NULL,
ALTER COLUMN season_id DROP NOT NULL;

-- Add foreign key
ALTER TABLE plant_positions
ADD CONSTRAINT fk_plant_positions_farm
FOREIGN KEY (farm_id) REFERENCES farms(id);

-- Update unique constraint (if any)
-- FROM: UNIQUE(season_id, row_number, column_number)
-- TO:   UNIQUE(farm_id, row_number, column_number)
```

---

## 🔧 Next Steps

1. ✅ Update Controller endpoints to support `GetByFarm()` 
2. ⏳ Generate and run database migration
3. ⏳ Update API documentation
4. ⏳ Test all endpoints with Postman
5. ⏳ Update UI to show farm-based plant positions

---

## 💡 Usage Examples

### Create plant without season assignment
```csharp
var dto = new CreatePlantPositionDto {
    FarmId = farmId,
    SeasonId = null,  // Chưa gắn vào vụ nào
    RowNumber = 1,
    ColumnNumber = 5,
    ProductId = coffeeProductId
};
```

### Assign plant to a season later
```csharp
var updateDto = new UpdatePlantPositionDto {
    ProductId = position.ProductId,
    HealthStatus = PlantHealthStatus.Healthy,
    // SeasonId will be set via separate endpoint or business logic
};
```

### Get all plants in a farm (regardless of season)
```csharp
var farmPlants = await plantPositionService.GetByFarmAsync(farmId);
// Returns all plants, some may have SeasonId, some may not
```

---

**Updated:** 2025-12-31  
**Status:** ✅ Ready for database migration
