using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Data;
using ShopAPI.Interfaces;
using ShopAPI.Models;
using Stripe;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[DisableCors]
public class WebhookController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IStripeWebhookService _stripeWebhookService;

    public WebhookController(AppDbContext db, IConfiguration config, IStripeWebhookService stripeWebhookService)
    {
        _db = db;
        _config = config;
        _stripeWebhookService = stripeWebhookService;
    }

    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        //var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        //var webhookSecret = _config["Stripe:WebhookSecret"]!;

        //try
        //{
        //    var stripeEvent = EventUtility.ConstructEvent(
        //        json,
        //        Request.Headers["Stripe-Signature"],
        //        webhookSecret,
        //        throwOnApiVersionMismatch: false
        //    );

        //    if (stripeEvent.Type == "payment_intent.succeeded")
        //    {
        //        var intent = stripeEvent.Data.Object as PaymentIntent;
        //        if (intent is null) return BadRequest();

        //        var order = _db.Orders
        //            .FirstOrDefault(o => o.StripePaymentIntentId == intent.Id);

        //        if (order is not null)
        //        {
        //            var oldStatus = order.Status;
        //            order.Status = "on_the_way";
        //            _db.PaymentLogs.Add(new PaymentLog
        //            {
        //                StripeEventId = stripeEvent.Id,
        //                EventType = stripeEvent.Type,
        //                PaymentIntentId = intent.Id,
        //                Status = intent.Status,
        //                RawJson = json
        //            });

        //            _db.OrderStatusLogs.Add(new OrderStatusLog
        //            {
        //                OrderId = order.Id,
        //                OldStatus = oldStatus,
        //                NewStatus = order.Status,
        //                Note = "Payment succeeded via Stripe"
        //            });
        //            await _db.SaveChangesAsync();
        //        }
        //    }

        //    if (stripeEvent.Type == "payment_intent.payment_failed")
        //    {
        //        var intent = stripeEvent.Data.Object as PaymentIntent;
        //        if (intent is null) return BadRequest();

        //        var order = _db.Orders
        //            .FirstOrDefault(o => o.StripePaymentIntentId == intent.Id);

        //        if (order is not null)
        //        {
        //            var oldStatus = order.Status;
        //            order.Status = "payment_failed";

        //            _db.OrderStatusLogs.Add(new OrderStatusLog
        //            {
        //                OrderId = order.Id,
        //                OldStatus = oldStatus,
        //                NewStatus = order.Status,
        //                Note = "Payment failed via Stripe"
        //            });

        //            await _db.SaveChangesAsync();
        //        }
        //    }

        //    return Ok();
        //}
        //catch (StripeException ex)
        //{
        //    return BadRequest(new { message = ex.Message });
        //}
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            await _stripeWebhookService.HandleEventAsync(json, Request.Headers["Stripe-Signature"]);
            return Ok();
        }
        catch (StripeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

    }
}