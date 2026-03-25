using ShopAPI.Application.DTOs;
using ShopAPI.Application.Interfaces.Managers;
using ShopAPI.Common;
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

    /// <summary>
    /// Creates a payment intent for the current user's cart.
    /// Returns the client secret and payment intent ID
    /// needed for processing the payment on the client side.
    /// </summary>
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

    /// <summary>
    /// Confirms the order after successful payment.
    /// </summary>
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

    /// <summary>
    /// Retrieves a list of orders for the current user,
    /// including order details such as products, quantities, total amount, and order status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderManager.GetOrdersByUserAsync(UserId);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves details of a specific order by its ID for the current user.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderManager.GetOrderByIdAsync(UserId, id);
        return order is null ? NotFound() : Ok(order);
    }


    /// <summary>
    /// Retrieves a list of all orders in the system for administrative purposes.
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderManager.GetAllOrdersAdminAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves order details including payment and status logs for the current user.
    /// </summary>
    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> GetOrderWithLogs(int id)
    {
        var order = await _orderManager.GetOrderWithLogsAsync(UserId, id);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Retrieves order details including payment and status logs for administrative purposes.
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpGet("admin/{id:int}/details")]
    public async Task<IActionResult> GetOrderWithLogsAdmin(int id)
    {
        var order = await _orderManager.GetOrderWithLogsAdminAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Triggers cleanup of old payment and status logs.
    /// Admin only endpoint for maintenance.
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPost("admin/cleanup-logs")]
    public async Task<IActionResult> CleanupLogs([FromQuery] int paymentLogsDays = 90, [FromQuery] int statusLogsDays = 180)
    {
        try
        {
            await _orderManager.CleanupOldLogsAsync(paymentLogsDays, statusLogsDays);
            return Ok(new { message = "Logs cleaned up successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error cleaning up logs", error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a payment intent for a "Buy Now" action,
    /// which allows the user to purchase a single product immediately without adding it to the cart.
    /// </summary>
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
