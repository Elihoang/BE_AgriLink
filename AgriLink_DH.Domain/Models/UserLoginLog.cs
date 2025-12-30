using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lịch sử đăng nhập - tracking security và phân tích hành vi người dùng
/// </summary>
[Table("user_login_logs")]
public class UserLoginLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// IP Address - để biết người dùng đăng nhập từ đâu
    /// Quan trọng để phát hiện đăng nhập bất thường từ địa điểm lạ
    /// </summary>
    [Column("ip_address")]
    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User Agent - thông tin trình duyệt và thiết bị
    /// Giúp biết nông dân dùng điện thoại (ra vườn) hay máy tính (văn phòng)
    /// </summary>
    [Column("device_info")]
    [MaxLength(500)]
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Thời gian đăng nhập
    /// </summary>
    [Column("login_time")]
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Trạng thái đăng nhập: true = thành công, false = thất bại
    /// Để phát hiện ai đó đang dò mật khẩu (nhiều lần thất bại)
    /// </summary>
    [Column("is_success")]
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Thông tin bổ sung: location, browser name, OS...
    /// Lưu dạng JSON để linh hoạt mở rộng sau
    /// </summary>
    [Column("metadata")]
    public string? Metadata { get; set; }

    /// <summary>
    /// Action type: Login, Register, Logout, RefreshToken...
    /// </summary>
    [Column("action_type")]
    public LoginActionType ActionType { get; set; } = LoginActionType.Login;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
