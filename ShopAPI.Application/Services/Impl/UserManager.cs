
using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Application.Services.Impl
{
    public class UserManager : IUserManager
    {
        private readonly IUserRopository _repo;

        public UserManager(IUserRopository repo)
        {
            _repo = repo;
        }
        public async Task<object> GetUsers(string? search, int page, int pageSize)
        {
            var (users, total) = await _repo.GetUsers(search, page, pageSize);

            return new
            {
                data = users.Select(u => new UserDtos
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                }),
                total
            };
        }

        public async Task CreateUser(CreateUserDto dto)
        {
            var user = new UserEntity
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            await _repo.Add(user);
        }

        public async Task UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _repo.GetById(id);
            if (user == null) throw new Exception("User not found");

            user.Name = dto.Name;
            user.Role = dto.Role;

            await _repo.Update(user);
        }

        public async Task SoftDeleteUser(int id)
        {
            var user = await _repo.GetById(id);
            if (user == null) throw new Exception("User not found");

            user.IsActive = false;
            await _repo.Update(user);
        }

        public async Task ForceLogoutUser(int id)
        {
            var user = await _repo.GetById(id);
            if (user == null) throw new Exception("User not found");

            user.SecurityStamp = Guid.NewGuid().ToString();
            await _repo.Update(user);
        }
    }
}
