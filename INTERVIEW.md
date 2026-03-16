# 🎯 INTERVIEW PREP — AgriLink Backend

> Tài liệu ôn thi phỏng vấn Backend .NET dựa trên dự án **AgriLink_DH**

---

## 📚 Mục Lục

1. [Transaction & ACID](#1-transaction--acid)
2. [Repository Pattern](#2-repository-pattern)
3. [Unit of Work Pattern](#3-unit-of-work-pattern)
4. [Cache-Aside Pattern (Redis)](#4-cache-aside-pattern-redis)
5. [Template Method Pattern](#5-template-method-pattern)
6. [Dependency Injection & Lifetime](#6-dependency-injection--lifetime)
7. [DTO Pattern](#7-dto-pattern)
8. [JWT Authentication](#8-jwt-authentication)
9. [OOP Principles](#9-oop-principles)
10. [Câu hỏi tổng hợp hay gặp](#10-câu-hỏi-tổng-hợp-hay-gặp)

---

## 1. Transaction & ACID

### Transaction là gì?
> Một nhóm các thao tác DB **"tất cả thành công hoặc tất cả thất bại"**.

### 4 tính chất ACID

| Chữ cái | Tên | Nghĩa đơn giản |
|---|---|---|
| **A** | Atomicity | All or nothing — không có trạng thái nửa vời |
| **C** | Consistency | Dữ liệu luôn hợp lệ trước & sau transaction |
| **I** | Isolation | Transactions chạy song song không ảnh hưởng nhau |
| **D** | Durability | Sau commit, data không bao giờ mất dù server crash |

### Trong AgriLink — 2 cách dùng

**Cách 1: Implicit Transaction (EF Core tự làm)**
```csharp
// ArticleService.cs
public async Task<ArticleDto> CreateArticleAsync(CreateArticleDto dto)
{
    await _articleRepository.AddAsync(article);  // chưa lưu DB
    // ... thêm các thao tác khác

    // EF Core tự bọc trong 1 transaction
    await _unitOfWork.SaveChangesAsync();  // ← tất cả commit 1 lần
}
```
> ✅ `SaveChangesAsync()` = 1 database transaction. Nếu lỗi → tự rollback toàn bộ.

**Cách 2: Explicit Transaction (thủ công)**
```csharp
// UnitOfWork.cs có hỗ trợ
await _unitOfWork.BeginTransactionAsync();
try
{
    await _farmRepo.ChangeOwner(farmId, newOwnerId);
    await _unitOfWork.SaveChangesAsync();

    await _userRepo.UpdateFarmCount(newOwnerId);
    await _unitOfWork.SaveChangesAsync();

    await _unitOfWork.CommitTransactionAsync();  // ✅ Commit tất cả
}
catch
{
    await _unitOfWork.RollbackTransactionAsync(); // ❌ Hoàn tác tất cả
    throw;
}
```
> Dùng khi cần nhiều `SaveChangesAsync()` nằm trong 1 transaction.

### Câu hỏi hay gặp

**Q: EF Core có tự dùng transaction không?**
> Có. Mỗi `SaveChangesAsync()` = 1 implicit transaction. Nhiều entity changes trong cùng 1 lần SaveChanges đều nằm chung transaction.

**Q: Khi nào cần explicit transaction?**
> Khi gọi `SaveChangesAsync()` nhiều lần và muốn all-or-nothing cho tất cả.

**Q: Transaction ảnh hưởng performance không?**
> Có. Transaction giữ DB lock → deadlock nếu không cẩn thận. Phải giữ transaction ngắn, không làm I/O blocking bên trong.

---

## 2. Repository Pattern

### Vấn đề giải quyết
> Service không nên biết đang dùng EF Core, SQL Server hay MongoDB. Nếu đổi DB → chỉ sửa Repository, không đụng Business Logic.

### Cấu trúc trong AgriLink

```
IRepository<T>          ← Generic interface (Domain layer)
    ↓ implements
BaseRepository<T>       ← Base implementation (Core layer)
    ↓ extends
ArticleRepository       ← Specific repository với custom queries
FarmRepository
CropSeasonRepository
... (22 repositories)
```

### Code thực tế

```csharp
// 1. Interface — Domain layer (không biết EF Core)
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, ...);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, ...);
    Task<T> AddAsync(T entity, ...);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> RemoveByIdAsync(Guid id, ...);
}

// 2. Base — implement toàn bộ generic CRUD
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public virtual async Task<T?> GetByIdAsync(Guid id, ...)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public virtual void Update(T entity) => _dbSet.Update(entity);
    // ...
}

// 3. Specific — chỉ override cần thiết, thêm query riêng
public class ArticleRepository : BaseRepository<Article>, IArticleRepository
{
    // Override để thêm Include (Eager Loading)
    public override async Task<Article?> GetByIdAsync(Guid id, ...)
        => await _dbSet.AsNoTracking()
                       .Include(a => a.Category)
                       .Include(a => a.Author)
                       .FirstOrDefaultAsync(a => a.Id == id, ct);

    // Custom query riêng của Article
    public async Task<Article?> GetBySlugAsync(string slug, ...)
        => await _dbSet.FirstOrDefaultAsync(a => a.Slug == slug, ct);

    public async Task IncrementViewCountAsync(Guid id, ...)
    {
        var article = await _dbSet.FindAsync(new object[] { id }, ct);
        if (article != null) { article.ViewCount++; _dbSet.Update(article); }
    }
}
```

### Câu hỏi hay gặp

**Q: Tại sao dùng `virtual` trong BaseRepository?**
> Để subclass có thể `override`. Ví dụ `ArticleRepository.GetByIdAsync()` override để thêm `.Include()` – BaseRepository không biết Article cần Include gì.

**Q: `AsNoTracking()` là gì?**
> EF Core tracking thay đổi entity để auto-detect changes. `AsNoTracking` tắt tính năng này → nhanh hơn cho READ-ONLY queries, nhưng không dùng cho Update.

**Q: Repository Pattern có nhược điểm không?**
> Có. Thêm abstraction layer → thêm code. Một số người cho rằng EF Core đã là Repository rồi (DbSet). Nhưng lợi ích testability và separation of concerns thường vượt trội.

---

## 3. Unit of Work Pattern

### Vấn đề giải quyết
> Có 5 repository, mỗi repo có SaveChanges riêng → 5 transactions riêng lẻ → data inconsistency. UoW gom tất cả vào 1 transaction.

### Code thực tế

```csharp
// Interface — Domain layer
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}

// Implementation — Lazy initialization
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IProductRepository? _products;

    // Lazy init: chỉ tạo khi cần lần đầu tiên
    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public async Task<int> SaveChangesAsync(...)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(...)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(...)
    {
        await _transaction!.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose() => _transaction?.Dispose();
}
```

### Cách Service dùng

```csharp
public class ArticleService
{
    private readonly IArticleRepository _repo;
    private readonly IUnitOfWork _unitOfWork;  // 1 điểm SaveChanges

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleDto dto)
    {
        var article = new Article { ... };
        await _repo.AddAsync(article);           // pending change

        await _unitOfWork.SaveChangesAsync();    // commit 1 lần
        return MapToDto(article);
    }
}
```

### Câu hỏi hay gặp

**Q: Repository vs Unit of Work khác nhau thế nào?**
> Repository = quản lý 1 loại entity. Unit of Work = quản lý transaction cho nhiều Repository cùng lúc.

**Q: `??=` (null-coalescing assignment) là gì?**
> `a ??= b` nghĩa là: nếu `a` là null thì gán `a = b`, sau đó return `a`. Đây là lazy initialization pattern.

---

## 4. Cache-Aside Pattern (Redis)

### Vấn đề giải quyết
> Query DB mỗi request → chậm, tốn tài nguyên. Redis cache ở RAM → nhanh hơn 100-1000 lần.

### Flow

```
Request
  │
  ▼
Check Redis ──── HIT ──→ Return ngay ✅ (< 1ms)
  │
  MISS
  │
  ▼
Query Database (~10-50ms)
  │
  ▼
Save to Redis (với TTL)
  │
  ▼
Return response
```

### Code thực tế — BaseCachedService

```csharp
public abstract class BaseCachedService
{
    protected readonly RedisService RedisService;

    protected async Task<T?> GetOrSetCacheAsync<T>(
        string cacheKey,
        Func<Task<T?>> fetchDataFunc,   // hàm fetch từ DB
        TimeSpan? expiration = null) where T : class
    {
        // 1. Try cache
        var cached = await RedisService.GetAsync<T>(cacheKey);
        if (cached != null) return cached;  // HIT

        // 2. Cache miss → fetch DB
        var fresh = await fetchDataFunc();
        if (fresh == null) return null;

        // 3. Lưu vào cache với TTL
        await RedisService.SetAsync(cacheKey, fresh, expiration);
        return fresh;
    }
}

// ArticleService sử dụng — cache 15 phút cho danh sách
public class ArticleService : BaseCachedService
{
    private const string CACHE_KEY_PUBLISHED = "articles:published";

    public async Task<IEnumerable<ArticleListItemDto>> GetPublishedArticlesAsync()
    {
        // Chỉ 3 dòng thay vì phải viết cache logic mỗi lần
        return await GetOrSetCacheListAsync<ArticleListItemDto>(
            cacheKey: CACHE_KEY_PUBLISHED,
            fetchDataFunc: async () => (await _articleRepo.GetPublishedArticlesAsync())
                                       .Select(MapToListItemDto),
            expiration: TimeSpan.FromMinutes(15)
        );
    }

    // Sau khi Update → phải xóa cache
    public async Task UpdateArticleAsync(Guid id, UpdateArticleDto dto)
    {
        // ... update logic ...
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        await InvalidateCacheAsync($"article:id:{id}");
        await InvalidateCacheAsync($"article:slug:{article.Slug}");
        await InvalidateCacheByPatternAsync("articles:*"); // Xóa tất cả list cache
    }
}
```

### Cache trong dự án

| Cache Key | TTL | Nội dung |
|---|---|---|
| `articles:published` | 15 phút | Danh sách bài đã publish |
| `articles:featured:{count}` | 15 phút | Bài nổi bật |
| `article:slug:{slug}` | 60 phút | Chi tiết bài theo slug |
| `article:id:{id}` | 60 phút | Chi tiết bài theo ID |
| `refresh:{userId}` | 7 ngày | JWT Refresh Token |

### Câu hỏi hay gặp

**Q: Cache-Aside vs Write-Through?**
> - Cache-Aside: App tự quản lý. Read miss → load → cache. Phổ biến hơn.
> - Write-Through: Mỗi write vào DB thì write luôn vào cache. Không bị miss nhưng cache nhiều data không cần thiết.

**Q: TTL (Time-To-Live) là gì? Đặt bao nhiêu?**
> Thời gian tồn tại của cache key. Hết TTL → Redis tự xóa. Đặt theo "độ tươi" cần thiết: list bài viết 15 phút (thay đổi thường xuyên), chi tiết bài 60 phút.

**Q: Stale data là gì?**
> Dữ liệu trong cache không còn đồng bộ với DB. Giải quyết bằng TTL hoặc cache invalidation khi có update/delete.

---

## 5. Template Method Pattern

### Vấn đề giải quyết
> 10 service đều cần cache logic giống nhau: check redis → fetch db → save redis. Tránh copy-paste bằng cách đưa vào abstract base class.

### Code thực tế

```csharp
// Base class định nghĩa "khung" (template)
public abstract class BaseCachedService
{
    // Template method — logic cố định, subclass không cần viết lại
    protected async Task<T?> GetOrSetCacheAsync<T>(
        string cacheKey,
        Func<Task<T?>> fetchDataFunc,  // ← "hook" subclass tự định nghĩa
        TimeSpan? expiration = null) { ... }

    protected async Task InvalidateCacheAsync(string key) { ... }
    protected async Task InvalidateCacheByPatternAsync(string pattern) { ... }
}

// Subclass chỉ cần gọi template, không viết lại cache logic
public class FarmService : BaseCachedService
{
    public async Task<FarmDto?> GetFarmByIdAsync(Guid id)
    {
        return await GetOrSetCacheAsync<FarmDto>(
            cacheKey: $"farm:{id}",
            fetchDataFunc: async () => {
                var farm = await _farmRepo.GetByIdAsync(id);
                return farm != null ? MapToDto(farm) : null;
            },
            expiration: TimeSpan.FromMinutes(30)
        );
    }
}
```

### Câu hỏi hay gặp

**Q: Template Method khác Strategy Pattern thế nào?**
> - Template Method: Subclass kế thừa và override/dùng method của base. Quan hệ IS-A (inheritance).
> - Strategy: Inject một "strategy object" vào từ ngoài. Quan hệ HAS-A (composition). Strategy linh hoạt hơn ở runtime.

---

## 6. Dependency Injection & Lifetime

### 3 Lifetime trong ASP.NET Core

```csharp
// SINGLETON — 1 instance toàn app (từ start đến shutdown)
services.AddSingleton<CloudinaryService>();
services.AddSingleton<IConnectionMultiplexer>(...); // Redis

// SCOPED — 1 instance per HTTP Request
services.AddScoped<ArticleService>();
services.AddScoped<IArticleRepository, ArticleRepository>();
services.AddDbContext<ApplicationDbContext>(...);  // Scoped by default!

// TRANSIENT — Tạo mới mỗi lần resolve
// (Dự án chưa dùng, dùng cho lightweight stateless services)
```

### Dễ nhớ

```
Singleton   = 1 lần → dùng mãi
Scoped      = 1 request → 1 instance
Transient   = Mỗi lần inject → mới
```

### Câu hỏi hay gặp

**Q: Có thể inject Scoped service vào Singleton không?**
> **Không được!** Singleton sống lâu hơn Scoped → "Captive Dependency" bug. Scoped service bị giữ mãi, không được dispose sau request → memory leak, data sharing giữa requests.

**Q: DbContext nên dùng lifetime nào?**
> **Scoped**. 1 DbContext per request → đảm bảo mọi thay đổi trong 1 request nằm trong cùng EF Core unit of work. Dispose sau request → clean state.

**Q: `AddHttpClient<WeatherService>()` là gì?**
> Typed HttpClient với HttpClientFactory. Tránh socket exhaustion (vấn đề khi new HttpClient() nhiều lần), tự động dispose, hỗ trợ retry policy.

---

## 7. DTO Pattern

### Vấn đề giải quyết
> - Không muốn expose field nhạy cảm (PasswordHash, InternalId...)
> - Domain Model có 25 fields nhưng list API chỉ cần 8 fields
> - API contract độc lập với Domain → thay đổi DB schema không break client

### Trong AgriLink

```
Domain Model            DTO (API Response)
─────────────           ──────────────────
Article                 ArticleListItemDto   ← Danh sách (ít field)
  - Id                    - Id
  - Title                 - Title
  - Content (dài!)        - ThumbnailUrl
  - Tags (JSON str)       - ViewCount
  - CreatedBy (internal)  - PublishedTimeAgo  ← Tính toán thêm
  - UpdatedBy (internal)
  - ...20 fields        ArticleDto           ← Chi tiết (nhiều field hơn)
                          - Id, Title, Content
                          - Tags: List<string> ← Đã deserialize từ JSON
                          - ...
```

### Câu hỏi hay gặp

**Q: AutoMapper hay manual mapping?**
> Dự án này dùng **manual mapping** (private method `MapToDto()`). AutoMapper tiện lợi nhưng ẩn mapping logic, khó debug. Manual mapping rõ ràng hơn, dễ kiểm soát.

**Q: Request DTO và Response DTO khác nhau?**
> Có. Request DTO (CreateArticleDto, UpdateArticleDto) nhận data từ client. Response DTO (ArticleDto, ArticleListItemDto) trả về client. Không bao giờ dùng chung vì requirement khác nhau.

---

## 8. JWT Authentication

### Flow trong AgriLink

```
ĐĂNG NHẬP:
POST /auth/login
  → Verify password (BCrypt)
  → GenerateTokenPair() → AccessToken (60 phút) + RefreshToken (7 ngày)
  → Lưu RefreshToken vào Redis với key "refresh:{userId}"
  → Trả về cả 2 token cho client

GỌI API:
Header: Authorization: Bearer {AccessToken}
  → JwtBearer Middleware verify chữ ký + hết hạn
  → Populate HttpContext.User.Claims

LÀM MỚI TOKEN:
POST /auth/refresh { refreshToken }
  → Tìm userId trong Redis by refreshToken
  → Sinh AccessToken mới + RefreshToken mới
  → Update Redis

ĐĂNG XUẤT:
POST /auth/logout
  → Xóa RefreshToken khỏi Redis → không thể refresh nữa
```

### Câu hỏi hay gặp

**Q: AccessToken vs RefreshToken?**
> - AccessToken: Ngắn hạn (60 phút), gửi mỗi request. Nếu bị đánh cắp → tồn tại tối đa 60 phút.
> - RefreshToken: Dài hạn (7 ngày), chỉ dùng để lấy AccessToken mới. Lưu ở Redis → có thể revoke ngay.

**Q: Tại sao lưu RefreshToken ở Redis chứ không phải DB?**
> Redis nhanh hơn (in-memory), TTL tự động expire, dễ revoke. Nếu lưu DB cần query mỗi lần refresh → chậm hơn.

**Q: JWT stateless nghĩa là gì?**
> Server không lưu session. AccessToken tự chứa thông tin (userId, role...) và chữ ký → server chỉ cần verify chữ ký, không cần lookup DB.

---

## 9. OOP Principles

### 4 Pillars trong AgriLink

#### Abstraction (Trừu tượng)
```csharp
// Client code chỉ biết interface, không biết implementation
IArticleRepository _repo;  // không biết là ArticleRepository hay MockRepository
IUnitOfWork _uow;          // không biết là UnitOfWork hay TestUnitOfWork
BaseCachedService          // abstract class ẩn cache logic
```

#### Encapsulation (Đóng gói)
```csharp
public class ArticleRepository : BaseRepository<Article>
{
    // _context và _dbSet là protected — chỉ class con truy cập
    // Business logic trong Services, không lộ ra Controller
    // PasswordHash không bao giờ xuất hiện trong DTO
}
```

#### Inheritance (Kế thừa)
```csharp
BaseRepository<T>    ← tái sử dụng CRUD
  └── ArticleRepository  ← kế thừa + override GetByIdAsync()

BaseCachedService    ← tái sử dụng cache logic
  └── ArticleService     ← kế thừa + dùng GetOrSetCacheAsync()
  └── AuthService        ← kế thừa + dùng cache cho user
```

#### Polymorphism (Đa hình)
```csharp
// Runtime polymorphism qua interface
IRepository<Article> repo = new ArticleRepository(context);
// Gọi GetByIdAsync() → ArticleRepository.GetByIdAsync() chạy (đã override)

// DI container resolve đúng implementation lúc runtime
services.AddScoped<IArticleRepository, ArticleRepository>();
// → khi inject IArticleRepository → nhận ArticleRepository
```

---

## 10. Câu Hỏi Tổng Hợp Hay Gặp

---

**Q: Solid Principles là gì? Dự án áp dụng thế nào?**

| Nguyên tắc | Áp dụng trong AgriLink |
|---|---|
| **S**ingle Responsibility | Controller chỉ routing, Service chỉ business logic, Repository chỉ data access |
| **O**pen/Closed | BaseRepository mở rộng bằng kế thừa, không sửa base code |
| **L**iskov Substitution | `ArticleRepository` thay thế được `BaseRepository<Article>` |
| **I**nterface Segregation | `IArticleRepository` chỉ có method của Article, không nhồi nhét |
| **D**ependency Inversion | Service depend on `IArticleRepository` (interface), không depend on `ArticleRepository` (implementation) |

---

**Q: Sự khác biệt giữa `IEnumerable` và `IQueryable`?**
> - `IEnumerable`: Dữ liệu đã load vào memory, filter ở C#
> - `IQueryable`: Chưa query DB, filter được dịch thành SQL → hiệu quả hơn

AgriLink dùng cả hai. `FindAsync(predicate)` dùng `Where(predicate).ToListAsync()` → EF Core dịch sang SQL.

---

**Q: `AsNoTracking()` ảnh hưởng gì?**
> EF Core theo dõi entity để biết cần UPDATE gì không. `AsNoTracking()` tắt tracking → không dùng được cho Update (phải load lại hoặc Attach). Nhanh hơn ~20-30% cho read-only.

---

**Q: Soft Delete là gì? Dự án dùng ở đâu?**
> Không xóa thật sự, chỉ đánh dấu `IsDeleted = true`. Farm model có `IsDeleted + DeletedAt`. Lợi ích: khôi phục được, audit history, không vỡ foreign key.

---

**Q: Tại sao dùng `Guid` làm Primary Key thay vì `int`?**
> - Guid: Unique globally, có thể generate ở client, không đoán được sequence. Nhược: 16 bytes (vs 4 bytes int), index lớn hơn, insert chậm hơn nếu dùng GUID v4 random (gây page split).
> - AgriLink dùng `Guid.NewGuid()` — OK cho scale vừa, distributed system.

---

**Q: `CancellationToken` là gì?**
> Cho phép cancel async operation khi client disconnect. Truyền qua toàn bộ call stack từ Controller → Service → Repository → EF Core. Nếu user tắt request giữa chừng → DB query bị cancel → giải phóng resource.

---

## 🗣️ Script Trả Lời — "Dự án anh dùng pattern gì?"

> *"Dự án AgriLink Backend của tôi áp dụng một số design pattern chính:*
>
> ***Repository Pattern*** để tách data access khỏi business logic. Tôi có generic `IRepository<T>` và `BaseRepository<T>` implement toàn bộ CRUD. Các entity-specific repository như `ArticleRepository` kế thừa base và override những method cần thêm eager loading qua `.Include()`.*
>
> ***Unit of Work Pattern*** để quản lý transaction. `IUnitOfWork` expose `SaveChangesAsync()` là điểm commit duy nhất. Điều này đảm bảo nhiều thay đổi trên nhiều bảng nằm trong cùng 1 database transaction — all or nothing.*
>
> ***Cache-Aside Pattern*** với Redis. `BaseCachedService` là abstract class cung cấp template method `GetOrSetCacheAsync()`. Service con chỉ cần truyền cache key và function fetch DB — base class lo toàn bộ: check Redis, fallback DB, save cache, TTL.*
>
> ***DTO Pattern*** để tách API contract khỏi Domain Model, tránh expose fields nhạy cảm, kiểm soát payload size.*
>
> Tất cả được wire lại qua **Extension Methods** để giữ `Program.cs` gọn gàng."

---

## ⚡ Quick Cheatsheet — Nhớ Khi Đi Phỏng Vấn

```
ACID:
  A = All or nothing (Atomicity)
  C = Luôn hợp lệ (Consistency)
  I = Transactions độc lập (Isolation)
  D = Commit không mất (Durability)

DI Lifetimes:
  Singleton  = 1 instance / toàn app
  Scoped     = 1 instance / request
  Transient  = mới mỗi lần inject

Patterns:
  Repository = tách data access
  UoW        = 1 transaction, nhiều repo
  Cache-Aside = Redis → DB fallback
  Template   = base class định khung
  DTO        = tách domain khỏi API response

OOP:
  S = Single Responsibility
  O = Open/Closed (mở rộng, không sửa)
  L = Liskov Substitution (subclass thay được superclass)
  I = Interface Segregation (interface nhỏ gọn)
  D = Dependency Inversion (depend on abstraction)
```

---

*Good luck! 🚀 — AgriLink Interview Prep v1.0*
