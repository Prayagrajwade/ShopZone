using ShopAPI.Common;

namespace ShopAPI.Infrastructure.Repository;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _db;

    public CartRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CartItemDto>> GetCartAsync(int userId)
    {
        return await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .Select(c => new CartItemDto(
                c.Id,
                c.ProductId,
                c.Product!.Name,
                c.Product.Price,
                c.Quantity,
                c.Product.ImageUrl,
                c.Product.Price * c.Quantity,
                c.Product.Stock))
            .ToListAsync();
    }

    public async Task<bool> AddToCartAsync(int userId, AddToCartDto dto)
    {
        var product = await _db.Products.FindAsync(dto.ProductId);

        if (product is null || !product.IsActive)
            return false;

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);

        var totalRequestedQty = dto.Quantity;

        if (existing is not null)
            totalRequestedQty += existing.Quantity;

        if (totalRequestedQty > product.Stock)
            throw new BadRequestException($"Only {product.Stock} items available in stock. Only {product.Stock} available.");

        if (existing is not null)
        {
            existing.Quantity += dto.Quantity;
        }
        else
        {
            _db.CartItems.Add(new CartItemEntity
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (item is null) return false;

        if (quantity <= 0)
            _db.CartItems.Remove(item);
        else
            item.Quantity = quantity;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(int userId, int cartItemId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (item is null) return false;

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task ClearCartAsync(int userId)
    {
        var items = _db.CartItems.Where(c => c.UserId == userId);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task MergeCartAsync(int userId, List<AddToCartDto> items)
    {
        foreach (var dto in items)
        {
            await AddToCartAsync(userId, dto);
        }
    }

    public async Task<CartItemEntity?> GetCartItemAsync(int userId, int productId)
    {
        return await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
    }

    public async Task<ProductEntity?> GetProductAsync(int productId)
    {
        return await _db.Products.FindAsync(productId);
    }
}
