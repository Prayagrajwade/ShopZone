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