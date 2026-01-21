namespace AgriLink_DH.Share.DTOs.MarketPrice;

/// <summary>
/// DTO cho giá một loại hàng hóa nông sản (cà phê, hồ tiêu, v.v.)
/// </summary>
public class CommodityPriceDto
{
    /// <summary>
    /// Tên hàng hóa (Robusta, Arabica, Hồ tiêu, v.v.)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mã hàng hóa (ROBUSTA, ARABICA, PEPPER, v.v.)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Giá hiện tại (VND/kg)
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Thay đổi so với ngày hôm trước (VND)
    /// </summary>
    public decimal Change { get; set; }

    /// <summary>
    /// Phần trăm thay đổi
    /// </summary>
    public decimal ChangePercent { get; set; }

    /// <summary>
    /// Đơn vị (VND/kg, USD/tấn, v.v.)
    /// </summary>
    public string Unit { get; set; } = "VND/kg";

    /// <summary>
    /// Thời gian cập nhật giá
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Nguồn dữ liệu (Cafef, ICE London, v.v.)
    /// </summary>
    public string Source { get; set; } = string.Empty;
}
