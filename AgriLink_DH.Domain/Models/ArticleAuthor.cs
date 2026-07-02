using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Thông tin tác giả bài viết
/// Có thể là chuyên gia, viện nghiên cứu, hoặc biên tập viên
/// </summary>
public class ArticleAuthor : BaseEntity
{

    /// <summary>
    /// Tên tác giả (ThS. Lê Văn Nam, Viện Nông Nghiệp,...)
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Học vị/Chức danh (ThS, TS, Kỹ sư,...)
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Tổ chức/Cơ quan (Viện KHKT Nông Lâm Nghiệp Tây Nguyên,...)
    /// </summary>
    public string? Organization { get; private set; }

    /// <summary>
    /// Email liên hệ
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// URL ảnh đại diện
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Tiểu sử/Giới thiệu ngắn
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Trạng thái xác minh tác giả (verified badge)
    /// </summary>
    public bool IsVerified { get; private set; } = false;

    /// <summary>
    /// Liên kết mạng xã hội (JSON)
    /// </summary>
    public string? SocialLinks { get; private set; } // JSON: {"facebook":"url","linkedin":"url"}

    /// <summary>
    /// Chuyên môn (JSON array)
    /// </summary>
    public string? Specialties { get; private set; } // JSON: ["Cà phê","Hồ tiêu","Canh tác hữu cơ"]

    /// <summary>
    /// Kích hoạt
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // Navigation Properties
    public virtual ICollection<Article> Articles { get; private set; } = new List<Article>();

    /// <summary>
    /// Required for EF Core instantiation
    /// </summary>
    protected ArticleAuthor() { }

    /// <summary>
    /// Tạo mới tác giả (Rich Domain Model Constructor)
    /// </summary>
    public ArticleAuthor(string name, string? title = null, string? organization = null, string? email = null, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên tác giả không được để trống.", nameof(name));

        Name = name.Trim();
        Title = title?.Trim();
        Organization = organization?.Trim();
        Email = email?.Trim();
        Phone = phone?.Trim();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        IsVerified = false;
    }

    // --- BEHAVIORS (Hành vi nghiệp vụ) ---

    public void UpdateBasicInfo(string name, string? title, string? organization)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên tác giả không được để trống.", nameof(name));

        Name = name.Trim();
        Title = title?.Trim();
        Organization = organization?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(string? email, string? phone)
    {
        Email = email?.Trim();
        Phone = phone?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string? bio, string? avatarUrl, string? socialLinks, string? specialties)
    {
        Bio = bio?.Trim();
        AvatarUrl = avatarUrl?.Trim();
        SocialLinks = socialLinks?.Trim();
        Specialties = specialties?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Verify()
    {
        if (!IsVerified)
        {
            IsVerified = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RevokeVerification()
    {
        if (IsVerified)
        {
            IsVerified = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
