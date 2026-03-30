# 🌾 AgriLink Backend API

> **"Rẫy Số"** — Hệ thống quản lý nông nghiệp thông minh dành cho nông dân Tây Nguyên

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql)
![Redis](https://img.shields.io/badge/Redis-Aiven-DC382D?style=for-the-badge&logo=redis)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens)

---

## 📋 Mục Lục

- [Tổng quan](#tổng-quan)
- [Kiến trúc](#kiến-trúc)
- [Tech Stack](#tech-stack)
- [Cài đặt & Chạy](#cài-đặt--chạy)
- [Cấu hình môi trường](#cấu-hình-môi-trường)
- [API Endpoints](#api-endpoints)
- [Design Patterns](#design-patterns)

---

## 🌟 Tổng Quan

AgriLink là REST API hỗ trợ nông dân trong việc:

| Tính năng | Mô tả |
|---|---|
| 🌱 Quản lý vườn/rẫy | Tạo và quản lý hồ sơ trang trại, vị trí GPS |
| 📅 Mùa vụ | Theo dõi các mùa vụ, vị trí cây trồng |
| 👷 Nhân công | Quản lý nhân công, chấm công, ứng lương |
| 🧪 Vật tư | Theo dõi sử dụng vật tư (phân, thuốc...) |
| 🧺 Thu hoạch | Ghi nhận phiên thu hoạch, chi tiết bao hàng |
| 💰 Bán hàng | Quản lý giao dịch bán nông sản |
| 🌤️ Thời tiết | Tự động lấy dự báo thời tiết theo vị trí GPS |
| 📰 Bài viết | Hệ thống tri thức nông nghiệp (Blog, tin tức) |
| 📊 Giá thị trường | Cập nhật giá nông sản từ nhiều tỉnh thành |
| 🔐 Xác thực | JWT Authentication với Refresh Token |

---

## 🏗️ Kiến Trúc

Dự án được tổ chức theo **4-Layer Clean Architecture**:

```
AgriLink_DH/
├── 📁 AgriLink_DH.Api           # Presentation Layer
│   ├── Controllers/             # 25 API Controllers
│   ├── Extensions/              # DI Registration
│   ├── Migrations/              # EF Core Migrations
│   └── Program.cs               # App Entry Point
│
├── 📁 AgriLink_DH.Core          # Business Logic Layer
│   ├── Configurations/          # DbContext
│   ├── Repositories/            # 24 Repository Implementations
│   ├── Services/                # 27 Service Classes
│   └── Helpers/                 # JWT, Slug, IP, Excel helpers
│
├── 📁 AgriLink_DH.Domain        # Domain Layer (Core Business)
│   ├── Models/                  # 23 Entity Models
│   ├── Interface/               # Repository & UoW Interfaces
│   └── Common/                  # Enums (UserRole, ArticleStatus...)
│
└── 📁 AgriLink_DH.Share         # Shared Layer
    └── DTOs/                    # ~79 DTO files (24 groups)
```

**Dependency Flow:**
```
Api → Core → Domain ← Share
```

Domain không phụ thuộc vào bất kỳ layer nào → đảm bảo Domain Model thuần túy.

---

## 🛠️ Tech Stack

| Thành phần | Technology | Ghi chú |
|---|---|---|
| Framework | ASP.NET Core 8 | Minimal APIs + Controllers |
| ORM | Entity Framework Core 8 | Code-First, Migrations |
| Database | PostgreSQL | Hosted on Aiven Cloud |
| Cache | Redis | Hosted on Aiven Cloud, SSL |
| Auth | JWT Bearer | Access + Refresh Token |
| Password | BCrypt.Net | Hashing |
| Upload ảnh | Cloudinary | Cloud image storage |
| Logging | Serilog | Console + File (rolling) |
| API Docs | Scalar | Thay thế Swagger UI |
| Email/SMS | _(to-do)_ | — |

---

## 🚀 Cài Đặt & Chạy

### Yêu cầu hệ thống
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 14+
- Redis (local hoặc Aiven)

### Clone & Restore

```bash
git clone <repo-url>
cd AgriLink_DH
dotnet restore
```

### Cấu hình môi trường

Copy file mẫu và điền giá trị:

```bash
cp .env.example .env
```

Hoặc edit `AgriLink_DH.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=agrilink;Username=...;Password=...;SSL Mode=Require",
    "Redis": "rediss://...@...aiven.com:...?ssl=true"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "AgriLink",
    "Audience": "AgriLinkClient",
    "AccessTokenExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "7"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

### Chạy Migrations

```bash
cd AgriLink_DH.Api
dotnet ef database update
```

### Chạy ứng dụng

```bash
dotnet run --project AgriLink_DH.Api
```

API sẽ chạy tại: `https://localhost:7xxx`  
Scalar API Docs: `https://localhost:7xxx/scalar/v1`

---

## 🔌 API Endpoints

### 🔐 Auth
| Method | Endpoint | Mô tả |
|---|---|---|
| POST | `/api/auth/register` | Đăng ký tài khoản |
| POST | `/api/auth/login` | Đăng nhập |
| POST | `/api/auth/refresh` | Làm mới Access Token |
| POST | `/api/auth/logout` | Đăng xuất |
| GET | `/api/auth/me` | Thông tin user hiện tại |

### 🌾 Farm Management
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/farms` | Danh sách trang trại |
| POST | `/api/farms` | Tạo trang trại mới |
| PUT | `/api/farms/{id}` | Cập nhật trang trại |
| DELETE | `/api/farms/{id}` | Xoá trang trại (Soft Delete) |

### 📰 Article System
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/articles` | Danh sách bài viết đã publish |
| GET | `/api/articles/featured` | Bài viết nổi bật |
| GET | `/api/articles/{slug}` | Chi tiết bài viết theo slug |
| POST | `/api/articles` | Tạo bài viết mới |
| POST | `/api/articles/{id}/publish` | Xuất bản bài viết |

### 📊 Market Price
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/market-price` | Giá nông sản hiện tại |
| GET | `/api/market-price/history` | Lịch sử giá |
| GET | `/api/market-price/provinces` | Giá theo tỉnh thành |

> 📄 Xem toàn bộ tại Scalar: `/scalar/v1`

---

## 🎨 Design Patterns

### Repository Pattern
```csharp
// Generic interface cho mọi entity
IRepository<T> → BaseRepository<T> → FarmRepository, ArticleRepository...
```

### Unit of Work
```csharp
// Quản lý transaction & SaveChanges tập trung
await _unitOfWork.SaveChangesAsync();
```

### Cache-Aside (Redis)
```csharp
// Đọc Redis → miss → đọc DB → lưu Redis
protected async Task<T?> GetOrSetCacheAsync<T>(cacheKey, fetchFunc, expiration)
```

### DTO Pattern
```csharp
// Tách Domain Model ra khỏi API response
Article (Domain) → ArticleDto / ArticleListItemDto (Share)
```

Chi tiết xem trong [agrilink_be_architecture.md](docs/).

---

## 📁 Cấu Trúc Log

```
Logs/
├── all/          # Tất cả logs (Debug+), rolling theo ngày
└── errors/       # Chỉ Error+, rolling theo ngày
```

---

## 🐳 Docker

```bash
# Build & run với Docker Compose
docker-compose up -d
```

---

## 👥 Đóng Góp

1. Fork repo
2. Tạo branch: `git checkout -b feature/ten-tinh-nang`
3. Commit: `git commit -m "feat: them tinh nang X"`
4. Push & tạo Pull Request

---

*AgriLink — Kết nối nông dân với công nghệ số* 🌱
