using Microsoft.Extensions.Logging;
using ShopAPI.Application.Interfaces.Managers;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Application.Managers;

public class CartManager : ICartManager
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<CartManager> _logger;

    public CartManager(ICartRepository cartRepository, ILogger<CartManager> logger)
    {
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CartItemDto>> GetCartAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching cart for UserId: {UserId}", userId);
            var items = await _cartRepository.GetCartAsync(userId);
            _logger.LogInformation("Retrieved {ItemCount} items in cart for UserId: {UserId}", items.Count(), userId);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cart for UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddToCartAsync(int userId, AddToCartDto dto)
    {
        try
        {
            _logger.LogInformation("Adding product {ProductId} to cart for UserId: {UserId}, Quantity: {Quantity}", 
                dto.ProductId, userId, dto.Quantity);
            var result = await _cartRepository.AddToCartAsync(userId, dto);
            if (result)
                _logger.LogInformation("Product {ProductId} added to cart successfully for UserId: {UserId}", 
                    dto.ProductId, userId);
            else
                _logger.LogWarning("Failed to add product {ProductId} to cart for UserId: {UserId}", 
                    dto.ProductId, userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to cart for UserId: {UserId}", 
                dto.ProductId, userId);
            throw;
        }
    }

    public async Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity)
    {
        try
        {
            _logger.LogInformation("Updating cart item {CartItemId} for UserId: {UserId}, New Quantity: {Quantity}", 
                cartItemId, userId, quantity);
            var result = await _cartRepository.UpdateQuantityAsync(userId, cartItemId, quantity);
            if (result)
                _logger.LogInformation("Cart item {CartItemId} updated successfully for UserId: {UserId}", 
                    cartItemId, userId);
            else
                _logger.LogWarning("Failed to update cart item {CartItemId} for UserId: {UserId}", 
                    cartItemId, userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quantity for cart item {CartItemId} for UserId: {UserId}", 
                cartItemId, userId);
            throw;
        }
    }

    public async Task<bool> RemoveItemAsync(int userId, int cartItemId)
    {
        try
        {
            _logger.LogInformation("Removing cart item {CartItemId} for UserId: {UserId}", cartItemId, userId);
            var result = await _cartRepository.RemoveItemAsync(userId, cartItemId);
            if (result)
                _logger.LogInformation("Cart item {CartItemId} removed successfully for UserId: {UserId}", 
                    cartItemId, userId);
            else
                _logger.LogWarning("Failed to remove cart item {CartItemId} for UserId: {UserId}", 
                    cartItemId, userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cart item {CartItemId} for UserId: {UserId}", 
                cartItemId, userId);
            throw;
        }
    }

    public async Task ClearCartAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Clearing cart for UserId: {UserId}", userId);
            await _cartRepository.ClearCartAsync(userId);
            _logger.LogInformation("Cart cleared successfully for UserId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart for UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task MergeCartAsync(int userId, List<AddToCartDto> items)
    {
        try
        {
            _logger.LogInformation("Merging {ItemCount} items into cart for UserId: {UserId}", items.Count, userId);
            await _cartRepository.MergeCartAsync(userId, items);
            _logger.LogInformation("Cart merged successfully for UserId: {UserId} with {ItemCount} items", userId, items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging cart for UserId: {UserId}", userId);
            throw;
        }
    }
}
