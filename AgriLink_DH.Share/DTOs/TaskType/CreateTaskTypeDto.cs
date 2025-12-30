using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.TaskType;

public class CreateTaskTypeDto
{
    [Required]
    public Guid FarmId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? DefaultUnit { get; set; }

    public decimal? DefaultPrice { get; set; }
}
