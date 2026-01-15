# AgriLink_DH - API Backend

## 📋 Tổng quan

AgriLink_DH là hệ thống backend API cho ứng dụng quản lý nông nghiệp, được xây dựng với .NET 9.0 và PostgreSQL.

### Tech Stack
- **Framework**: .NET 9.0
- **Database**: PostgreSQL
- **Cache**: Redis
- **ORM**: Entity Framework Core
- **Authentication**: JWT
- **Logging**: Serilog
- **API Documentation**: Swagger/Scalar

---

## 🚀 Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Redis 7+](https://redis.io/download) (optional, cho caching)
- ~~[Docker](https://www.docker.com/get-started)~~ (⚠️ Chưa setup - chỉ có example files)

### Installation

#### ⭐ Recommended: Local Development
```bash
# 1. Clone repository
git clone <repository-url>
cd AgriLink_DH

# 2. Setup database connection trong appsettings.Development.json
# Create file: AgriLink_DH.Api/appsettings.Development.json

# 3. Restore dependencies
dotnet restore AgriLink_DH.sln

# 4. Run migrations
dotnet ef database update --project AgriLink_DH.Api

# 5. Run application
dotnet run --project AgriLink_DH.Api
```

#### 🐳 Docker (⚠️ EXAMPLE ONLY - Chưa setup thực tế)
⚠️ **Lưu ý**: Docker files được cung cấp chỉ là **ví dụ tham khảo**. Hiện tại project chưa có Docker setup thực tế.

```bash
# Nếu muốn thử Docker trong tương lai:
# 1. Copy environment file
cp .env.example .env
# Edit .env với actual values

# 3. Start services
docker-compose up -d

# 4. Check status
docker-compose ps
```

#### Option 2: Local Development
```bash
# 1. Restore dependencies
dotnet restore AgriLink_DH.sln

# 2. Setup database connection trong appsettings.Development.json

# 3. Run migrations
dotnet ef database update --project AgriLink_DH.Api

# 4. Run application
dotnet run --project AgriLink_DH.Api
```

### Verify Installation
```bash
# API should be running at:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001

# Access Swagger UI:
# http://localhost:5000/swagger
```

---

## 📁 Project Structure

```
AgriLink_DH/
├── .github/                    # GitHub Actions workflows
│   └── workflows/
│       ├── ci.yml             # CI pipeline
│       ├── cd.yml             # CD pipeline
│       └── pr-check.yml       # PR validation
│
├── AgriLink_DH.Api/           # Web API Layer
│   ├── Controllers/           # API Controllers
│   ├── Extensions/            # Service extensions
│   ├── Migrations/            # EF Core migrations
│   └── Program.cs            # Entry point
│
├── AgriLink_DH.Core/          # Business Logic Layer
│   ├── Services/             # Service implementations
│   └── Interfaces/           # Service contracts
│
├── AgriLink_DH.Domain/        # Domain Layer
│   ├── Entities/             # Domain models
│   ├── Repositories/         # Data access
│   └── ApplicationDbContext.cs
│
├── AgriLink_DH.Share/         # Shared Layer
│   ├── DTOs/                 # Data transfer objects
│   └── Common/               # Shared utilities
│
├── Dockerfile                 # Docker configuration
├── docker-compose.yml        # Docker Compose setup
├── .env.example              # Environment template
├── CI_CD_GUIDE.md           # CI/CD documentation
└── CI_CD_CHO_NGUOI_MOI.md   # Beginner's guide
```

---

## 🔧 Development

### Running Tests
```bash
# Run all tests
dotnet test AgriLink_DH.sln

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project AgriLink_DH.Api

# Update database
dotnet ef database update --project AgriLink_DH.Api

# Remove last migration
dotnet ef migrations remove --project AgriLink_DH.Api
```

### Code Formatting
```bash
# Check formatting
dotnet format AgriLink_DH.sln --verify-no-changes

# Auto format
dotnet format AgriLink_DH.sln
```

---

## 🔐 Configuration

### Environment Variables

Tạo file `.env` từ `.env.example`:

```env
# Database
POSTGRES_DB=agrilink_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password

# JWT
JWT_SECRET_KEY=your_very_long_and_secure_jwt_secret_key
JWT_ISSUER=AgriLink
JWT_AUDIENCE=AgriLinkUsers
JWT_EXPIRATION=60
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=agrilink_db;Username=postgres;Password=***",
    "RedisConnection": "localhost:6379"
  },
  "JwtSettings": {
    "SecretKey": "***",
    "Issuer": "AgriLink",
    "Audience": "AgriLinkUsers",
    "ExpirationInMinutes": 60
  }
}
```

---

## 🚦 CI/CD

### Available Workflows

1. **CI - Build & Test** (`ci.yml`)
   - Auto runs on push to main/develop
   - Builds solution
   - Runs tests
   - Code quality checks
   - Security scanning

2. **Deploy to Production** (`deploy-production.yml`)
   - Manual trigger only  
   - Multiple deployment options (all DISABLED by default)
   - Artifact generation

3. **PR Check** (`pr-check.yml`)
   - Auto runs on pull requests
   - Validates PR
   - Posts status comments

### Documentation
- **Chi tiết**: Xem [CI_CD_GUIDE.md](./CI_CD_GUIDE.md)
- **Người mới**: Xem [CI_CD_CHO_NGUOI_MOI.md](./CI_CD_CHO_NGUOI_MOI.md)

---

## 📚 API Documentation

### Swagger UI
- Development: http://localhost:5000/swagger
- Production: https://your-domain.com/swagger

### Scalar UI (Modern alternative)
- Development: http://localhost:5000/scalar/v1

### Common Endpoints

```
POST   /api/auth/login           # User login
POST   /api/auth/register        # User registration
GET    /api/farms                # Get all farms
GET    /api/farms/{id}           # Get farm by ID
POST   /api/farms                # Create new farm
PUT    /api/farms/{id}           # Update farm
DELETE /api/farms/{id}           # Delete farm
```

---

## 🐳 Docker Commands

```bash
# Build và start
docker-compose up -d --build

# View logs
docker-compose logs -f api

# Restart API
docker-compose restart api

# Stop all
docker-compose down

# Remove volumes (careful: deletes data)
docker-compose down -v

# Access database
docker-compose exec postgres psql -U postgres -d agrilink_db
```

---

## 🔍 Troubleshooting

### Database Connection Failed
```bash
# Check if PostgreSQL is running
docker-compose ps postgres

# Check connection string in appsettings
# Verify credentials match .env file
```

### Build Errors
```bash
# Clean solution
dotnet clean AgriLink_DH.sln

# Restore packages
dotnet restore AgriLink_DH.sln

# Rebuild
dotnet build AgriLink_DH.sln
```

### Migration Issues
```bash
# Drop database and recreate
dotnet ef database drop --project AgriLink_DH.Api
dotnet ef database update --project AgriLink_DH.Api
```

---

## 📖 Additional Documentation

- [CI/CD Guide](./CI_CD_GUIDE.md) - Comprehensive CI/CD documentation
- [Beginner's CI/CD Guide](./CI_CD_CHO_NGUOI_MOI.md) - Simple guide for beginners
- [Farmer Module README](./FARMER_MODULE_README.md) - Farmer module documentation

---

## 🤝 Contributing

1. Create feature branch: `git checkout -b feature/amazing-feature`
2. Commit changes: `git commit -m 'Add amazing feature'`
3. Push to branch: `git push origin feature/amazing-feature`
4. Open Pull Request

### Code Style
- Follow C# coding conventions
- Use meaningful variable/method names
- Add XML comments for public APIs
- Write unit tests for new features

---

## 📝 License

[Your License Here]

---

## 👥 Team

AgriLink Development Team

---

## 📞 Support

- Issues: [GitHub Issues](https://github.com/your-org/AgriLink_DH/issues)
- Docs: [Wiki](https://github.com/your-org/AgriLink_DH/wiki)

---

**Last Updated:** 2026-01-13
