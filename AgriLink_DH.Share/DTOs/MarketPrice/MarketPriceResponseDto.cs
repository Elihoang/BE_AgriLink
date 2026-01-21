namespace AgriLink_DH.Share.DTOs.MarketPrice;

/// <summary>
/// Response DTO tổng hợp cho trang Market Price
/// </summary>
public class MarketPriceResponseDto
{
    /// <summary>
    /// Giá các loại hàng hóa chính
    /// </summary>
    public List<CommodityPriceDto> Commodities { get; set; } = new();

    /// <summary>
    /// Giá theo khu vực
    /// </summary>
    public List<RegionalPriceDto> RegionalPrices { get; set; } = new();

    /// <summary>
    /// Thời gian lấy dữ liệu
    /// </summary>
    public DateTime FetchedAt { get; set; }

    /// <summary>
    /// Có phải dữ liệu từ cache không
    /// </summary>
    public bool IsFromCache { get; set; }
}
