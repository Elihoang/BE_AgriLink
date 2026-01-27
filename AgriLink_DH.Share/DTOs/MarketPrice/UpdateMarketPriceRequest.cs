namespace AgriLink_DH.Share.DTOs.MarketPrice;

/// <summary>
/// DTO để Admin cập nhật giá thủ công
/// </summary>
public class UpdateMarketPriceRequest
{
    /// <summary>
    /// Product ID (Guid từ bảng products)
    /// Ví dụ: 00000000-0000-0000-0000-000000000001 (Cà phê Robusta)
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Mã khu vực (DAK_LAK, LAM_DONG, GIA_LAI, DAK_NONG)
    /// NULL = toàn quốc
    /// </summary>
    public string? RegionCode { get; set; }
    
    /// <summary>
    /// Tên khu vực (Đắk Lắk, Lâm Đồng...)
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// Giá (VND/kg)
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Thay đổi so với hôm trước
    /// </summary>
    public decimal Change { get; set; }
    
    /// <summary>
    /// Ngày ghi nhận (default = hôm nay)
    /// </summary>
    public DateTime? RecordedDate { get; set; }
    
    /// <summary>
    /// Nguồn (giacaphe.com, cafef.vn, etc.)
    /// </summary>
    public string? Source { get; set; }
}
