using ShopAPI.Application.DTOs;
using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderManager _orderManager;

    public OrdersController(IOrderManager orderManager) => _orderManager = orderManager;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("create-payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent()
    {
        try
        {
            var result = await _orderManager.CreatePaymentIntentAsync(UserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmOrder([FromBody] string paymentIntentId)
    {
        try
        {
            var result = await _orderManager.ConfirmOrderAsync(UserId, paymentIntentId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderManager.GetOrdersByUserAsync(UserId);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderManager.GetOrderByIdAsync(UserId, id);
        return order is null ? NotFound() : Ok(order);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderManager.GetAllOrdersAdminAsync();
        return Ok(orders);
    }

    [HttpPost("buy-now")]
    public async Task<IActionResult> BuyNow([FromBody] BuyNowDto dto)
    {
        try
        {
            var result = await _orderManager.CreateBuyNowPaymentIntentAsync(UserId, dto);
            return Ok(result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Something went wrong." });
        }
    }
}
