namespace AgriLink_DH.Share.DTOs.MarketPrice;

/// <summary>
/// DTO cho giá theo khu vực (tỉnh thành)
/// </summary>
public class RegionalPriceDto
{
    /// <summary>
    /// Tên khu vực (Đắk Lắk, Gia Lai, Lâm Đồng, v.v.)
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Mã khu vực
    /// </summary>
    public string RegionCode { get; set; } = string.Empty;

    /// <summary>
    /// Giá cà phê (VND/kg)
    /// </summary>
    public decimal CoffeePrice { get; set; }

    /// <summary>
    /// Giá hồ tiêu (VND/kg)
    /// </summary>
    public decimal? PepperPrice { get; set; }

    /// <summary>
    /// Thay đổi so với ngày hôm trước
    /// </summary>
    public decimal Change { get; set; }

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
