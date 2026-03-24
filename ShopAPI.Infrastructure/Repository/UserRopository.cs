
namespace ShopAPI.Infrastructure.Repository
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
                        .Select(x => new UserDto(x.Name, x.Email))
                        .FirstAsync();

            return user;
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<(List<UserEntity>, int)> GetUsers(string? search, int page, int pageSize)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<UserEntity?> GetById(int id)
            => await _db.Users.FindAsync(id);

        public async Task Add(UserEntity user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task Update(UserEntity user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
