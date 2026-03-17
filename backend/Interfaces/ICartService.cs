using ShopAPI.DTOs;

namespace ShopAPI.Interfaces;

public interface ICartService
{
    Task<IEnumerable<CartItemDto>> GetCartAsync(int userId);
    Task<bool> AddToCartAsync(int userId, AddToCartDto dto);
    Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity);
    Task<bool> RemoveItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
}
