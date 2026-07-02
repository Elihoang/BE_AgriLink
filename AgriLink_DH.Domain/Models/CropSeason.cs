using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Niên vụ - Trái tim của hệ thống. Tách bạch chi phí Xen canh
/// </summary>
public class CropSeason : SoftDeletableEntity
{

    public Guid FarmId { get; private set; }

    public Guid ProductId { get; private set; } // QUAN TRỌNG: Vụ này của cây gì?

    public string Name { get; private set; } = string.Empty; // "Vụ Cà 2025", "Vụ Tiêu 2025"

    public DateTime? StartDate { get; private set; }

    public DateTime? EndDate { get; private set; }

    public SeasonStatus Status { get; private set; } = SeasonStatus.Active;

    // Growth Stage Tracking
    public string? CurrentStage { get; private set; } // "Ra bông", "Đậu trái", etc.
    
    public DateTime? StageChangedAt { get; private set; }
    
    public string? StageNotes { get; private set; }

    public string? Note { get; private set; }

    // Soft Delete inherited from SoftDeletableEntity

    // Navigation Properties
    public virtual Farm Farm { get; private set; } = null!;

    public virtual Product Product { get; private set; } = null!;

    public virtual ICollection<DailyWorkLog> DailyWorkLogs { get; private set; } = new List<DailyWorkLog>();
    public virtual ICollection<WorkerAdvance> WorkerAdvances { get; private set; } = new List<WorkerAdvance>();
    public virtual ICollection<MaterialUsage> MaterialUsages { get; private set; } = new List<MaterialUsage>();
    public virtual ICollection<HarvestSession> HarvestSessions { get; private set; } = new List<HarvestSession>();
    public virtual ICollection<FarmSale> FarmSales { get; private set; } = new List<FarmSale>();

    protected CropSeason() { }

    public CropSeason(Guid farmId, Guid productId, string name, DateTime? startDate = null, DateTime? endDate = null, string? note = null)
    {
        if (farmId == Guid.Empty) throw new ArgumentException("FarmId không hợp lệ", nameof(farmId));
        if (productId == Guid.Empty) throw new ArgumentException("ProductId không hợp lệ", nameof(productId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên niên vụ không được để trống", nameof(name));

        FarmId = farmId;
        ProductId = productId;
        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Note = note?.Trim();
        Status = SeasonStatus.Active;
    }

    public void UpdateDetails(string name, DateTime? startDate, DateTime? endDate, string? note)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên niên vụ không được để trống", nameof(name));

        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Note = note?.Trim();
    }

    public void ChangeStatus(SeasonStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == SeasonStatus.Closed && !EndDate.HasValue)
        {
            EndDate = DateTime.UtcNow;
        }
    }

    public void UpdateGrowthStage(string stage, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(stage)) throw new ArgumentException("Giai đoạn sinh trưởng không được để trống", nameof(stage));

        CurrentStage = stage.Trim();
        StageChangedAt = DateTime.UtcNow;
        StageNotes = notes?.Trim();
    }

    // SoftDelete and Restore are inherited
}
