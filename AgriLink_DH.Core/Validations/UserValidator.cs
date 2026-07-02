using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.User;

namespace AgriLink_DH.Core.Validations;

public class UserValidator
{
    private readonly IUserRepository _userRepository;

    public UserValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ValidateCreateUserAsync(CreateUserDto dto)
    {
        if (await _userRepository.ExistsByUsernameAsync(dto.Username))
        {
            throw new InvalidOperationException($"Tên đăng nhập '{dto.Username}' đã tồn tại");
        }

        if (!string.IsNullOrEmpty(dto.Email) && await _userRepository.ExistsByEmailAsync(dto.Email))
        {
            throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng");
        }
    }

    public async Task ValidateUpdateUserAsync(User? user, UpdateUserDto dto, Guid id)
    {
        if (user == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {id}");
        }

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng");
            }
        }
    }

    public void ValidateChangePassword(User? user, string currentPassword, Guid userId)
    {
        if (user == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Mật khẩu hiện tại không đúng");
        }
    }

    public void ValidateToggleActiveStatus(User? user, Guid userId)
    {
        if (user == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");
        }
    }
}
