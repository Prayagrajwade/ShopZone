namespace ShopAPI.Application.Interfaces.Managers;

public interface ICartManager
{
    Task<IEnumerable<CartItemDto>> GetCartAsync(int userId);
    Task<bool> AddToCartAsync(int userId, AddToCartDto dto);
    Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity);
    Task<bool> RemoveItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
    Task MergeCartAsync(int userId, List<AddToCartDto> items);
}
