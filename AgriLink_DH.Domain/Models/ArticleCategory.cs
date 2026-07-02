using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Danh mục bài viết tri thức nông nghiệp
/// Phân loại: Kỹ thuật canh tác, Phòng bệnh, Giống cây trồng, Thị trường,...
/// </summary>
public class ArticleCategory : BaseEntity
{

    /// <summary>
    /// Tên danh mục (Kỹ thuật canh tác, Phòng bệnh, Thị trường,...)
    /// </summary>
                public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mã danh mục (enum type-safe)
    /// </summary>
        public ArticleCategoryType Code { get; set; }

    /// <summary>
    /// Mô tả danh mục
    /// </summary>
            public string? Description { get; set; }

    /// <summary>
    /// Icon/Emoji đại diện
    /// </summary>
            public string? Icon { get; set; }

    /// <summary>
    /// Màu sắc nhận diện (hex color)
    /// </summary>
            public string? Color { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
        public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Kích hoạt sử dụng
    /// </summary>
        public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
        public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
