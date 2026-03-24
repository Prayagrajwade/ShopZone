using Microsoft.Extensions.Logging;
using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Common;
using ShopAPI.Interfaces.Repository;
using ShopAPI.Services;

namespace ShopAPI.Application.Services.Impl;

public class AuthManager : IAuthManager
{
    private readonly IUserRopository _userRepository;
    private readonly JwtService _jwt;
    private readonly ILogger<AuthManager> _logger;

    public AuthManager(IUserRopository userRepository, JwtService jwt, ILogger<AuthManager> logger)
    {
        _userRepository = userRepository;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        try
        {
            _logger.LogInformation("User registration attempt for email: {Email}", dto.Email);

            var emailExists = await _userRepository.UserExistsByEmailAsync(dto.Email);
            if (emailExists)
            {
                _logger.LogWarning("Registration failed: Email already exists: {Email}", dto.Email);
                throw new InvalidOperationException("Email already exists.");
            }

            var user = new UserEntity
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = AppConstants.Roles.User
            };

            await _userRepository.CreateUserAsync(user);
            _logger.LogInformation("User registered successfully with ID: {UserId}, Email: {Email}", user.Id, user.Email);

            var token = _jwt.GenerateToken(user);
            _logger.LogInformation("JWT token generated for new user {UserId}", user.Id);

            return new AuthResponseDto(token, user.Role, user.Name, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", dto.Email);
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        try
        {
            _logger.LogInformation("User login attempt for email: {Email}", dto.Email);

            var user = await _userRepository.GetUserByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid credentials for email: {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var token = _jwt.GenerateToken(user);
            _logger.LogInformation("User logged in successfully: {UserId}, Email: {Email}", user.Id, user.Email);

            return new AuthResponseDto(token, user.Role, user.Name, user.Id);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized login attempt for email: {Email}", dto.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", dto.Email);
            throw;
        }
    }
}
