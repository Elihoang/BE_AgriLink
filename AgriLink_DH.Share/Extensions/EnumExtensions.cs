using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.Extensions;

/// <summary>
/// Extension methods cho các enum - để lấy tên tiếng Việt
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Lấy tên tiếng Việt của UserRole
    /// </summary>
    public static string ToVietnamese(this UserRole role)
    {
        return role switch
        {
            UserRole.User => "Người dùng",
            UserRole.Farmer => "Nông dân",
            UserRole.Accountant => "Kế toán",
            UserRole.Admin => "Quản trị viên",
            _ => role.ToString()
        };
    }

    /// <summary>
    /// Lấy tên tiếng Việt của LoginActionType
    /// </summary>
    public static string ToVietnamese(this LoginActionType action)
    {
        return action switch
        {
            LoginActionType.Register => "Đăng ký",
            LoginActionType.Login => "Đăng nhập",
            LoginActionType.Logout => "Đăng xuất",
            LoginActionType.RefreshToken => "Làm mới token",
            LoginActionType.ChangePassword => "Đổi mật khẩu",
            LoginActionType.ResetPassword => "Đặt lại mật khẩu",
            _ => action.ToString()
        };
    }

    /// <summary>
    /// Lấy tên tiếng Việt của SeasonStatus
    /// </summary>
    public static string ToVietnamese(this SeasonStatus status)
    {
        return status switch
        {
            SeasonStatus.Active => "Đang hoạt động",
            SeasonStatus.Closed => "Đã kết thúc",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Lấy tên tiếng Việt của PaymentMethod
    /// </summary>
    public static string ToVietnamese(this PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Daily => "Công nhật",
            PaymentMethod.Product => "Khoán sản phẩm",
            _ => method.ToString()
        };
    }

    /// <summary>
    /// Lấy tên tiếng Việt của WeatherCondition
    /// </summary>
    public static string ToVietnamese(this WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => "Nắng",
            WeatherCondition.Rainy => "Mưa",
            WeatherCondition.Cloudy => "Âm u",
            _ => condition.ToString()
        };
    }

    /// <summary>
    /// Lấy tên tiếng Việt của WorkerType
    /// </summary>
    public static string ToVietnamese(this WorkerType type)
    {
        return type switch
        {
            WorkerType.Permanent => "Thợ ruột",
            WorkerType.Seasonal => "Thời vụ",
            _ => type.ToString()
        };
    }
}
