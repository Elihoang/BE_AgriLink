using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.Product;

/// <summary>
/// DTO cho cập nhật Product
/// </summary>
public class UpdateProductDto
{
    [Required(ErrorMessage = "Tên sản phẩm không được bỏ trống")]
    [MaxLength(100, ErrorMessage = "Tên sản phẩm không được quá 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Đơn vị không được quá 20 ký tự")]
    public string Unit { get; set; } = "kg";

    [MaxLength(20, ErrorMessage = "Mã sản phẩm không được quá 20 ký tự")]
    public string? Code { get; set; }

    [MaxLength(500, ErrorMessage = "URL hình ảnh không được quá 500 ký tự")]
    public string? ImageUrl { get; set; }
}
