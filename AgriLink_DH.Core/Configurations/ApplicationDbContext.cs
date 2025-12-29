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
    public DbSet<Product> Products { get; set; }
    public DbSet<Farm> Farms { get; set; }
    public DbSet<TaskType> TaskTypes { get; set; }
    #endregion

    #region DbSets - Nhóm Sản xuất & Chi phí
    public DbSet<CropSeason> CropSeasons { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<WorkSession> WorkSessions { get; set; }
    public DbSet<WorkSessionDetail> WorkSessionDetails { get; set; }
    public DbSet<WorkerAdvance> WorkerAdvances { get; set; }
    public DbSet<MaterialUsage> MaterialUsages { get; set; }
    #endregion

    #region DbSets - Nhóm Doanh thu & Kết quả
    public DbSet<HarvestSession> HarvestSessions { get; set; }
    public DbSet<HarvestBagDetail> HarvestBagDetails { get; set; }
    public DbSet<FarmSale> FarmSales { get; set; }
    public DbSet<WeatherLog> WeatherLogs { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // UNIQUE CONSTRAINTS & INDEXES
        // ========================================

        // Product: Unique code
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        // Farm: Index on owner_user_id for faster lookup
        modelBuilder.Entity<Farm>()
            .HasIndex(f => f.OwnerUserId);

        // CropSeason: Index on farm_id + product_id for faster queries
        modelBuilder.Entity<CropSeason>()
            .HasIndex(cs => new { cs.FarmId, cs.ProductId });

        // Worker: Index on farm_id + is_active
        modelBuilder.Entity<Worker>()
            .HasIndex(w => new { w.FarmId, w.IsActive });

        // WorkSession: Index on season_id + work_date
        modelBuilder.Entity<WorkSession>()
            .HasIndex(ws => new { ws.SeasonId, ws.WorkDate });

        // HarvestSession: Index on season_id + harvest_date
        modelBuilder.Entity<HarvestSession>()
            .HasIndex(hs => new { hs.SeasonId, hs.HarvestDate });

        // FarmSale: Index on season_id + sale_date
        modelBuilder.Entity<FarmSale>()
            .HasIndex(fs => new { fs.SeasonId, fs.SaleDate });

        // ========================================
        // CASCADE DELETE CONFIGURATIONS
        // ========================================

        // WorkSessionDetail: Cascade delete when WorkSession is deleted
        modelBuilder.Entity<WorkSessionDetail>()
            .HasOne(wsd => wsd.WorkSession)
            .WithMany(ws => ws.WorkSessionDetails)
            .HasForeignKey(wsd => wsd.SessionId)
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

        modelBuilder.Entity<CropSeason>()
            .Property(cs => cs.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Worker>()
            .Property(w => w.WorkerType)
            .HasConversion<string>();

        modelBuilder.Entity<WorkSessionDetail>()
            .Property(wsd => wsd.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<WeatherLog>()
            .Property(wl => wl.Condition)
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
    }
}
