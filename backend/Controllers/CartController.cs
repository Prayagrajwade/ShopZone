using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService) => _cartService = cartService;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var items = await _cartService.GetCartAsync(UserId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartDto dto)
    {
        try
        {
            var success = await _cartService.AddToCartAsync(UserId, dto);

            return success
                ? Ok(new { message = "Added to cart." })
                : BadRequest(new { message = "Product not found or inactive." });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int quantity)
    {
        try
        {
            var success = await _cartService.UpdateQuantityAsync(UserId, id, quantity);
            return success ? Ok(new { message = "Cart updated." }) : NotFound();
        }catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var success = await _cartService.RemoveItemAsync(UserId, id);
        return success ? Ok(new { message = "Item removed." }) : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(UserId);
        return Ok(new { message = "Cart cleared." });
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeCart(List<AddToCartDto> items)
    {
        await _cartService.MergeCartAsync(UserId, items);
        return Ok();
    }
}
