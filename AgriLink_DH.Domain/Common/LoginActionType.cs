namespace AgriLink_DH.Domain.Common;

/// <summary>
/// Loại hành động authentication/authorization
/// </summary>
public enum LoginActionType
{
    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    Register,
    
    /// <summary>
    /// Đăng nhập
    /// </summary>
    Login,
    
    /// <summary>
    /// Đăng xuất
    /// </summary>
    Logout,
    
    /// <summary>
    /// Refresh token
    /// </summary>
    RefreshToken,
    
    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    ChangePassword,
    
    /// <summary>
    /// Reset mật khẩu
    /// </summary>
    ResetPassword
}
