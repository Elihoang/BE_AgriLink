using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

public class Material : BaseEntity
{

    public Guid OwnerUserId { get; private set; }

    public User? Owner { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Unit { get; private set; } = string.Empty; // kg, lít, bao, chai...

    public decimal QuantityInStock { get; private set; } = 0; // Số lượng tồn kho

    public decimal CostPerUnit { get; private set; } = 0; // Đơn giá ước tính (để tính chi phí khi xuất kho)

    public string? Note { get; private set; }

    public string? ImageUrl { get; private set; }

    public MaterialType MaterialType { get; private set; } = MaterialType.Other;

    public DateTime? ExpiryDate { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    protected Material() { }

    public Material(Guid ownerUserId, string name, string unit, MaterialType materialType, decimal costPerUnit = 0, string? note = null, string? imageUrl = null, DateTime? expiryDate = null)
    {
        if (ownerUserId == Guid.Empty) throw new ArgumentException("OwnerUserId không hợp lệ", nameof(ownerUserId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên vật tư không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Đơn vị tính không được để trống", nameof(unit));
        if (costPerUnit < 0) throw new ArgumentException("Đơn giá không được âm", nameof(costPerUnit));

        OwnerUserId = ownerUserId;
        Name = name.Trim();
        Unit = unit.Trim();
        MaterialType = materialType;
        CostPerUnit = costPerUnit;
        Note = note?.Trim();
        ImageUrl = imageUrl?.Trim();
        ExpiryDate = expiryDate;
        QuantityInStock = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string unit, MaterialType materialType, decimal costPerUnit, string? note, string? imageUrl, DateTime? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên vật tư không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Đơn vị tính không được để trống", nameof(unit));
        if (costPerUnit < 0) throw new ArgumentException("Đơn giá không được âm", nameof(costPerUnit));

        Name = name.Trim();
        Unit = unit.Trim();
        MaterialType = materialType;
        CostPerUnit = costPerUnit;
        Note = note?.Trim();
        ImageUrl = imageUrl?.Trim();
        ExpiryDate = expiryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Import(decimal quantity, decimal? newCostPerUnit = null)
    {
        if (quantity <= 0) throw new ArgumentException("Số lượng nhập phải lớn hơn 0", nameof(quantity));

        // Nếu có giá mới, tính giá bình quân gia quyền
        if (newCostPerUnit.HasValue && newCostPerUnit.Value >= 0)
        {
            var totalOldValue = QuantityInStock * CostPerUnit;
            var totalNewValue = quantity * newCostPerUnit.Value;
            var newTotalQuantity = QuantityInStock + quantity;
            
            CostPerUnit = (totalOldValue + totalNewValue) / newTotalQuantity;
        }

        QuantityInStock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Consume(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Số lượng xuất phải lớn hơn 0", nameof(quantity));
        if (QuantityInStock < quantity) throw new InvalidOperationException($"Không đủ số lượng trong kho. Hiện có: {QuantityInStock}");

        QuantityInStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdjustStock(decimal newQuantity)
    {
        if (newQuantity < 0) throw new ArgumentException("Số lượng tồn kho không được âm", nameof(newQuantity));
        QuantityInStock = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
