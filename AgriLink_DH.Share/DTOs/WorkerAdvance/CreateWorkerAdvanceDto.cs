using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.WorkerAdvance;

public class CreateWorkerAdvanceDto
{
    [Required]
    public Guid WorkerId { get; set; }

    [Required]
    public Guid SeasonId { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Số tiền ứng phải lớn hơn 0")]
    public decimal Amount { get; set; }

    public DateTime AdvanceDate { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }
}
