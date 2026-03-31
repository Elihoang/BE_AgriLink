using System.Text;
using AgriLink_DH.Api.Extensions;
using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Serilog;
using Serilog.Events;

// Enable legacy timestamp behavior for Npgsql to handle DateTime kinds flexibly
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ========================================
//  Serilog Configuration - LOG TẤT CẢ BẠO GỒM ERRORS
// ========================================
Log.Logger = new LoggerConfiguration()
    // Global minimum level
    .MinimumLevel.Debug()

    // Reduce noisy logs
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)

    // Enrich
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()

    // ===== CONSOLE (color by level) =====
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{Section}] {Message:lj}{NewLine}{Exception}"
    )

    // ===== FILE: ALL LOGS =====
    .WriteTo.File(
        path: "Logs/all/agrilink-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{Section}] {SourceContext}{NewLine}    {Message:lj}{NewLine}{Exception}"
    )

    // ===== FILE: ERRORS ONLY =====
    .WriteTo.File(
        path: "Logs/errors/error-.log",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{Section}] {SourceContext}{NewLine}    {Message:lj}{NewLine}{Exception}{NewLine}---"
    )

    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("[STARTUP] Starting AgriLink API");
Log.Information("[LOGGING] Level: Debug (All logs including errors)");
Log.Information("[LOGGING] All logs: Logs/all/agrilink-YYYYMMDD.log");
Log.Information("[LOGGING] Errors only: Logs/errors/error-YYYYMMDD.log");

// Add services to the container.
builder.Services.AddSignalR(); // Add Real-time mapping

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enum thành string thay vì số
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add DbContext - PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("AgriLink_DH.Api")
    )
);

// Add Redis (Aiven) - SSL với ConfigurationOptions để bypass Aiven self-signed cert
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis")!;
    var options = ConfigurationOptions.Parse(connectionString);

    // Bắt buộc cho Aiven: bỏ qua SSL cert validation (Aiven dùng self-signed cert)
    options.CertificateValidation += (sender, certificate, chain, sslPolicyErrors) => true;

    // Tăng timeout cho cloud Redis (network latency cao hơn localhost)
    options.ConnectTimeout = 10000;    // 10 giây để connect
    options.SyncTimeout = 10000;       // 10 giây cho sync ops
    options.AsyncTimeout = 10000;      // 10 giây cho async ops
    options.ConnectRetry = 5;          // Retry 5 lần nếu kết nối thất bại
    options.KeepAlive = 60;            // Giữ alive mỗi 60s
    options.AbortOnConnectFail = false; // Không abort app nếu Redis down

    options.ReconnectRetryPolicy = new LinearRetry(1000); // Retry mỗi 1s

    Log.Information("[REDIS] Connecting to Aiven Redis: {Endpoint}", options.EndPoints.FirstOrDefault());

    var multiplexer = ConnectionMultiplexer.Connect(options);

    multiplexer.ConnectionFailed += (_, e) =>
        Log.Error("[REDIS] Connection failed: {Endpoint} - {FailureType} - {Exception}",
            e.EndPoint, e.FailureType, e.Exception?.Message);

    multiplexer.ConnectionRestored += (_, e) =>
        Log.Information("[REDIS] Connection restored: {Endpoint}", e.EndPoint);

    Log.Information("[REDIS] Connected successfully: IsConnected={IsConnected}", multiplexer.IsConnected);

    return multiplexer;
});

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
        ),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Bind MomoSettings
builder.Services.Configure<MomoSettings>(builder.Configuration.GetSection("MomoDisbursement"));

// Add Repositories and Services
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AgriLink API",
        Version = "v1",
        Description = "API cho hệ thống quản lý nông nghiệp AgriLink (Rẫy Số)"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token với format: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Fix: cho phép Swashbuckle generate đúng schema cho IFormFile / multipart
    options.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure Forwarded Headers for Reverse Proxy support
// Để lấy đúng IP từ X-Forwarded-For, X-Real-IP headers
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Cho phép tất cả proxy (development). Trong production nên giới hạn KnownProxies/KnownNetworks
    KnownNetworks = { },
    KnownProxies = { }
});

app.UseSwagger();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("AgriLink API")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithOpenApiRoutePattern("/swagger/v1/swagger.json"); // hardcode path
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
// Import the Hubs namespace if needed via global using or fully qualified
app.MapHub<AgriLink_DH.Api.Hubs.BluetoothScaleHub>("/hubs/scale");

try
{
    Log.Information("✅ AgriLink API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.Information("👋 Shutting down AgriLink API...");
    Log.CloseAndFlush();
}
