using AgriLink_DH.Core.Services;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.CropSeason;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgriLink_DH.Tests.Services;

public class CropSeasonServiceTests
{
    private readonly Mock<ICropSeasonRepository> _mockCropRepository;
    private readonly Mock<IFarmRepository> _mockFarmRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly IUnitOfWork _mockUnitOfWorkObj; // Optional if not needed
    private readonly CropSeasonValidator _validator;
    private readonly CropSeasonService _service;

    public CropSeasonServiceTests()
    {
        _mockCropRepository = new Mock<ICropSeasonRepository>();
        _mockFarmRepository = new Mock<IFarmRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _validator = new CropSeasonValidator(
            _mockCropRepository.Object,
            _mockFarmRepository.Object,
            _mockProductRepository.Object
        );

        _service = new CropSeasonService(
            _mockCropRepository.Object,
            _mockFarmRepository.Object,
            _mockProductRepository.Object,
            _mockUnitOfWork.Object,
            _validator
        );
    }

    [Fact]
    public async Task GetAllSeasonsAsync_ShouldReturnAllSeasons()
    {
        // Arrange
        var season1 = new CropSeason(Guid.NewGuid(), Guid.NewGuid(), "Season 1");
        var season2 = new CropSeason(Guid.NewGuid(), Guid.NewGuid(), "Season 2");
        season2.ChangeStatus(SeasonStatus.Closed);
        
        var seasons = new List<CropSeason> { season1, season2 };

        _mockCropRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(seasons);

        // Act
        var result = await _service.GetAllSeasonsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "Season 1");
        result.Should().Contain(s => s.Name == "Season 2");
    }

    [Fact]
    public async Task CreateSeasonAsync_ShouldReturnDto_WhenFarmAndProductExist()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var seasonId = Guid.NewGuid();
        var dto = new CreateCropSeasonDto 
        { 
            FarmId = farmId, 
            ProductId = productId, 
            Name = "New Season",
            StartDate = DateTime.UtcNow
        };

        _mockFarmRepository.Setup(repo => repo.GetByIdAsync(farmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Farm(Guid.NewGuid(), "Test Farm"));

        _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product("Test Product"));

        _mockCropRepository.Setup(repo => repo.AddAsync(It.IsAny<CropSeason>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CropSeason season, CancellationToken ct) => season);
            
        // Act
        var result = await _service.CreateSeasonAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Season");
        result.FarmName.Should().Be("Test Farm");
        
        _mockCropRepository.Verify(repo => repo.AddAsync(It.Is<CropSeason>(s => s.Name == "New Season"), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateSeasonAsync_ShouldThrowException_WhenFarmNotFound()
    {
        // Arrange
        var dto = new CreateCropSeasonDto { FarmId = Guid.NewGuid() };

        _mockFarmRepository.Setup(repo => repo.GetByIdAsync(dto.FarmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Farm?)null);

        // Act
        var act = async () => await _service.CreateSeasonAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Không tìm thấy vườn với ID: {dto.FarmId}");
    }
}
