using Microsoft.AspNetCore.Cors;
using Stripe;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[DisableCors]
public class WebhookController : ControllerBase
{
    private readonly IStripeWebhookManager _stripeWebhookManager;

    public WebhookController(IStripeWebhookManager stripeWebhookManager)
    {
        _stripeWebhookManager = stripeWebhookManager;
    }

    /// <summary>
    /// Handles incoming Stripe webhook events. The request body contains the event data in JSON format,
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            await _stripeWebhookManager.HandleEventAsync(json, Request.Headers["Stripe-Signature"]);
            return Ok();
        }
        catch (StripeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

    }
}