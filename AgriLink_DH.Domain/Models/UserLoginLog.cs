using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lịch sử đăng nhập - tracking security và phân tích hành vi người dùng
/// </summary>
public class UserLoginLog : BaseEntity
{

            public Guid UserId { get; set; }

    /// <summary>
    /// IP Address - để biết người dùng đăng nhập từ đâu
    /// Quan trọng để phát hiện đăng nhập bất thường từ địa điểm lạ
    /// </summary>
            public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User Agent - thông tin trình duyệt và thiết bị
    /// Giúp biết nông dân dùng điện thoại (ra vườn) hay máy tính (văn phòng)
    /// </summary>
            public string? DeviceInfo { get; set; }

    /// <summary>
    /// Thời gian đăng nhập
    /// </summary>
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Trạng thái đăng nhập: true = thành công, false = thất bại
    /// Để phát hiện ai đó đang dò mật khẩu (nhiều lần thất bại)
    /// </summary>
        public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Thông tin bổ sung: location, browser name, OS...
    /// Lưu dạng JSON để linh hoạt mở rộng sau
    /// </summary>
        public string? Metadata { get; set; }

    /// <summary>
    /// Action type: Login, Register, Logout, RefreshToken...
    /// </summary>
        public LoginActionType ActionType { get; set; } = LoginActionType.Login;

    // Navigation property
        public virtual User? User { get; set; }
}
