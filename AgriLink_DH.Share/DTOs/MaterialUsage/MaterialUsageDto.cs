namespace AgriLink_DH.Share.DTOs.MaterialUsage;

public class MaterialUsageDto
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public DateTime UsageDate { get; set; }
    public Guid? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public string? MaterialImageUrl { get; set; }
    public string? Note { get; set; }
}
