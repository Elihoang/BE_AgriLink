using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.Worker;

public class UpdateWorkerDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public decimal? DefaultDailyWage { get; set; }

    public WorkerType WorkerType { get; set; }
    public bool IsActive { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
