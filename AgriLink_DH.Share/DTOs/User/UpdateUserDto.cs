using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.User;

public class UpdateUserDto
{
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
