using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<StripeWebhookManager> _logger;

    public StripeWebhookManager(IOrderRepository orderRepository, IConfiguration config,
        IEmailTemplateRepository templateRepo,
        IEmailService emailService,
        IUserRopository userRepository,
        ILogger<StripeWebhookManager> logger)
    {
        _orderRepository = orderRepository;
        _config = config;
        _templateRepo = templateRepo;
        _emailService = emailService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task HandleEventAsync(string json, string signatureHeader)
    {
        try
        {
            _logger.LogInformation("Processing Stripe webhook event");

            var webhookSecret = _config["Stripe:WebhookSecret"]!;

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                webhookSecret,
                throwOnApiVersionMismatch: false
            );

            _logger.LogInformation("Stripe event type: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case AppConstants.PaymentEvents.Succeeded:
                    _logger.LogInformation("Handling payment succeeded event");
                    await HandlePaymentSucceeded(stripeEvent, json);
                    break;

                case AppConstants.PaymentEvents.Failed:
                    _logger.LogInformation("Handling payment failed event");
                    await HandlePaymentFailed(stripeEvent);
                    break;

                case "payment_intent.canceled":
                    _logger.LogInformation("Handling payment canceled event (abandoned order)");
                    await HandlePaymentCanceled(stripeEvent);
                    break;

                default:
                    _logger.LogWarning("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook event");
            throw;
        }
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent, string json)
    {
        try
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent is null)
            {
                _logger.LogWarning("PaymentIntent is null in succeeded event");
                return;
            }

            _logger.LogInformation("Processing payment succeeded for PaymentIntentId: {PaymentIntentId}", intent.Id);

            var order = await _orderRepository.GetOrderByStripeIdAsync(intent.Id);

            if (order is null)
            {
                _logger.LogError("CRITICAL: Order not found for PaymentIntentId: {PaymentIntentId}. " +
                    "Order should have been created when PaymentIntent was created.", intent.Id);
                return;
            }

            _logger.LogInformation("Found order {OrderId} for PaymentIntentId: {PaymentIntentId}", order.Id, intent.Id);

            if (!string.IsNullOrEmpty(order.WebhookEventId) && order.WebhookEventId == stripeEvent.Id)
            {
                _logger.LogInformation("Webhook already processed for this order: OrderId: {OrderId}, EventId: {EventId}. Skipping.", 
                    order.Id, stripeEvent.Id);
                return;
            }

            if (order.StockReserved)
            {
                _logger.LogInformation("Stock already reserved for order: OrderId: {OrderId}. Updating webhook tracking only.", order.Id);
                order.WebhookEventId = stripeEvent.Id;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateOrderAsync(order);
                return;
            }

            foreach (var item in order.Items)
            {
                var product = await _orderRepository.GetProductAsync(item.ProductId);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found for order item", item.ProductId);
                    continue;
                }

                if (product.Stock < item.Quantity)
                {
                    _logger.LogError("Not enough stock for product {ProductName} (Stock: {Stock}, Required: {Quantity})", 
                        product.Name, product.Stock, item.Quantity);
                    throw new Exception($"Not enough stock for product {product.Name}");
                }

                product.Stock -= item.Quantity;
                _logger.LogInformation("Updated stock for product {ProductName}, New Stock: {NewStock}", 
                    product.Name, product.Stock);
            }

            var oldStatus = order.Status;
            order.Status = AppConstants.OrderStatus.OnTheWay;
            order.StockReserved = true;
            order.WebhookEventId = stripeEvent.Id;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateOrderAsync(order);
            _logger.LogInformation("Updated order {OrderId} status from {OldStatus} to {NewStatus}. Stock reserved: true, WebhookEventId: {EventId}", 
                order.Id, oldStatus, order.Status, stripeEvent.Id);

            var template = await _templateRepo.GetByTypeAsync(AppConstants.TemeplateType.Invoice);
            if (template is null)
            {
                _logger.LogWarning("Email template not found for type: {TemplateType}", AppConstants.TemeplateType.Invoice);
                return;
            }

            _logger.LogInformation("Email template found for order {OrderId}", order.Id);

            var userDetails = await _userRepository.GetUSerDetailsAsync(order.UserId);
            _logger.LogInformation("Retrieved user details for UserId: {UserId}", order.UserId);

            var productRows = "";

            foreach (var item in order.Items)
            {
                var product = await _orderRepository.GetProductAsync(item.ProductId);

                if (product == null) continue;

                var total = item.Quantity * product.Price;

                productRows += $@"
                <tr>
                    <td>{product.Name}</td>
                    <td>₹{product.Price}</td>
                    <td>{item.Quantity}</td>
                    <td>₹{total}</td>
                </tr>";
            }

            var body = EmailTemplateHelper.Replace(template.Body, new Dictionary<string, string>
            {
                ["UserName"] = userDetails.Name,
                ["OrderId"] = order.Id.ToString(),
                ["Amount"] = order.TotalAmount.ToString(),
                ["ProductRows"] = productRows
            });

            var subject = EmailTemplateHelper.Replace(template.Subject, new Dictionary<string, string>
            {
                ["OrderId"] = order.Id.ToString()
            });

            var invoiceBytes = GenerateInvoice(order);
            _logger.LogInformation("Generated invoice PDF for order {OrderId}, Size: {InvoiceSize} bytes", 
                order.Id, invoiceBytes.Length);

            var recipientEmail = "test@yopmail.com"; //userDetails?.Email ??

            try
            {
                _logger.LogInformation("Sending invoice email to {RecipientEmail} for order {OrderId}", 
                    recipientEmail, order.Id);

                await _emailService.SendEmailAsync(
                    recipientEmail,
                    subject,
                    body,
                    invoiceBytes,
                    $"Invoice_{order.Id}.pdf"
                );

                _logger.LogInformation("Invoice email sent successfully to {RecipientEmail} for order {OrderId}", 
                    recipientEmail, order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice email to {RecipientEmail} for order {OrderId}", 
                    recipientEmail, order.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandlePaymentSucceeded");
            throw;
        }
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        try
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent is null)
            {
                _logger.LogWarning("PaymentIntent is null in failed event");
                return;
            }

            _logger.LogInformation("Processing payment failed for PaymentIntentId: {PaymentIntentId}", intent.Id);

            var order = await _orderRepository.GetOrderByStripeIdAsync(intent.Id);
            if (order is null)
            {
                _logger.LogWarning("Order not found for failed PaymentIntentId: {PaymentIntentId}", intent.Id);
                return;
            }

            var oldStatus = order.Status;
            order.Status = AppConstants.OrderStatus.PaymentFailed;

            await _orderRepository.UpdateOrderAsync(order);
            _logger.LogInformation("Updated order {OrderId} status to PaymentFailed (previous status: {OldStatus})", 
                order.Id, oldStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandlePaymentFailed");
            throw;
        }
    }

    private async Task HandlePaymentCanceled(Event stripeEvent)
    {
        try
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent is null)
            {
                _logger.LogWarning("PaymentIntent is null in canceled event");
                return;
            }

            _logger.LogInformation("Processing payment canceled for PaymentIntentId: {PaymentIntentId}", intent.Id);

            var order = await _orderRepository.GetOrderByStripeIdAsync(intent.Id);
            if (order is null)
            {
                _logger.LogWarning("Order not found for canceled PaymentIntentId: {PaymentIntentId}", intent.Id);
                return;
            }

            var oldStatus = order.Status;

            if (order.Status == "pending")
            {
                order.Status = "abandoned";
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateOrderAsync(order);
                _logger.LogInformation("Updated order {OrderId} status to abandoned (previous status: {OldStatus}). " +
                    "User canceled payment.", order.Id, oldStatus);
            }
            else
            {
                _logger.LogWarning("Order {OrderId} already has status {Status}, not marking as abandoned", 
                    order.Id, order.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandlePaymentCanceled");
            throw;
        }
    }

    public byte[] GenerateInvoice(OrderEntity order)
    {
        using var ms = new MemoryStream();
        var writer = new iText.Kernel.Pdf.PdfWriter(ms);
        var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
        var doc = new iText.Layout.Document(pdf);

        doc.Add(new iText.Layout.Element.Paragraph($"Invoice #{order.Id}")
            .SetFontSize(18));

        doc.Add(new iText.Layout.Element.Paragraph($"Total Amount: ₹{order.TotalAmount}"));
        doc.Add(new iText.Layout.Element.Paragraph("\n"));

        var table = new iText.Layout.Element.Table(4).UseAllAvailableWidth();

        table.AddHeaderCell("Product");
        table.AddHeaderCell("Price");
        table.AddHeaderCell("Quantity");
        table.AddHeaderCell("Total");

        foreach (var item in order.Items)
        {
            var product = item.Product;

            if (product == null) continue;

            var total = item.Quantity * product.Price;

            table.AddCell(product.Name);
            table.AddCell($"₹{product.Price}");
            table.AddCell(item.Quantity.ToString());
            table.AddCell($"₹{total}");
        }

        doc.Add(table);

        doc.Add(new iText.Layout.Element.Paragraph("\n"));
        doc.Add(new iText.Layout.Element.Paragraph($"Grand Total: ₹{order.TotalAmount}"));

        doc.Close();
        return ms.ToArray();
    }
}
