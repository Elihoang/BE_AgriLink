using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.Worker;

public class WorkerDto
{
    public Guid Id { get; set; }
    // FarmId removed as Worker is now global to User
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public WorkerType WorkerType { get; set; }
    public string WorkerTypeLabel { get; set; } = string.Empty;
    public decimal? DefaultDailyWage { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
}
