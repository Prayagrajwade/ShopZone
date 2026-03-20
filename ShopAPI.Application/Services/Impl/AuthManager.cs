using BCrypt.Net;
using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Common;
using ShopAPI.Interfaces.Repository;
using ShopAPI.Services;

namespace ShopAPI.Application.Services.Impl;

public class AuthManager : IAuthManager
{
    private readonly IUserRopository _userRepository;
    private readonly JwtService _jwt;

    public AuthManager(IUserRopository userRepository, JwtService jwt)
    {
        _userRepository = userRepository;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var emailExists = await _userRepository.UserExistsByEmailAsync(dto.Email);
        if (emailExists)
            throw new InvalidOperationException("Email already exists.");

        var user = new UserEntity
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = AppConstants.Roles.User
        };

        await _userRepository.CreateUserAsync(user);

        var token = _jwt.GenerateToken(user);
        return new AuthResponseDto(token, user.Role, user.Name, user.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _jwt.GenerateToken(user);
        return new AuthResponseDto(token, user.Role, user.Name, user.Id);
    }
}
