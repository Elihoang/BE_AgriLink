using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Nhật ký Thời tiết - Lưu lịch sử môi trường
/// </summary>
[Table("weather_logs")]
public class WeatherLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("farm_id")]
    [Required]
    public Guid FarmId { get; set; }

    [Column("log_date")]
    public DateTime LogDate { get; set; } = DateTime.UtcNow.Date;

    [Column("condition")]
    [MaxLength(50)]
    public WeatherCondition Condition { get; set; } 

    [Column("rainfall_mm")]
    [Precision(5, 2)]
    public decimal? RainfallMm { get; set; } // Lượng mưa (nếu có)

    [Column("note")]
    public string? Note { get; set; } // "Mưa đá rụng trái", "Hạn hán kéo dài"

    // Navigation Properties
    [ForeignKey(nameof(FarmId))]
    public virtual Farm Farm { get; set; } = null!;
}
