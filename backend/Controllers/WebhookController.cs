using Microsoft.AspNetCore.Cors;
using ShopAPI.Application.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[DisableCors]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IStripeWebhookManager _stripeWebhookManager;

    public WebhookController(IConfiguration config, IStripeWebhookManager stripeWebhookManager)
    {
        _config = config;
        _stripeWebhookManager = stripeWebhookManager;
    }

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