using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.User;
using BCrypt.Net;

namespace AgriLink_DH.Core.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserLoginLogRepository _loginLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        IUserRepository userRepository,
        IUserLoginLogRepository loginLogRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _loginLogRepository = loginLogRepository;
        _unitOfWork = unitOfWork;
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
        // Check username exists
        if (await _userRepository.ExistsByUsernameAsync(dto.Username))
        {
            throw new InvalidOperationException($"Tên đăng nhập '{dto.Username}' đã tồn tại");
        }

        // Check email exists
        if (!string.IsNullOrEmpty(dto.Email) && await _userRepository.ExistsByEmailAsync(dto.Email))
        {
            throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng");
        }

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
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
        if (user == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {id}");
        }

        // Check email if changed
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng");
            }
            user.Email = dto.Email;
        }

        user.FullName = dto.FullName;
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
        if (user == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Mật khẩu hiện tại không đúng");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");
        }

        user.IsActive = isActive;
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
            Email = user.Email,
            FullName = user.FullName,
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
