using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.User;

public class CreateUserDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [MinLength(3, ErrorMessage = "Tên đăng nhập tối thiểu 3 ký tự")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
