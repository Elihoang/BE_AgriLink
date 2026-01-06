namespace AgriLink_DH.Share.DTOs.CropSeason;

/// <summary>
/// DTO để cập nhật giai đoạn sinh trưởng của vụ mùa
/// </summary>
public class UpdateStageDto
{
    /// <summary>
    /// Giai đoạn hiện tại: "Sau thu hoạch", "Ra bông", "Đậu trái", etc.
    /// </summary>
    public string Stage { get; set; } = string.Empty;
    
    /// <summary>
    /// Ghi chú cho giai đoạn này
    /// </summary>
    public string? StageNotes { get; set; }
}
