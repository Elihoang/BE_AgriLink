using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.TaskType;

public class UpdateTaskTypeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? DefaultUnit { get; set; }

    public decimal? DefaultPrice { get; set; }
}
