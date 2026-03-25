namespace ShopAPI.Interfaces.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? fileName = null);
    }
}
