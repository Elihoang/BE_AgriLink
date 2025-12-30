namespace AgriLink_DH.Share.DTOs.HarvestSession;

public class HarvestSessionDto
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public DateTime HarvestDate { get; set; }
    public int TotalBags { get; set; }
    public decimal TotalWeight { get; set; }
    public string? StorageLocation { get; set; }
}
