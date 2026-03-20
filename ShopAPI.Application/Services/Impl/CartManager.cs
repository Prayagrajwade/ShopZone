using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Common;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Application.Services.Impl;

public class CartManager : ICartManager
{
    private readonly ICartRepository _cartRepository;

    public CartManager(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<IEnumerable<CartItemDto>> GetCartAsync(int userId)
    {
        return await _cartRepository.GetCartAsync(userId);
    }

    public async Task<bool> AddToCartAsync(int userId, AddToCartDto dto)
    {
        return await _cartRepository.AddToCartAsync(userId, dto);
    }

    public async Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity)
    {
        return await _cartRepository.UpdateQuantityAsync(userId, cartItemId, quantity);
    }

    public async Task<bool> RemoveItemAsync(int userId, int cartItemId)
    {
        return await _cartRepository.RemoveItemAsync(userId, cartItemId);
    }

    public async Task ClearCartAsync(int userId)
    {
        await _cartRepository.ClearCartAsync(userId);
    }

    public async Task MergeCartAsync(int userId, List<AddToCartDto> items)
    {
        await _cartRepository.MergeCartAsync(userId, items);
    }
}
