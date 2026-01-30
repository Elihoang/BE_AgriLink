namespace AgriLink_DH.Domain.Common;

/// <summary>
/// Trạng thái bài viết
/// </summary>
public enum ArticleStatus
{
    /// <summary>
    /// Bản nháp - chưa xuất bản
    /// </summary>
    Draft,
    
    /// <summary>
    /// Đã xuất bản - hiển thị công khai
    /// </summary>
    Published,
    
    /// <summary>
    /// Đã lưu trữ - không hiển thị nhưng vẫn lưu
    /// </summary>
    Archived,
    
    /// <summary>
    /// Chờ duyệt - đang chờ biên tập viên phê duyệt
    /// </summary>
    PendingReview,
    
    /// <summary>
    /// Bị từ chối - không được duyệt
    /// </summary>
    Rejected
}
