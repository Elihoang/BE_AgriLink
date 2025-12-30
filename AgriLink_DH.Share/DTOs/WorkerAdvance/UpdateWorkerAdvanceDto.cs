using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.WorkerAdvance;

public class UpdateWorkerAdvanceDto
{
    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Số tiền ứng phải lớn hơn 0")]
    public decimal Amount { get; set; }

    public DateTime AdvanceDate { get; set; }
    
    public bool IsDeducted { get; set; }

    public string? Note { get; set; }
}
