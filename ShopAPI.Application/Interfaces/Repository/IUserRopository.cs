namespace ShopAPI.Interfaces.Repository
{
    public interface IUserRopository
    {
        Task<UserDto> GetUSerDetailsAsync(int userId);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<UserEntity> CreateUserAsync(UserEntity user);
        Task<UserEntity?> GetUserByEmailAsync(string email);
    }
}
