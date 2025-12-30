namespace AgriLink_DH.Domain.Common;

/// <summary>
/// Phương thức thanh toán công lao động
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Công nhật - Trả theo ngày
    /// </summary>
    Daily,
    
    /// <summary>
    /// Khoán sản phẩm - Trả theo kg, gốc, etc.
    /// </summary>
    Product
}
