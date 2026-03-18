using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Repository
{
    public class UserRopository : IUserRopository
    {
        private readonly AppDbContext _db;

        public UserRopository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<UserDto> GetUSerDetailsAsync(int userId)
        {
            var user = await _db.Users
                        .Where(x => x.Id == userId)
                        .Select(x => new UserDto(x.Name))
                        .FirstAsync();

            return user;
        }
    }
}
