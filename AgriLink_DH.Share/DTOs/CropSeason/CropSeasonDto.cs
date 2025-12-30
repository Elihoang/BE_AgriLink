using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.CropSeason;

public class CropSeasonDto
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SeasonStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? Note { get; set; }
}
