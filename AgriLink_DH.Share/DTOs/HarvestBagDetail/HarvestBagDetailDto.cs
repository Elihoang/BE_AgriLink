namespace AgriLink_DH.Share.DTOs.HarvestBagDetail;

public class HarvestBagDetailDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int BagIndex { get; set; }
    public decimal GrossWeight { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetWeight { get; set; }
}
