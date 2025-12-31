using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.PlantPosition;

/// <summary>
/// DTO cho việc tạo nhiều cây cùng lúc trong 1 rẫy
/// </summary>
public class BulkCreatePlantDto
{
    [Required]
    public Guid FarmId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 cây để tạo")]
    public List<CreatePlantPositionDto> Positions { get; set; } = new();
}
