using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.DailyWorkLog;

public class UpdateDailyWorkLogDto
{
    public DateTime WorkDate { get; set; }

    public Guid? TaskTypeId { get; set; }

    public string? Note { get; set; }
}
