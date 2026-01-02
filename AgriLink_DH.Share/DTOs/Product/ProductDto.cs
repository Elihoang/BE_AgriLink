namespace AgriLink_DH.Share.DTOs.Product;

/// <summary>
/// DTO cho Product Response
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ImageUrl { get; set; }
}
