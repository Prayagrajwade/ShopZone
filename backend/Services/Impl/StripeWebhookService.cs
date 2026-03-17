using Stripe;

namespace ShopAPI.Services.Impl
{
    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public StripeWebhookService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
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
    }
}
