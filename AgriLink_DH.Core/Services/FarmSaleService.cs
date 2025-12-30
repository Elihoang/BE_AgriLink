using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.FarmSale;

namespace AgriLink_DH.Core.Services;

public class FarmSaleService
{
    private readonly IFarmSaleRepository _farmSaleRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FarmSaleService(
        IFarmSaleRepository farmSaleRepository,
        ICropSeasonRepository cropSeasonRepository,
        IUnitOfWork unitOfWork)
    {
        _farmSaleRepository = farmSaleRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
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
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        var revenue = dto.Quantity * dto.UnitPrice;

        var sale = new FarmSale
        {
            SeasonId = dto.SeasonId,
            SaleDate = dto.SaleDate.ToUniversalTime(),
            QuantitySold = dto.Quantity,
            PricePerKg = dto.UnitPrice,
            TotalRevenue = revenue,
            BuyerName = dto.BuyerName,
            Note = dto.Note
        };

        await _farmSaleRepository.AddAsync(sale);
        await _unitOfWork.SaveChangesAsync();

        sale.CropSeason = season;
        return MapToDto(sale);
    }

    public async Task<bool> DeleteSaleAsync(Guid id)
    {
        var sale = await _farmSaleRepository.GetByIdAsync(id);
        if (sale == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu bán hàng với ID: {id}");

        _farmSaleRepository.Remove(sale);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SoftDeleteSaleAsync(Guid id)
    {
        var sale = await _farmSaleRepository.GetByIdAsync(id);
        if (sale == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu bán hàng với ID: {id}");

        sale.IsDeleted = true;
        sale.DeletedAt = DateTime.UtcNow;

        _farmSaleRepository.Update(sale);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreSaleAsync(Guid id)
    {
        var sale = await _farmSaleRepository.GetByIdAsync(id);
        if (sale == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu bán hàng với ID: {id}");

        sale.IsDeleted = false;
        sale.DeletedAt = null;

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
