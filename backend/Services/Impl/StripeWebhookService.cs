using iText.Kernel.Pdf;
using ShopAPI.Interfaces.Repository;
using Stripe;

namespace ShopAPI.Services.Impl
{
    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailTemplateRepository _templateRepo;
        private readonly IEmailService _emailService;
        private readonly IUserRopository _userRopository;

        public StripeWebhookService(AppDbContext db, IConfiguration config,
            IEmailTemplateRepository templateRepo,
            IEmailService emailService,
            IUserRopository userRopository)
        {
            _db = db;
            _config = config;
            _templateRepo = templateRepo;
            _emailService = emailService;
            _userRopository = userRopository;
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


            var alreadyProcessed = await _db.PaymentLogs
                                  .AnyAsync(x => x.StripeEventId == stripeEvent.Id);

            if (alreadyProcessed)
            {
                return;
            }

            var order = _db.Orders
                .FirstOrDefault(o => o.StripePaymentIntentId == intent.Id);

            if (order is null) return;

            var oldStatus = order.Status;
            order.Status = AppConstants.OrderStatus.OnTheWay;

            _db.PaymentLogs.Add(new PaymentLog
            {
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                PaymentIntentId = intent.Id,
                Status = intent.Status,
                RawJson = json
            });

            _db.OrderStatusLogs.Add(new OrderStatusLog
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = order.Status,
                Note = AppConstants.Notes.PaymentSucceeded
            });

            await _db.SaveChangesAsync();

            var template = await _templateRepo.GetByTypeAsync(AppConstants.TemeplateType.Invoice);
            if (template is null) return;

            var userDetails = await _userRopository.GetUSerDetailsAsync(order.UserId);

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

            await _emailService.SendEmailAsync(
                "test@yopmail.com",
                subject,
                body,
                invoiceBytes,
                $"Invoice_{order.Id}.pdf"
            );
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent is null) return;

            var order = _db.Orders
                .FirstOrDefault(o => o.StripePaymentIntentId == intent.Id);

            if (order is null) return;

            var oldStatus = order.Status;
            order.Status = AppConstants.OrderStatus.PaymentFailed;

            _db.OrderStatusLogs.Add(new OrderStatusLog
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = order.Status,
                Note = AppConstants.Notes.PaymentFailed
            });

            await _db.SaveChangesAsync();
        }

 

    #region Invoice
        public byte[] GenerateInvoice(Order order)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            doc.Add(new iText.Layout.Element.Paragraph($"Invoice #{order.Id}"));
            doc.Add(new iText.Layout.Element.Paragraph($"Amount: ₹{order.TotalAmount}"));

            doc.Close();
            return ms.ToArray();
        }
    }
    #endregion
}
