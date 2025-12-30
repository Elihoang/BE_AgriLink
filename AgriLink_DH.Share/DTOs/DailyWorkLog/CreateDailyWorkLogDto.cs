using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.DailyWorkLog;

public class CreateDailyWorkLogDto
{
    [Required]
    public Guid SeasonId { get; set; }

    public DateTime WorkDate { get; set; } = DateTime.UtcNow;

    public Guid? TaskTypeId { get; set; }

    public string? Note { get; set; }
}
