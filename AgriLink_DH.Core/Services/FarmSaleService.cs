using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.FarmSale;

namespace AgriLink_DH.Core.Services;

public class FarmSaleService
{
    private readonly IFarmSaleRepository _farmSaleRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FarmSaleValidator _validator;

    public FarmSaleService(
        IFarmSaleRepository farmSaleRepository,
        ICropSeasonRepository cropSeasonRepository,
        IUnitOfWork unitOfWork,
        FarmSaleValidator validator)
    {
        _farmSaleRepository = farmSaleRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IEnumerable<FarmSaleDto>> GetBySeasonAsync(Guid seasonId)
    {
        var sales = await _farmSaleRepository.GetBySeasonIdAsync(seasonId);
        return sales.Select(MapToDto);
    }

    public async Task<decimal> GetTotalRevenueAsync(Guid seasonId)
    {
        return await _farmSaleRepository.GetTotalRevenueBySeasonAsync(seasonId);
    }

    public async Task<FarmSaleDto?> GetByIdAsync(Guid id)
    {
        var sale = await _farmSaleRepository.GetByIdAsync(id);
        return sale != null ? MapToDto(sale) : null;
    }

    public async Task<FarmSaleDto> CreateSaleAsync(CreateFarmSaleDto dto)
    {
        await _validator.ValidateCreateSaleAsync(dto.SeasonId);
        var season = (await _cropSeasonRepository.GetByIdAsync(dto.SeasonId))!;

        var sale = new FarmSale(
            dto.SeasonId,
            dto.SaleDate.ToUniversalTime(),
            dto.BuyerName,
            dto.Quantity,
            dto.UnitPrice,
            dto.Note
        );

        await _farmSaleRepository.AddAsync(sale);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = MapToDto(sale);
        resultDto.SeasonName = season.Name;
        return resultDto;
    }

    public async Task<bool> DeleteSaleAsync(Guid id)
    {
        var sale = await _farmSaleRepository.GetByIdAsync(id);
        _validator.ValidateDeleteSale(sale, id);

        _farmSaleRepository.Remove(sale);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SoftDeleteSaleAsync(Guid id)
    {
        var sale = await _farmSaleRepository.GetByIdAsync(id);
        _validator.ValidateDeleteSale(sale, id);

        sale.SoftDelete();

        _farmSaleRepository.Update(sale);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreSaleAsync(Guid id)
    {
        var sale = await _farmSaleRepository.GetByIdAsync(id);
        _validator.ValidateDeleteSale(sale, id);

        sale.Restore();

        _farmSaleRepository.Update(sale);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static FarmSaleDto MapToDto(FarmSale sale)
    {
        return new FarmSaleDto
        {
            Id = sale.Id,
            SeasonId = sale.SeasonId,
            SeasonName = sale.CropSeason?.Name ?? string.Empty,
            SaleDate = sale.SaleDate,
            Quantity = sale.QuantitySold,
            Unit = "Kg",
            UnitPrice = sale.PricePerKg,
            Revenue = sale.TotalRevenue,
            BuyerName = sale.BuyerName,
            Note = sale.Note
        };
    }
}
