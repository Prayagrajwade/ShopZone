using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopAPI.Common.Email;
using ShopAPI.Interfaces.Email;
using ShopAPI.Interfaces.Repository;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, IEmailTemplateRepository emailTemplateRepository, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _templateRepository = emailTemplateRepository;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Sending email to {RecipientEmail} with subject: {Subject}", to, subject);

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
            _logger.LogInformation("Email sent successfully to {RecipientEmail}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {RecipientEmail} with subject: {Subject}", to, subject);
            throw;
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? fileName = null)
    {
        try
        {
            _logger.LogInformation("Sending email to {RecipientEmail} with subject: {Subject} and attachment: {FileName}", 
                to, subject, fileName ?? "none");

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
                _logger.LogInformation("Attachment added: {FileName} ({AttachmentSize} bytes)", fileName, attachment.Length);
            }

           await smtp.SendMailAsync(message);
            _logger.LogInformation("Email with attachment sent successfully to {RecipientEmail}", to);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {RecipientEmail} with subject: {Subject} and attachment: {FileName}", 
                to, subject, fileName ?? "none");
            throw;
        }
    }

    #region Seeder
    public async Task SeedInvoiceTemplateAsync()
    {
        var exists = await _templateRepository.ExistsAsync("Invoice");

        if (exists)
            return;

        var template = new TempleteEntity
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