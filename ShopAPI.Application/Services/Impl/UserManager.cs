
using Microsoft.Extensions.Logging;
using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Application.Services.Impl
{
    public class UserManager : IUserManager
    {
        private readonly IUserRopository _repo;
        private readonly ILogger<UserManager> _logger;

        public UserManager(IUserRopository repo, ILogger<UserManager> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<object> GetUsers(string? search, int page, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching users with search: {Search}, page: {Page}, pageSize: {PageSize}", 
                    search ?? "none", page, pageSize);

                var (users, total) = await _repo.GetUsers(search, page, pageSize);

                _logger.LogInformation("Retrieved {UserCount} users, Total: {Total}", users.Count(), total);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                throw;
            }
        }

        public async Task CreateUser(CreateUserDto dto)
        {
            try
            {
                _logger.LogInformation("Creating new user with Email: {Email}, Role: {Role}", dto.Email, dto.Role);

                var user = new UserEntity
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = dto.Role,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                await _repo.Add(user);
                _logger.LogInformation("User created successfully: UserId: {UserId}, Email: {Email}", user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with Email: {Email}", dto.Email);
                throw;
            }
        }

        public async Task UpdateUser(int id, UpdateUserDto dto)
        {
            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", id);

                var user = await _repo.GetById(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: UserId: {UserId}", id);
                    throw new Exception("User not found");
                }

                user.Name = dto.Name;
                user.Role = dto.Role;

                await _repo.Update(user);
                _logger.LogInformation("User updated successfully: UserId: {UserId}, NewRole: {NewRole}", id, dto.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task SoftDeleteUser(int id)
        {
            try
            {
                _logger.LogInformation("Soft deleting user with ID: {UserId}", id);

                var user = await _repo.GetById(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found for soft delete: UserId: {UserId}", id);
                    throw new Exception("User not found");
                }

                user.IsActive = false;
                await _repo.Update(user);
                _logger.LogInformation("User soft deleted successfully: UserId: {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task ForceLogoutUser(int id)
        {
            try
            {
                _logger.LogInformation("Force logging out user with ID: {UserId}", id);

                var user = await _repo.GetById(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found for force logout: UserId: {UserId}", id);
                    throw new Exception("User not found");
                }

                user.SecurityStamp = Guid.NewGuid().ToString();
                await _repo.Update(user);
                _logger.LogInformation("User force logged out successfully: UserId: {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force logging out user with ID: {UserId}", id);
                throw;
            }
        }
    }
}
