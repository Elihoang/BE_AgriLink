namespace AgriLink_DH.Domain.Common;

/// <summary>
/// Vai trò người dùng trong hệ thống
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Người dùng thường - Nông dân
    /// </summary>
    User,
    
    /// <summary>
    /// Nông dân - Chủ vườn cà phê/tiêu
    /// </summary>
    Farmer,
    
    /// <summary>
    /// Kế toán - Quản lý tài chính
    /// </summary>
    Accountant,
    
    /// <summary>
    /// Quản trị viên hệ thống
    /// </summary>
    Admin
}
