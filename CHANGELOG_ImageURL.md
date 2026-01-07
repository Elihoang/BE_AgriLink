# Thêm Image URL cho Models

## Tóm tắt
Đã thêm trường `ImageUrl` cho 4 models chính: **Product**, **Farm**, **Worker**, và **User** để hỗ trợ lưu trữ và hiển thị hình ảnh.

## Các thay đổi đã thực hiện

### 1. Domain Models (c:\HocTap\CaNhan\AgriLink_DH\AgriLink_DH.Domain\Models)
Đã thêm thuộc tính `ImageUrl` vào các models:

✅ **Product.cs**
```csharp
[Column("image_url")]
[MaxLength(500)]
public string? ImageUrl { get; set; } // URL hình ảnh sản phẩm
```

✅ **Farm.cs**
```csharp
[Column("image_url")]
[MaxLength(500)]
public string? ImageUrl { get; set; } // URL hình ảnh trang trại
```

✅ **Worker.cs**
```csharp
[Column("image_url")]
[MaxLength(500)]
public string? ImageUrl { get; set; } // URL hình ảnh nhân công
```

✅ **User.cs**
```csharp
[Column("image_url")]
[MaxLength(500)]
public string? ImageUrl { get; set; } // URL hình ảnh người dùng/avatar
```

### 2. Database Migration
Đã tạo và áp dụng migration: **20260102051403_AddImageUrlToModels**

Migration này thêm cột `image_url` (VARCHAR(500), nullable) vào 4 bảng:
- `products`
- `farms`
- `workers`
- `users`

### 3. DTOs (c:\HocTap\CaNhan\AgriLink_DH\AgriLink_DH.Share\DTOs)
Đã cập nhật tất cả DTOs liên quan:

#### Product DTOs
- ✅ `ProductDto.cs` - Thêm `ImageUrl`
- ✅ `CreateProductDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`
- ✅ `UpdateProductDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`

#### Farm DTOs
- ✅ `FarmDto.cs` - Thêm `ImageUrl`
- ✅ `CreateFarmDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`
- ✅ `UpdateFarmDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`

#### Worker DTOs
- ✅ `WorkerDto.cs` - Thêm `ImageUrl`
- ✅ `CreateWorkerDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`
- ✅ `UpdateWorkerDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`

#### User DTOs
- ✅ `UserDto.cs` - Thêm `ImageUrl`
- ✅ `CreateUserDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`
- ✅ `UpdateUserDto.cs` - Thêm `ImageUrl` với validation `[MaxLength(500)]`

### 4. Services (c:\HocTap\CaNhan\AgriLink_DH\AgriLink_DH.Core\Services)
Đã cập nhật các services để xử lý và map trường `ImageUrl`:

#### ProductService.cs
- ✅ `CreateProductAsync` - Gán `ImageUrl` khi tạo sản phẩm mới
- ✅ `UpdateProductAsync` - Cập nhật `ImageUrl` khi update sản phẩm
- ✅ `MapToDto` - Map `ImageUrl` từ Entity sang DTO

#### FarmService.cs
- ✅ `CreateFarmAsync` - Gán `ImageUrl` khi tạo farm mới
- ✅ `UpdateFarmAsync` - Cập nhật `ImageUrl` khi update farm
- ✅ `MapToDto` - Map `ImageUrl` từ Entity sang DTO

#### WorkerService.cs
- ✅ `CreateWorkerAsync` - Gán `ImageUrl` khi tạo worker mới
- ✅ `UpdateWorkerAsync` - Cập nhật `ImageUrl` khi update worker
- ✅ `MapToDto` - Map `ImageUrl` từ Entity sang DTO

#### UserService.cs
- ✅ `CreateUserAsync` - Gán `ImageUrl` khi tạo user mới
- ✅ `UpdateUserAsync` - Cập nhật `ImageUrl` khi update user
- ✅ `MapToDto` - Map `ImageUrl` từ Entity sang DTO

## Cách sử dụng

### 1. Tạo mới với hình ảnh
```csharp
// Ví dụ với Product
var createDto = new CreateProductDto 
{
    Name = "Cà phê Robusta",
    Unit = "kg",
    Code = "CF_ROBUSTA",
    ImageUrl = "https://example.com/images/coffee.jpg"
};

// Ví dụ với Farm
var farmDto = new CreateFarmDto 
{
    Name = "Rẫy Đắk Mil",
    AreaSize = 2.5m,
    AddressGps = "Đắk Lắk",
    ImageUrl = "https://example.com/images/farm.jpg"
};

// Ví dụ với Worker
var workerDto = new CreateWorkerDto 
{
    FullName = "Nguyễn Văn A",
    Phone = "0901234567",
    ImageUrl = "https://example.com/images/worker.jpg"
};

// Ví dụ với User
var userDto = new CreateUserDto 
{
    Username = "farmer01",
    FullName = "Nguyễn Văn B",
    Email = "farmer@example.com",
    ImageUrl = "https://example.com/images/avatar.jpg"
};
```

### 2. Cập nhật hình ảnh
```csharp
// Update Product
var updateDto = new UpdateProductDto 
{
    Name = "Cà phê Robusta",
    ImageUrl = "https://example.com/images/new-coffee.jpg"
};
await productService.UpdateProductAsync(productId, updateDto);

// Update Farm
var farmUpdateDto = new UpdateFarmDto 
{
    Name = "Rẫy Đắk Mil",
    ImageUrl = "https://example.com/images/new-farm.jpg"
};
await farmService.UpdateFarmAsync(farmId, farmUpdateDto);
```

### 3. Hiển thị hình ảnh
```csharp
// Lấy dữ liệu
var product = await productService.GetProductByIdAsync(productId);
var imageUrl = product.ImageUrl ?? "https://example.com/default-image.jpg";

// Trong UI (React/Vue/etc)
<img src={imageUrl} alt={product.Name} />
```

## Lưu ý kỹ thuật

1. **Nullable Field**: Trường `ImageUrl` là nullable (không bắt buộc)
2. **Max Length**: Giới hạn tối đa 500 ký tự
3. **Data Type**: String/VARCHAR trong database
4. **Default Value**: NULL nếu không có hình ảnh
5. **Validation**: Có thêm validation cho length (500) trong DTOs

## Kiểm tra sau khi cập nhật

✅ Database migration đã apply thành công
✅ Tất cả Models đã có trường ImageUrl
✅ Tất cả DTOs đã có trường ImageUrl
✅ Tất cả Services đã xử lý mapping ImageUrl
✅ Project build thành công

## Các bước tiếp theo (Optional)

1. **Upload Service**: Tạo service để upload hình ảnh lên cloud storage (Cloudinary, AWS S3, Azure Blob, etc.)
2. **API Endpoint**: Thêm endpoint để upload hình ảnh
3. **Validation**: Thêm validation cho format file (jpg, png, etc.) và size
4. **UI Integration**: Tích hợp upload hình ảnh trong UI (React)

---
**Ngày cập nhật**: 02/01/2026
**Migration**: 20260102051403_AddImageUrlToModels
