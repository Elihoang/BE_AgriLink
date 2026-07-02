using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Hồ sơ Vườn/Rẫy - Tài sản đất đai của nông dân
/// </summary>
public class Farm : SoftDeletableEntity
{

    public Guid OwnerUserId { get; private set; } // Link tới bảng Users (Tài khoản App)

    public string Name { get; private set; } = string.Empty; // "Rẫy Đắk Mil", "Vườn Sau Nhà"
    
    public decimal? AreaSize { get; private set; } // Diện tích (Hecta). VD: 2.5

    public string? AddressGps { get; private set; } 

    /// <summary>
    /// Vĩ độ (Latitude) - Người dùng chọn trên map
    /// </summary>
    public decimal? Latitude { get; private set; } // VD: 12.6667000 (Đắk Lắk)

    /// <summary>
    /// Kinh độ (Longitude) - Người dùng chọn trên map
    /// </summary>
    public decimal? Longitude { get; private set; } // VD: 108.0500000 (Đắk Lắk)

    public string? GoogleMapsUrl { get; private set; } // Link gốc Google Maps

    public string? ImageUrl { get; private set; } // URL hình ảnh trang trại

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Soft Delete inherited from SoftDeletableEntity

    // Navigation Properties
    public virtual User? Owner { get; private set; }

    public virtual ICollection<CropSeason> CropSeasons { get; private set; } = new List<CropSeason>();
    public virtual ICollection<TaskType> TaskTypes { get; private set; } = new List<TaskType>();
    public virtual ICollection<Worker> Workers { get; private set; } = new List<Worker>();
    public virtual ICollection<WeatherLog> WeatherLogs { get; private set; } = new List<WeatherLog>();

    protected Farm() { }

    public Farm(Guid ownerUserId, string name, decimal? areaSize = null, string? addressGps = null, decimal? latitude = null, decimal? longitude = null, string? googleMapsUrl = null, string? imageUrl = null)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("Chủ vườn không hợp lệ", nameof(ownerUserId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên vườn không được để trống", nameof(name));

        OwnerUserId = ownerUserId;
        Name = name.Trim();
        AreaSize = areaSize;
        AddressGps = addressGps?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        GoogleMapsUrl = googleMapsUrl?.Trim();
        ImageUrl = imageUrl?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, decimal? areaSize, string? addressGps, decimal? latitude, decimal? longitude, string? googleMapsUrl, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên vườn không được để trống", nameof(name));

        Name = name.Trim();
        AreaSize = areaSize;
        AddressGps = addressGps?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        GoogleMapsUrl = googleMapsUrl?.Trim();
        if (imageUrl != null) ImageUrl = imageUrl.Trim();
    }

    // SoftDelete and Restore are inherited
}
