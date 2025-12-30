using System.Security.Claims;
using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Core.Helpers;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Auth;
using Microsoft.Extensions.Configuration;

namespace AgriLink_DH.Core.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtHelper _jwtHelper;
    private readonly RedisService _redisService;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        JwtHelper jwtHelper,
        RedisService redisService,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtHelper = jwtHelper;
        _redisService = redisService;
        _configuration = configuration;
        _context = context;
    }

    public async Task<(bool Success, string Message, UserResponseDto? User, TokenDto? Token)> RegisterAsync(
        RegisterDto registerDto,
        string? ipAddress = null,
        string? deviceInfo = null)
    {
        // Check if username exists
        if (await _userRepository.ExistsByUsernameAsync(registerDto.Username))
        {
            return (false, "Username already exists", null, null);
        }

        // Check if email exists
        if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
        {
            return (false, "Email already exists", null, null);
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create user
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            FullName = registerDto.FullName,
            PhoneNumber = registerDto.PhoneNumber,
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var tokens = _jwtHelper.GenerateTokenPair(user);

        // Store refresh token in Redis
        var refreshTokenExpiration = TimeSpan.FromDays(
            Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")
        );
        await _redisService.SetRefreshTokenAsync(user.Id.ToString(), tokens.RefreshToken, refreshTokenExpiration);

        // Log registration
        await CreateLoginLogAsync(user.Id, ipAddress ?? "Unknown", deviceInfo ?? "Unknown", LoginActionType.Register, true);

        // Map to response DTO
        var userResponse = MapToUserResponseDto(user);

        return (true, "Registration successful", userResponse, tokens);
    }

    public async Task<(bool Success, string Message, UserResponseDto? User, TokenDto? Token)> LoginAsync(
        LoginDto loginDto, 
        string? ipAddress = null, 
        string? deviceInfo = null)
    {
        // Find user by username or email
        var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail);

        if (user == null)
        {
            // Log failed login attempt if we have userId
            return (false, "Invalid username/email or password", null, null);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            // Log failed login - account inactive
            await CreateLoginLogAsync(user.Id, ipAddress ?? "Unknown", deviceInfo ?? "Unknown", LoginActionType.Login, false);
            return (false, "Account is inactive", null, null);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            // Log failed login - wrong password
            await CreateLoginLogAsync(user.Id, ipAddress ?? "Unknown", deviceInfo ?? "Unknown", LoginActionType.Login, false);
            return (false, "Invalid username/email or password", null, null);
        }

        // Generate tokens
        var tokens = _jwtHelper.GenerateTokenPair(user);

        // Store refresh token in Redis
        var refreshTokenExpiration = TimeSpan.FromDays(
            Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")
        );
        await _redisService.SetRefreshTokenAsync(user.Id.ToString(), tokens.RefreshToken, refreshTokenExpiration);

        // Log successful login
        await CreateLoginLogAsync(user.Id, ipAddress ?? "Unknown", deviceInfo ?? "Unknown", LoginActionType.Login, true);

        // Map to response DTO
        var userResponse = MapToUserResponseDto(user);

        return (true, "Login successful", userResponse, tokens);
    }

    public Task<(bool Success, string Message, TokenDto? Token)> RefreshTokenAsync(string refreshToken)
    {
        // Note: We need to get userId from the old access token
        // In a real scenario, the client should send both tokens
        // For now, we'll assume the refresh token validation
        
        // This is a simplified version - in production you should:
        // 1. Extract userId from the expired access token
        // 2. Verify the refresh token from Redis matches
        // 3. Generate new token pair
        
        return Task.FromResult<(bool Success, string Message, TokenDto? Token)>((false, "Invalid refresh token", null));
    }

    public async Task<bool> LogoutAsync(string userId, string? ipAddress = null, string? deviceInfo = null)
    {
        // Log logout action
        if (Guid.TryParse(userId, out var userGuid))
        {
            await CreateLoginLogAsync(userGuid, ipAddress ?? "Unknown", deviceInfo ?? "Unknown", LoginActionType.Logout, true);
        }

        // Delete refresh token from Redis
        return await _redisService.DeleteRefreshTokenAsync(userId);
    }

    private UserResponseDto MapToUserResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Tạo login log để tracking lịch sử đăng nhập
    /// </summary>
    public async Task CreateLoginLogAsync(
        Guid userId, 
        string ipAddress, 
        string deviceInfo,
        LoginActionType actionType = LoginActionType.Login,
        bool isSuccess = true)
    {
        var loginLog = new UserLoginLog
        {
            UserId = userId,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            LoginTime = DateTime.UtcNow,
            IsSuccess = isSuccess,
            ActionType = actionType
        };

        await _context.UserLoginLogs.AddAsync(loginLog);
        await _context.SaveChangesAsync();
    }
}
