namespace ShopAPI.Interfaces.Repository
{
    public interface IUserRopository
    {
        Task<UserDto> GetUSerDetailsAsync(int userId);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<UserEntity> CreateUserAsync(UserEntity user);
        Task<UserEntity?> GetUserByEmailAsync(string email);
        Task<(List<UserEntity>, int totalCount)> GetUsers(string? search, int page, int pageSize);
        Task<UserEntity?> GetById(int id);
        Task Add(UserEntity user);
        Task Update(UserEntity user);
    }
}
