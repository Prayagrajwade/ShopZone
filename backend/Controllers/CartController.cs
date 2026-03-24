using ShopAPI.Application.DTOs;
using ShopAPI.Common;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartManager _cartManager;

    public CartController(ICartManager cartManager) => _cartManager = cartManager;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Retrieves the current user's shopping cart items, including product details and quantities.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var items = await _cartManager.GetCartAsync(UserId);
        return Ok(items);
    }

    /// <summary>
    /// Adds a product to the current user's shopping cart. Validates that the product exists and is active before adding.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartDto dto)
    {
        try
        {
            var success = await _cartManager.AddToCartAsync(UserId, dto);

            return success
                ? Ok(new { message = "Added to cart." })
                : BadRequest(new { message = "Product not found or inactive." });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the quantity of a specific product in the current user's shopping cart.
    /// Validates that the product exists, is active,
    /// and that the requested quantity does not exceed available stock before updating.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int quantity)
    {
        try
        {
            var success = await _cartManager.UpdateQuantityAsync(UserId, id, quantity);
            return success ? Ok(new { message = "Cart updated." }) : NotFound();
        }catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

    }

    /// <summary>
    /// Removes a specific product from the current user's shopping cart.
    /// Validates that the product exists in the cart before attempting to remove it.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var success = await _cartManager.RemoveItemAsync(UserId, id);
        return success ? Ok(new { message = "Item removed." }) : NotFound();
    }

    /// <summary>
    /// Clears all items from the current user's shopping cart.
    /// Validates that the cart is not already empty before attempting to clear it.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartManager.ClearCartAsync(UserId);
        return Ok(new { message = "Cart cleared." });
    }

    /// <summary>
    /// Merges a list of items into the current user's shopping cart.
    /// This is typically used when a user logs in and
    /// has items in a temporary cart (e.g., from a guest session).
    /// </summary>
    [HttpPost("merge")]
    public async Task<IActionResult> MergeCart(List<AddToCartDto> items)
    {
        await _cartManager.MergeCartAsync(UserId, items);
        return Ok();
    }
}
