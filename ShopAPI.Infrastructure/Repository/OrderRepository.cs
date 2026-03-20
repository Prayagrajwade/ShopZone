using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Infrastructure.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<CartItemEntity>> GetUserCartItemsAsync(int userId)
    {
        return await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<ProductEntity?> GetProductAsync(int productId)
    {
        return await _db.Products.FindAsync(productId);
    }

    public async Task<OrderEntity?> GetOrderByStripeIdAsync(string stripePaymentIntentId)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.StripePaymentIntentId == stripePaymentIntentId);
    }

    public async Task CreateOrderAsync(OrderEntity order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateOrderAsync(OrderEntity order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }

    public async Task ClearUserCartAsync(int userId)
    {
        var items = _db.CartItems.Where(c => c.UserId == userId);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrderEntity>> GetOrdersByUserAsync(int userId)
    {
        return await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<OrderEntity?> GetOrderByIdAsync(int orderId, int userId)
    {
        return await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }

    public async Task<IEnumerable<OrderEntity>> GetAllOrdersAsync()
    {
        return await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
