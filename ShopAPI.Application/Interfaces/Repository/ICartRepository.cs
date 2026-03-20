namespace ShopAPI.Interfaces.Repository
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItemDto>> GetCartAsync(int userId);
        Task<bool> AddToCartAsync(int userId, AddToCartDto dto);
        Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity);
        Task<bool> RemoveItemAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
        Task MergeCartAsync(int userId, List<AddToCartDto> items);
        Task<CartItemEntity?> GetCartItemAsync(int userId, int productId);
        Task<ProductEntity?> GetProductAsync(int productId);
    }
}
