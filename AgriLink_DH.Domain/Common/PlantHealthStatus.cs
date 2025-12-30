namespace AgriLink_DH.Domain.Common;

/// <summary>
/// Tình trạng sức khỏe của cây trồng
/// </summary>
public enum PlantHealthStatus
{
    /// <summary>
    /// Khỏe mạnh, phát triển tốt
    /// </summary>
    Healthy,

    /// <summary>
    /// Đang bệnh (vàng lá, sâu bệnh...)
    /// </summary>
    Sick,

    /// <summary>
    /// Chết, cần thay thế
    /// </summary>
    Dead,

    /// <summary>
    /// Đã nhổ bỏ/loại bỏ
    /// </summary>
    Removed,

    /// <summary>
    /// Mới trồng, đang trong giai đoạn chăm sóc đặc biệt
    /// </summary>
    NewlyPlanted
}
