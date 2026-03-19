using Microsoft.Extensions.Options;
using ShopAPI.Interfaces.Repository;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IEmailTemplateRepository _templateRepository;

    public EmailService(IOptions<EmailSettings> settings, IEmailTemplateRepository emailTemplateRepository)
    {
        _settings = settings.Value;
        _templateRepository = emailTemplateRepository;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtp = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password
            ),
            EnableSsl = true
        };

        var message = new MailMessage(
            _settings.From,
            to,
            subject,
            body
        )
        {
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(message);
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? fileName = null)
    {
        try
        {
            var smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password
            ),
                EnableSsl = true
            };

            var message = new MailMessage(_settings.From, to, subject, body)
            {
                IsBodyHtml = true
            };

            if (attachment != null && fileName != null)
            {
                message.Attachments.Add(
                    new Attachment(new MemoryStream(attachment), fileName)
                );
            }

           await smtp.SendMailAsync(message);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Email failed: " + ex.Message);
            throw;
        }
    }

    #region Seeder
    public async Task SeedInvoiceTemplateAsync()
    {
        var exists = await _templateRepository.ExistsAsync("Invoice");

        if (exists)
            return;

        var template = new TempleteModel
        {
            Type = "Invoice",
            Subject = "Invoice - Order {{OrderId}}",
            Body = @"
                <h2>Order Invoice</h2>
                <p>Hello {{UserName}},</p>
                <p>Your order <b>#{{OrderId}}</b> is confirmed.</p>
                <p>Total: ₹{{Amount}}</p>
                <p>Thanks for shopping!</p>
            "
        };

        await _templateRepository.AddAsync(template);
    }
    #endregion
}