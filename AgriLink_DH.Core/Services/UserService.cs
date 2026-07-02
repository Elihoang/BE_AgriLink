using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.User;
using BCrypt.Net;

namespace AgriLink_DH.Core.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserLoginLogRepository _loginLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserValidator _validator;

    public UserService(
        IUserRepository userRepository,
        IUserLoginLogRepository loginLogRepository,
        IUnitOfWork unitOfWork,
        UserValidator validator)
    {
        _userRepository = userRepository;
        _loginLogRepository = loginLogRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.OrderByDescending(u => u.CreatedAt).Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        await _validator.ValidateCreateUserAsync(dto);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email ?? string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName ?? string.Empty,
            PhoneNumber = dto.PhoneNumber,
            Role = dto.Role,
            ImageUrl = dto.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        await _validator.ValidateUpdateUserAsync(user, dto, id);

        // Update email if changed
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user!.Email)
        {
            user.Email = dto.Email;
        }

        user!.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.ImageUrl = dto.ImageUrl;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        _validator.ValidateChangePassword(user, currentPassword, userId);

        user!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        _validator.ValidateToggleActiveStatus(user, userId);

        user!.IsActive = isActive;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task LogLoginAttemptAsync(Guid userId, bool isSuccess, string ipAddress, string? deviceInfo, LoginActionType actionType = LoginActionType.Login)
    {
        var log = new UserLoginLog
        {
            UserId = userId,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            LoginTime = DateTime.UtcNow,
            IsSuccess = isSuccess,
            ActionType = actionType
        };

        await _loginLogRepository.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserLoginLogDto>> GetLoginHistoryAsync(Guid userId, int count = 10)
    {
        var logs = await _loginLogRepository.GetRecentLoginsAsync(userId, count);
        return logs.Select(MapLoginLogToDto);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Role = user.Role,
            RoleLabel = user.Role.ToString(),
            IsActive = user.IsActive,
            ImageUrl = user.ImageUrl,
            CreatedAt = user.CreatedAt
        };
    }

    private static UserLoginLogDto MapLoginLogToDto(UserLoginLog log)
    {
        return new UserLoginLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            IpAddress = log.IpAddress,
            DeviceInfo = log.DeviceInfo,
            LoginTime = log.LoginTime,
            IsSuccess = log.IsSuccess,
            ActionType = log.ActionType.ToString()
        };
    }
}
