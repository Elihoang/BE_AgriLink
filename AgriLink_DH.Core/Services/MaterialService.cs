using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Material;

namespace AgriLink_DH.Core.Services;

public class MaterialService
{
    private readonly IMaterialRepository _materialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MaterialService(IMaterialRepository materialRepository, IUnitOfWork unitOfWork)
    {
        _materialRepository = materialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MaterialDto>> GetMyMaterialsAsync(Guid userId)
    {
        var materials = await _materialRepository.GetByUserIdAsync(userId);
        return materials.Select(MapToDto);
    }

    public async Task<MaterialDto> CreateMaterialAsync(Guid userId, CreateMaterialDto dto)
    {
        if (await _materialRepository.ExistsByNameAndUserAsync(dto.Name, userId))
        {
            throw new ArgumentException($"Vật tư '{dto.Name}' đã tồn tại trong kho của bạn.");
        }

        var material = new Material
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            Name = dto.Name,
            Unit = dto.Unit,
            QuantityInStock = dto.QuantityInStock,
            CostPerUnit = dto.CostPerUnit,
            Note = dto.Note,
            ImageUrl = dto.ImageUrl,
            MaterialType = dto.MaterialType,
            ExpiryDate = dto.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        await _materialRepository.AddAsync(material);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(material);
    }

    public async Task<MaterialDto> UpdateMaterialAsync(Guid userId, Guid id, UpdateMaterialDto dto)
    {
        var material = await _materialRepository.GetByIdAsync(id);
        if (material == null || material.OwnerUserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy vật tư.");
        }

        // Check duplicate name if name changed
        if (material.Name.ToLower() != dto.Name.ToLower())
        {
            if (await _materialRepository.ExistsByNameAndUserAsync(dto.Name, userId))
            {
                throw new ArgumentException($"Vật tư '{dto.Name}' đã tồn tại.");
            }
        }

        material.Name = dto.Name;
        material.Unit = dto.Unit;
        material.QuantityInStock = dto.QuantityInStock; // Cho phép sửa trực tiếp tồn kho (kiểm kê)
        material.CostPerUnit = dto.CostPerUnit;
        material.Note = dto.Note;
        material.ImageUrl = dto.ImageUrl;
        material.MaterialType = dto.MaterialType;
        material.ExpiryDate = dto.ExpiryDate;
        material.UpdatedAt = DateTime.UtcNow;

        _materialRepository.Update(material);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(material);
    }

    public async Task DeleteMaterialAsync(Guid userId, Guid id)
    {
        var material = await _materialRepository.GetByIdAsync(id);
        if (material == null || material.OwnerUserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy vật tư.");
        }

        // TODO: Check if material is used in any Usage log? (Later)

        _materialRepository.Remove(material);
        await _unitOfWork.SaveChangesAsync();
    }

    private static MaterialDto MapToDto(Material m)
    {
        return new MaterialDto
        {
            Id = m.Id,
            Name = m.Name,
            Unit = m.Unit,
            QuantityInStock = m.QuantityInStock,
            CostPerUnit = m.CostPerUnit,
            Note = m.Note,
            ImageUrl = m.ImageUrl,
            MaterialType = m.MaterialType,
            ExpiryDate = m.ExpiryDate,
            CreatedAt = m.CreatedAt
        };
    }
}
