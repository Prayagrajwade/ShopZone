using Microsoft.Extensions.Configuration;
using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Common;
using ShopAPI.Common.Email;
using ShopAPI.Interfaces.Email;
using ShopAPI.Interfaces.Repository;
using Stripe;

namespace ShopAPI.Application.Services.Impl;

public class StripeWebhookManager : IStripeWebhookManager
{
    private readonly IOrderRepository _orderRepository;
    private readonly IConfiguration _config;
    private readonly IEmailTemplateRepository _templateRepo;
    private readonly IEmailService _emailService;
    private readonly IUserRopository _userRepository;

    public StripeWebhookManager(IOrderRepository orderRepository, IConfiguration config,
        IEmailTemplateRepository templateRepo,
        IEmailService emailService,
        IUserRopository userRepository)
    {
        _orderRepository = orderRepository;
        _config = config;
        _templateRepo = templateRepo;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task HandleEventAsync(string json, string signatureHeader)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"]!;

        var stripeEvent = EventUtility.ConstructEvent(
            json,
            signatureHeader,
            webhookSecret,
            throwOnApiVersionMismatch: false
        );

        switch (stripeEvent.Type)
        {
            case AppConstants.PaymentEvents.Succeeded:
                await HandlePaymentSucceeded(stripeEvent, json);
                break;

            case AppConstants.PaymentEvents.Failed:
                await HandlePaymentFailed(stripeEvent);
                break;
        }
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent, string json)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var order = await _orderRepository.GetOrderByStripeIdAsync(intent.Id);

        if (order is null)
        {
            await Task.Delay(5000);
            order = await _orderRepository.GetOrderByStripeIdAsync(intent.Id);

            if (order is null)
            {
                Console.WriteLine("Order still not found after retry");
                return;
            }
        }

        foreach (var item in order.Items)
        {
            var product = await _orderRepository.GetProductAsync(item.ProductId);

            if (product == null) continue;

            if (product.Stock < item.Quantity)
            {
                throw new Exception($"Not enough stock for product {product.Name}");
            }

            product.Stock -= item.Quantity;
        }

        var oldStatus = order.Status;
        order.Status = AppConstants.OrderStatus.OnTheWay;

        await _orderRepository.UpdateOrderAsync(order);

        var template = await _templateRepo.GetByTypeAsync(AppConstants.TemeplateType.Invoice);
        if (template is null) return;

        var userDetails = await _userRepository.GetUSerDetailsAsync(order.UserId);

        var body = EmailTemplateHelper.Replace(template.Body, new Dictionary<string, string>
        {
            ["UserName"] = userDetails.Name,
            ["OrderId"] = order.Id.ToString(),
            ["Amount"] = order.TotalAmount.ToString()
        });

        var subject = EmailTemplateHelper.Replace(template.Subject, new Dictionary<string, string>
        {
            ["OrderId"] = order.Id.ToString()
        });

        var invoiceBytes = GenerateInvoice(order);

        var recipientEmail =  "test@yopmail.com"; //userDetails?.Email ??
        try
        {
            //await _emailService.SendEmailAsync(
            //    recipientEmail,
            //    subject,
            //    body,
            //    invoiceBytes,
            //    $"Invoice_{order.Id}.pdf"
            //);
        }
        catch (Exception ex)
        {
            // Log the error but don't rethrow so webhook processing can continue
            Console.WriteLine($"Failed to send email to {recipientEmail}: {ex.Message}");
        }
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var order = await _orderRepository.GetOrderByStripeIdAsync(intent.Id);
        if (order is null) return;

        var oldStatus = order.Status;
        order.Status = AppConstants.OrderStatus.PaymentFailed;

        await _orderRepository.UpdateOrderAsync(order);
    }

    public byte[] GenerateInvoice(OrderEntity order)
    {
        using var ms = new MemoryStream();
        var writer = new iText.Kernel.Pdf.PdfWriter(ms);
        var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
        var doc = new iText.Layout.Document(pdf);

        doc.Add(new iText.Layout.Element.Paragraph($"Invoice #{order.Id}"));
        doc.Add(new iText.Layout.Element.Paragraph($"Amount: ₹{order.TotalAmount}"));

        doc.Close();
        return ms.ToArray();
    }
}
