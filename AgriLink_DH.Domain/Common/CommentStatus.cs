namespace AgriLink_DH.Domain.Common;

/// <summary>
/// Trạng thái bình luận
/// </summary>
public enum CommentStatus
{
    /// <summary>
    /// Đang hiển thị
    /// </summary>
    Active,
    
    /// <summary>
    /// Bị ẩn (vi phạm quy định)
    /// </summary>
    Hidden,
    
    /// <summary>
    /// Đã xóa
    /// </summary>
    Deleted
}
