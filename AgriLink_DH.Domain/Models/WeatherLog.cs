using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Nhật ký Thời tiết - Lưu lịch sử môi trường
/// </summary>
public class WeatherLog : BaseEntity
{

            public Guid FarmId { get; set; }

        public DateTime LogDate { get; set; } = DateTime.UtcNow.Date;

            public WeatherCondition Condition { get; set; }
    public decimal? RainfallMm { get; set; } // Lượng mưa (nếu có)

        public string? Note { get; set; } // "Mưa đá rụng trái", "Hạn hán kéo dài"

    // Navigation Properties
        public virtual Farm Farm { get; set; } = null!;
}
