namespace AgriLink_DH.Share.DTOs.FarmSale;

public class FarmSaleDto
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Revenue { get; set; }
    public string? BuyerName { get; set; }
    public string? Note { get; set; }
}
