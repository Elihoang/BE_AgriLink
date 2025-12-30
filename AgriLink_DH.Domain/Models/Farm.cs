using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Hồ sơ Vườn/Rẫy - Tài sản đất đai của nông dân
/// </summary>
[Table("farms")]
public class Farm
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("owner_user_id")]
    [Required]
    public Guid OwnerUserId { get; set; } // Link tới bảng Users (Tài khoản App)

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // "Rẫy Đắk Mil", "Vườn Sau Nhà"

    [Column("area_size")]
    [Precision(10, 2)]
    public decimal? AreaSize { get; set; } // Diện tích (Hecta). VD: 2.5

    [Column("address_gps")]
    [MaxLength(50)]
    public string? AddressGps { get; set; } // "12.3456, 108.4567" (Lấy weather API)

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<CropSeason> CropSeasons { get; set; } = new List<CropSeason>();
    public virtual ICollection<TaskType> TaskTypes { get; set; } = new List<TaskType>();
    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
    public virtual ICollection<WeatherLog> WeatherLogs { get; set; } = new List<WeatherLog>();

    // Soft Delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
