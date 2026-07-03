using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Sổ Bán Hàng - Doanh thu (Tiền thực tế thu về - Cash flow)
/// </summary>
public class FarmSale : SoftDeletableEntity
{
    public Guid SeasonId { get; private set; } // Tiền của vụ nào
    public DateTime SaleDate { get; private set; } = DateTime.UtcNow.Date;
    public string? BuyerName { get; private set; }
    public decimal QuantitySold { get; private set; }
    public decimal PricePerKg { get; private set; }
    public decimal TotalRevenue { get; private set; }

    public string? Note { get; private set; }

    // Navigation Properties
    public virtual CropSeason CropSeason { get; private set; } = null!;

    // Soft Delete inherited from SoftDeletableEntity

    protected FarmSale() { }

    public FarmSale(Guid seasonId, DateTime saleDate, string? buyerName, decimal quantitySold, decimal pricePerKg, string? note = null)
    {
        Id = Guid.NewGuid();
        SeasonId = seasonId;
        SaleDate = saleDate.Date;
        BuyerName = buyerName;
        QuantitySold = quantitySold;
        PricePerKg = pricePerKg;
        TotalRevenue = quantitySold * pricePerKg;
        Note = note;
        IsDeleted = false;
    }

    // SoftDelete and Restore are inherited
}
