using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Danh mục Cây trồng Hệ thống 
/// </summary>
public class Product : BaseEntity
{

    public string Name { get; private set; } = string.Empty; // "Cà phê Robusta", "Hồ Tiêu"

    public string Unit { get; private set; } = "kg"; // Đơn vị tính chuẩn

    public string? Code { get; private set; } // "CF_ROBUSTA", "PEPPER" (Dùng để map logic code)

    public string? ImageUrl { get; private set; } // URL hình ảnh sản phẩm

    // Navigation Properties
    public virtual ICollection<CropSeason> CropSeasons { get; private set; } = new List<CropSeason>();

    protected Product() { }

    public Product(string name, string unit = "kg", string? code = null, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên sản phẩm/cây trồng không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Đơn vị tính không được để trống", nameof(unit));

        Name = name.Trim();
        Unit = unit.Trim();
        Code = code?.Trim().ToUpper();
        ImageUrl = imageUrl?.Trim();
    }

    public void UpdateDetails(string name, string unit, string? code, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên sản phẩm/cây trồng không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Đơn vị tính không được để trống", nameof(unit));

        Name = name.Trim();
        Unit = unit.Trim();
        Code = code?.Trim().ToUpper();
        ImageUrl = imageUrl?.Trim();
    }
}
