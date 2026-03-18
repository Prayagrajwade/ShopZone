namespace ShopAPI.Interfaces.Repository
{
    public interface IUserRopository
    {
        Task<UserDto> GetUSerDetailsAsync(int userId);
    }
}
