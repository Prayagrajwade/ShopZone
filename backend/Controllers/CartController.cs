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
public class CartController : ControllerBase
{
    private readonly ICartManager _cartManager;

    public CartController(ICartManager cartManager) => _cartManager = cartManager;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var items = await _cartManager.GetCartAsync(UserId);
        return Ok(items);
    }

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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var success = await _cartManager.RemoveItemAsync(UserId, id);
        return success ? Ok(new { message = "Item removed." }) : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartManager.ClearCartAsync(UserId);
        return Ok(new { message = "Cart cleared." });
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeCart(List<AddToCartDto> items)
    {
        await _cartManager.MergeCartAsync(UserId, items);
        return Ok();
    }
}
