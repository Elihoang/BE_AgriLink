using AgriLink_DH.Core.Services;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;

namespace AgriLink_DH.Api.Extensions;


/// Extension methods để đăng ký Dependency Injection

public static class ServiceCollectionExtensions
{
    
    /// Đăng ký các Repository
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, Core.Repositories.UnitOfWork>();
        
        // Register Repositories
        services.AddScoped<IProductRepository, Core.Repositories.ProductRepository>();
        services.AddScoped<IUserRepository, Core.Repositories.UserRepository>();
        services.AddScoped<IFarmRepository, Core.Repositories.FarmRepository>();
        services.AddScoped<ICropSeasonRepository, Core.Repositories.CropSeasonRepository>();
        services.AddScoped<IWorkerRepository, Core.Repositories.WorkerRepository>();
        services.AddScoped<ITaskTypeRepository, Core.Repositories.TaskTypeRepository>();
        services.AddScoped<IDailyWorkLogRepository, Core.Repositories.DailyWorkLogRepository>();
        services.AddScoped<IWorkAssignmentRepository, Core.Repositories.WorkAssignmentRepository>();
        services.AddScoped<IWorkerAdvanceRepository, Core.Repositories.WorkerAdvanceRepository>();
        services.AddScoped<IMaterialUsageRepository, Core.Repositories.MaterialUsageRepository>();
        services.AddScoped<IHarvestSessionRepository, Core.Repositories.HarvestSessionRepository>();
        services.AddScoped<IHarvestBagDetailRepository, Core.Repositories.HarvestBagDetailRepository>();
        services.AddScoped<IFarmSaleRepository, Core.Repositories.FarmSaleRepository>();
        services.AddScoped<IWeatherLogRepository, Core.Repositories.WeatherLogRepository>();
        services.AddScoped<IUserLoginLogRepository, Core.Repositories.UserLoginLogRepository>();
        services.AddScoped<IPlantPositionRepository, Core.Repositories.PlantPositionRepository>();

        
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
        
        // Weather Service - Gọi OpenWeather API
        services.AddHttpClient<WeatherService>();
        services.AddScoped<WeatherService>();
        
        return services;
    }
}
