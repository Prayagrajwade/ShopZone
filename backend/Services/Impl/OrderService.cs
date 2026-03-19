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

        var total = cartItems.Sum(c => c.Product!.Price * c.Quantity);
        var amountCents = (long)(total * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountCents,
            Currency = _paymentSettings.Currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

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

    public async Task<PaymentIntentDto> CreateBuyNowPaymentIntentAsync(int userId, BuyNowDto dto)
    {
        var product = await _db.Products.FindAsync(dto.ProductId);

        if (product == null || !product.IsActive)
            throw new Exception("Product not available");

        if (dto.Quantity > product.Stock)
            throw new Exception($"Only {product.Stock} available");

        var total = product.Price * dto.Quantity;
        var amountCents = (long)(total * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountCents,
            Currency = _paymentSettings.Currency,
            AutomaticPaymentMethods = new() { Enabled = true },

            Metadata = new Dictionary<string, string>
            {
                ["type"] = "buynow",
                ["productId"] = dto.ProductId.ToString(),
                ["quantity"] = dto.Quantity.ToString(),
                ["userId"] = userId.ToString()
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        return new PaymentIntentDto(intent.ClientSecret, intent.Id, total);
    }

    public async Task<ConfirmOrderResponseDto> ConfirmOrderAsync(int userId, string paymentIntentId)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var service = new PaymentIntentService();
        var intent = await service.GetAsync(paymentIntentId);

        Console.WriteLine("Metadata: " + System.Text.Json.JsonSerializer.Serialize(intent.Metadata));

        if (intent.Metadata != null &&
            intent.Metadata.TryGetValue("type", out var type) &&
            type == "buynow")
        {
            if (intent.Status != AppConstants.PaymentStatus.Succeeded)
                throw new BadRequestException("Payment not completed.");

            var productId = int.Parse(intent.Metadata["productId"]);
            var quantity = int.Parse(intent.Metadata["quantity"]);

            var product = await _db.Products.FindAsync(productId);

            if (product == null)
                throw new BadRequestException("Product not found");

            if (product.Stock < quantity)
                throw new BadRequestException($"Only {product.Stock} available");

            product.Stock -= quantity;

            var order = new Order
            {
                UserId = userId,
                Status = AppConstants.OrderStatus.Paid,
                TotalAmount = product.Price * quantity,
                StripePaymentIntentId = paymentIntentId,
                Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                }
            }
            };

            _db.Orders.Add(order);

            await _db.SaveChangesAsync();

            return new ConfirmOrderResponseDto(order.Id, "Order placed successfully.");
        }

        var cartItems = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new BadRequestException("Cart is empty.");

        if (intent.Status != AppConstants.PaymentStatus.Succeeded)
            throw new BadRequestException("Payment not completed.");

        var total = cartItems.Sum(c => c.Product!.Price * c.Quantity);

        var cartOrder = new Order
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

        _db.Orders.Add(cartOrder);

        _db.CartItems.RemoveRange(cartItems);

        await _db.SaveChangesAsync();

        return new ConfirmOrderResponseDto(cartOrder.Id, "Order placed successfully.");
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
