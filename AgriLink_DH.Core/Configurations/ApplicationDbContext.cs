using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Configurations;

/// <summary>
/// Database Context cho Farmer Module - AgriLink (Rẫy Số)
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    #region DbSets - Nhóm Hệ thống & Cấu hình
    public DbSet<User> Users { get; set; }
    public DbSet<UserLoginLog> UserLoginLogs { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Farm> Farms { get; set; }
    public DbSet<TaskType> TaskTypes { get; set; }
    #endregion

    #region DbSets - Nhóm Sản xuất & Chi phí
    public DbSet<CropSeason> CropSeasons { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<DailyWorkLog> DailyWorkLogs { get; set; }
    public DbSet<WorkAssignment> WorkAssignments { get; set; }
    public DbSet<WorkerAdvance> WorkerAdvances { get; set; }
    public DbSet<MaterialUsage> MaterialUsages { get; set; }
    #endregion

    #region DbSets - Nhóm Doanh thu & Kết quả
    public DbSet<HarvestSession> HarvestSessions { get; set; }
    public DbSet<HarvestBagDetail> HarvestBagDetails { get; set; }
    public DbSet<FarmSale> FarmSales { get; set; }
    public DbSet<WeatherLog> WeatherLogs { get; set; }
    public DbSet<PlantPosition> PlantPositions { get; set; } // Grid layout tracking từng cây
    public DbSet<Material> Materials { get; set; } // Quản lý kho vật tư
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // UNIQUE CONSTRAINTS & INDEXES
        // ========================================

        // User: Unique username and email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // UserLoginLog: Index on user_id + login_time for login history queries
        modelBuilder.Entity<UserLoginLog>()
            .HasIndex(ul => new { ul.UserId, ul.LoginTime });

        // Product: Unique code
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        // Farm: Index on owner_user_id for faster lookup
        modelBuilder.Entity<Farm>()
            .HasIndex(f => f.OwnerUserId);

        // Explicitly configure Foreign Key relationship
        modelBuilder.Entity<Farm>()
            .HasOne(f => f.Owner)
            .WithMany() // One-way relationship (User doesn't have Farms collection)
            .HasForeignKey(f => f.OwnerUserId)
            .IsRequired(); // Since OwnerUserId is [Required]

        // CropSeason: Index on farm_id + product_id for faster queries
        modelBuilder.Entity<CropSeason>()
            .HasIndex(cs => new { cs.FarmId, cs.ProductId });

        // DailyWorkLog: Index on season_id + work_date
        modelBuilder.Entity<DailyWorkLog>()
            .HasIndex(d => new { d.SeasonId, d.WorkDate });

        // HarvestSession: Index on season_id + harvest_date
        modelBuilder.Entity<HarvestSession>()
            .HasIndex(hs => new { hs.SeasonId, hs.HarvestDate });

        // FarmSale: Index on season_id + sale_date
        modelBuilder.Entity<FarmSale>()
            .HasIndex(fs => new { fs.SeasonId, fs.SaleDate });

        // PlantPosition: UNIQUE constraint on season_id + row + col (mỗi vị trí chỉ 1 cây)
        modelBuilder.Entity<PlantPosition>()
            .HasIndex(pp => new { pp.SeasonId, pp.RowNumber, pp.ColumnNumber })
            .IsUnique();

        // ========================================
        // CASCADE DELETE CONFIGURATIONS
        // ========================================

        // WorkAssignment: Cascade delete when DailyWorkLog is deleted
        modelBuilder.Entity<WorkAssignment>()
            .HasOne(wa => wa.DailyWorkLog)
            .WithMany(d => d.WorkAssignments)
            .HasForeignKey(wa => wa.LogId)
            .OnDelete(DeleteBehavior.Cascade);

        // HarvestBagDetail: Cascade delete when HarvestSession is deleted
        modelBuilder.Entity<HarvestBagDetail>()
            .HasOne(hbd => hbd.HarvestSession)
            .WithMany(hs => hs.HarvestBagDetails)
            .HasForeignKey(hbd => hbd.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================================
        // ENUM TO STRING CONVERSIONS (PostgreSQL)
        // ========================================

        // User: Role enum
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // UserLoginLog: ActionType enum
        modelBuilder.Entity<UserLoginLog>()
            .Property(ul => ul.ActionType)
            .HasConversion<string>();

        // SeasonStatus enum
        modelBuilder.Entity<CropSeason>()
            .Property(cs => cs.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Worker>()
            .Property(w => w.WorkerType)
            .HasConversion<string>();

        modelBuilder.Entity<WorkAssignment>()
            .Property(wa => wa.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<WeatherLog>()
            .Property(wl => wl.Condition)
            .HasConversion<string>();

        // MaterialType enum
        modelBuilder.Entity<Material>()
            .Property(m => m.MaterialType)
            .HasConversion<string>();

        // ========================================
        // DEFAULT VALUES & COMPUTED COLUMNS
        // ========================================

        // NetWeight calculation hint (can be enforced via trigger or computed column in PostgreSQL)
        // For EF Core, we'll calculate this in application logic or use database triggers

        // ========================================
        // SEED DATA (Optional - Admin tạo sẵn Products)
        // ========================================

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Cà phê Robusta",
                Unit = "kg",
                Code = "CF_ROBUSTA"
            },
            new Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Hồ Tiêu",
                Unit = "kg",
                Code = "PEPPER"
            },
            new Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Sầu Riêng",
                Unit = "kg",
                Code = "DURIAN"
            }
        );

        // Material: Index on owner_user_id + name (Unique per user)
        modelBuilder.Entity<Material>()
            .HasIndex(m => new { m.OwnerUserId, m.Name })
            .IsUnique();
    }
}
