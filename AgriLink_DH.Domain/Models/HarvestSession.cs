using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

public class HarvestSession : SoftDeletableEntity
{
    public Guid SeasonId { get; private set; } // Thu hoạch của vụ nào
    public DateTime HarvestDate { get; private set; } = DateTime.UtcNow.Date;
    public int TotalBags { get; private set; } = 0; // Tổng số bao
    public decimal TotalWeight { get; private set; } = 0; // Tổng kg

    public string? StorageLocation { get; private set; } // "KHO_NHA", "DAI_LY_A"

    // Navigation Properties
    public virtual CropSeason CropSeason { get; private set; } = null!;
    public virtual ICollection<HarvestBagDetail> HarvestBagDetails { get; private set; } = new List<HarvestBagDetail>();

    // Soft Delete inherited from SoftDeletableEntity

    protected HarvestSession() { }

    public HarvestSession(Guid seasonId, DateTime harvestDate, string? storageLocation = null)
    {
        Id = Guid.NewGuid();
        SeasonId = seasonId;
        HarvestDate = harvestDate.Date;
        StorageLocation = storageLocation;
        TotalBags = 0;
        TotalWeight = 0;
        IsDeleted = false;
    }

    public void UpdateDetails(DateTime harvestDate, string? storageLocation)
    {
        HarvestDate = harvestDate.Date;
        StorageLocation = storageLocation;
    }

    public void AddBag(decimal netWeight)
    {
        TotalBags++;
        TotalWeight += netWeight;
    }

    public void RemoveBag(decimal netWeight)
    {
        if (TotalBags > 0) TotalBags--;
        TotalWeight -= netWeight;
    }

    public void UpdateBagWeight(decimal oldNetWeight, decimal newNetWeight)
    {
        TotalWeight -= oldNetWeight;
        TotalWeight += newNetWeight;
    }

    public void RecalculateTotals(IEnumerable<HarvestBagDetail> details)
    {
        var validDetails = details.Where(d => !d.IsDeleted && !d.IsDraft).ToList();
        TotalBags = validDetails.Count;
        TotalWeight = validDetails.Sum(d => d.NetWeight);
    }

    // SoftDelete and Restore are inherited
}
