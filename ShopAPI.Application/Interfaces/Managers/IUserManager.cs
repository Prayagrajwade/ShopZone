namespace ShopAPI.Application.Interfaces.Managers;

public interface IUserManager
{
    Task<object> GetUsers(string? search, int page, int pageSize);
    Task CreateUser(CreateUserDto dto);
    Task UpdateUser(int id, UpdateUserDto dto);
    Task SoftDeleteUser(int id);
    Task ForceLogoutUser(int id);
}
