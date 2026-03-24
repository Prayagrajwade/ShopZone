namespace ShopAPI.Application.DTOs
{
    public class UserDtos
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class CreateUserDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "user";
    }

    public class UpdateUserDto
    {
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
