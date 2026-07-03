using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Material;

namespace AgriLink_DH.Core.Services;

public class MaterialService
{
    private readonly IMaterialRepository _materialRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MaterialValidator _validator;

    public MaterialService(IMaterialRepository materialRepository, IUnitOfWork unitOfWork, MaterialValidator validator)
    {
        _materialRepository = materialRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IEnumerable<MaterialDto>> GetMyMaterialsAsync(Guid userId)
    {
        var materials = await _materialRepository.GetByUserIdAsync(userId);
        return materials.Select(MapToDto);
    }

    public async Task<MaterialDto> CreateMaterialAsync(Guid userId, CreateMaterialDto dto)
    {
        await _validator.ValidateCreateAsync(userId, dto);

        var material = new Material(userId, dto.Name, dto.Unit, dto.MaterialType, dto.CostPerUnit, dto.Note, dto.ImageUrl, dto.ExpiryDate);
        if (dto.QuantityInStock > 0)
        {
            material.Import(dto.QuantityInStock, dto.CostPerUnit);
        }

        await _materialRepository.AddAsync(material);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(material);
    }

    public async Task<MaterialDto> UpdateMaterialAsync(Guid userId, Guid id, UpdateMaterialDto dto)
    {
        var material = await _materialRepository.GetByIdAsync(id);
        await _validator.ValidateUpdateAsync(userId, material, dto);

        material.UpdateDetails(dto.Name, dto.Unit, dto.MaterialType, dto.CostPerUnit, dto.Note, dto.ImageUrl, dto.ExpiryDate);
        
        if (material.QuantityInStock != dto.QuantityInStock)
        {
            material.AdjustStock(dto.QuantityInStock);
        }

        _materialRepository.Update(material);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(material);
    }

    public async Task DeleteMaterialAsync(Guid userId, Guid id)
    {
        var material = await _materialRepository.GetByIdAsync(id);
        _validator.ValidateDelete(userId, material);

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
