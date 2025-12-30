using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Vị trí từng cây trong rẫy - giống như sơ đồ ghế rạp chiếu phim
/// Mỗi record = 1 cây cụ thể tại vị trí (row, col)
/// </summary>
[Table("plant_positions")]
public class PlantPosition
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("season_id")]
    public Guid SeasonId { get; set; }

    /// <summary>
    /// Số hàng (row) - VD: hàng 1, 2, 3...
    /// </summary>
    [Column("row_number")]
    public int RowNumber { get; set; }

    /// <summary>
    /// Số cột (column) - VD: cột 1, 2, 3...
    /// </summary>
    [Column("column_number")]
    public int ColumnNumber { get; set; }

    /// <summary>
    /// Loại cây - Foreign Key tới bảng Products
    /// VD: ProductId của "Cà phê Arabica", "Sầu riêng Monthong"
    /// </summary>
    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Ngày trồng cây này
    /// </summary>
    [Column("plant_date")]
    public DateTime? PlantDate { get; set; }

    /// <summary>
    /// Tình trạng sức khỏe
    /// </summary>
    [Column("health_status")]
    public PlantHealthStatus HealthStatus { get; set; } = PlantHealthStatus.Healthy;

    /// <summary>
    /// Năng suất ước tính (kg/năm) của cây này
    /// </summary>
    [Column("estimated_yield")]
    [Precision(10, 2)]
    public decimal? EstimatedYield { get; set; }

    /// <summary>
    /// Ghi chú: "Cây này bệnh vàng lá", "Thay cây mới 15/3"...
    /// </summary>
    [Column("note")]
    public string? Note { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SeasonId))]
    public virtual CropSeason CropSeason { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}
