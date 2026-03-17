using Microsoft.Extensions.Options;
using Stripe;

namespace ShopAPI.Services.Impl;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly PaymentSettings _paymentSettings;

    public OrderService(AppDbContext db, IConfiguration config, IOptions<PaymentSettings> paymentOptions)
    {
        _db = db;
        _config = config;
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        _paymentSettings = paymentOptions.Value;
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int userId)
    {
        var cartItems = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        var total        = cartItems.Sum(c => c.Product!.Price * c.Quantity);
        var amountCents  = (long)(total * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount   = amountCents,
            Currency = _paymentSettings.Currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options);

        _db.PaymentLogs.Add(new PaymentLog
        {
            StripeEventId = intent.Id,
            EventType = AppConstants.PaymentEvents.Created,
            PaymentIntentId = intent.Id,
            Status = intent.Status,
            RawJson = System.Text.Json.JsonSerializer.Serialize(intent)
        });
        await _db.SaveChangesAsync();

        return new PaymentIntentDto(intent.ClientSecret, intent.Id, total);
    }

    public async Task<ConfirmOrderResponseDto> ConfirmOrderAsync(int userId, string paymentIntentId)
    {
        var cartItems = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(paymentIntentId);

        if (intent.Status != AppConstants.PaymentStatus.Succeeded)
        {
            _db.PaymentLogs.Add(new PaymentLog
            {
                StripeEventId = Guid.NewGuid().ToString(),
                EventType = AppConstants.PaymentEvents.FailedManualCheck,
                PaymentIntentId = intent.Id,
                Status = intent.Status,
                RawJson = System.Text.Json.JsonSerializer.Serialize(intent)
            });

            await _db.SaveChangesAsync();

            throw new InvalidOperationException("Payment not completed.");
        }

        var total = cartItems.Sum(c => c.Product!.Price * c.Quantity);

        var order = new Order
        {
            UserId = userId,
            Status = AppConstants.OrderStatus.Paid,
            TotalAmount = total,
            StripePaymentIntentId = paymentIntentId,
            Items = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Product!.Price
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        _db.PaymentLogs.Add(new PaymentLog
        {
            StripeEventId = Guid.NewGuid().ToString(),
            EventType = AppConstants.PaymentEvents.Checked,
            PaymentIntentId = intent.Id,
            Status = intent.Status,
            RawJson = System.Text.Json.JsonSerializer.Serialize(intent),
            OrderId = order.Id
        });
        _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync();

        return new ConfirmOrderResponseDto(order.Id, "Order placed successfully.");
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
    {
        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order is null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<AdminOrderDto>> GetAllOrdersAdminAsync()
    {
        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(o => new AdminOrderDto(
            o.Id,
            o.Status,
            o.TotalAmount,
            o.CreatedAt,
            o.User!.Name,
            o.User.Email,
            o.Items.Select(MapOrderItemToDto).ToList()
        ));
    }


    private static OrderDto MapToDto(Order o) => new(
        o.Id,
        o.Status,
        o.TotalAmount,
        o.CreatedAt,
        o.Items.Select(MapOrderItemToDto).ToList()
    );

    private static OrderItemDto MapOrderItemToDto(OrderItem i) => new(
        i.ProductId,
        i.Product!.Name,
        i.Quantity,
        i.UnitPrice
    );
}
