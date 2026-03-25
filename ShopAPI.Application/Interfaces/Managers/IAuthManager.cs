namespace ShopAPI.Application.Interfaces.Managers;

public interface IAuthManager
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
