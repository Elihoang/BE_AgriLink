using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.Worker;

public class CreateWorkerDto
{

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public decimal? DefaultDailyWage { get; set; }

    public WorkerType WorkerType { get; set; } = WorkerType.Seasonal;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(20)]
    public string? MomoPhone { get; set; }

    [MaxLength(50)]
    public string? BankAccount { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }
}
