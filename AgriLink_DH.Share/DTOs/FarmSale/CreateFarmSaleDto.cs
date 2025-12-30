using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.FarmSale;

public class CreateFarmSaleDto
{
    [Required]
    public Guid SeasonId { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [MaxLength(20)]
    public string? Unit { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [MaxLength(100)]
    public string? BuyerName { get; set; }

    public string? Note { get; set; }
}
