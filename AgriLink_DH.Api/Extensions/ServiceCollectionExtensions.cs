using AgriLink_DH.Core.Services;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Infrastructure.Repositories;

namespace AgriLink_DH.Api.Extensions;


/// Extension methods để đăng ký Dependency Injection

public static class ServiceCollectionExtensions
{
    
    /// Đăng ký các Repository
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register UnitOfWork (from Infrastructure)
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register Repositories (from Infrastructure)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();
        services.AddScoped<ICropSeasonRepository, CropSeasonRepository>();
        services.AddScoped<IWorkerRepository, WorkerRepository>();
        services.AddScoped<ITaskTypeRepository, TaskTypeRepository>();
        services.AddScoped<IDailyWorkLogRepository, DailyWorkLogRepository>();
        services.AddScoped<IWorkAssignmentRepository, WorkAssignmentRepository>();
        services.AddScoped<IWorkerAdvanceRepository, WorkerAdvanceRepository>();
        services.AddScoped<IMaterialUsageRepository, MaterialUsageRepository>();
        services.AddScoped<IHarvestSessionRepository, HarvestSessionRepository>();
        services.AddScoped<IHarvestBagDetailRepository, HarvestBagDetailRepository>();
        services.AddScoped<IFarmSaleRepository, FarmSaleRepository>();
        services.AddScoped<IWeatherLogRepository, WeatherLogRepository>();
        services.AddScoped<IUserLoginLogRepository, UserLoginLogRepository>();
        services.AddScoped<IPlantPositionRepository, PlantPositionRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<ISalaryPaymentRepository, SalaryPaymentRepository>();
        services.AddScoped<IMarketPriceRepository, MarketPriceRepository>();
        
        // Article System Repositories
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>();
        services.AddScoped<IArticleAuthorRepository, ArticleAuthorRepository>();
        services.AddScoped<IArticleCommentRepository, ArticleCommentRepository>();
        services.AddScoped<IArticleLikeRepository, ArticleLikeRepository>();
        
        return services;
    }

    
    /// Đăng ký các Service
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<AuthService>();
        services.AddScoped<FarmService>();
        services.AddScoped<CropSeasonService>();
        services.AddScoped<RedisService>();
        services.AddScoped<Core.Helpers.JwtHelper>();
        services.AddScoped<WorkerService>();
        services.AddScoped<TaskTypeService>();
        services.AddScoped<DailyWorkLogService>();
        services.AddScoped<WorkerAdvanceService>();
        services.AddScoped<MaterialUsageService>();
        services.AddScoped<HarvestSessionService>();
        services.AddScoped<HarvestBagDetailService>();
        services.AddScoped<FarmSaleService>();
        services.AddScoped<WeatherLogService>();
        services.AddScoped<UserService>();
        services.AddScoped<PlantPositionService>();
        services.AddScoped<MaterialService>();
        
        // Weather Service - Gọi OpenWeather API
        services.AddHttpClient<WeatherService>();
        services.AddScoped<WeatherService>();
        
        // Market Price Service - Quản lý giá nông sản (Database)
        services.AddScoped<MarketPriceDbService>();
        
        // Article System Services
        services.AddScoped<ArticleService>();
        services.AddScoped<ArticleCategoryService>();
        services.AddScoped<ArticleAuthorService>();
        services.AddScoped<ArticleCommentService>();
        services.AddScoped<ArticleLikeService>();
        
        // Cloudinary - Upload ảnh
        services.AddSingleton<CloudinaryService>();
        

        // Salary Payment and Momo Services
        services.AddScoped<SalaryPaymentService>();
        services.AddScoped<IMomoService, MockMomoService>();

        return services;
    }
}
