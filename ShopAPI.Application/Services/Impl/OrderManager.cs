using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Common;
using ShopAPI.Interfaces.Repository;
using Stripe;

namespace ShopAPI.Application.Services.Impl;

public class OrderManager : IOrderManager
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IConfiguration _config;
    private readonly PaymentSettings _paymentSettings;

    public OrderManager(IOrderRepository orderRepository, IProductsRepository productsRepository,
        IConfiguration config, IOptions<PaymentSettings> paymentOptions)
    {
        _orderRepository = orderRepository;
        _productsRepository = productsRepository;
        _config = config;
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        _paymentSettings = paymentOptions.Value;
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int userId)
    {
        var cartItems = await _orderRepository.GetUserCartItemsAsync(userId);

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

        return new PaymentIntentDto(intent.ClientSecret, intent.Id, total);
    }

    public async Task<PaymentIntentDto> CreateBuyNowPaymentIntentAsync(int userId, BuyNowDto dto)
    {
        var product = await _productsRepository.GetByIdAsync(dto.ProductId);
        var productEntity = await _orderRepository.GetProductAsync(dto.ProductId);

        if (productEntity == null || !productEntity.IsActive)
            throw new Exception("Product not available");

        if (dto.Quantity > productEntity.Stock)
            throw new Exception($"Only {productEntity.Stock} available");

        var total = productEntity.Price * dto.Quantity;
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

        var stripeService = new PaymentIntentService();
        var intent = await stripeService.CreateAsync(options);

        return new PaymentIntentDto(intent.ClientSecret, intent.Id, total);
    }

    public async Task<ConfirmOrderResponseDto> ConfirmOrderAsync(int userId, string paymentIntentId)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var service = new PaymentIntentService();
        var intent = await service.GetAsync(paymentIntentId);

        if (intent.Metadata != null &&
            intent.Metadata.TryGetValue("type", out var type) &&
            type == "buynow")
        {
            if (intent.Status != AppConstants.PaymentStatus.Succeeded)
                throw new BadRequestException("Payment not completed.");

            var productId = int.Parse(intent.Metadata["productId"]);
            var quantity = int.Parse(intent.Metadata["quantity"]);

            var product = await _orderRepository.GetProductAsync(productId);

            if (product == null)
                throw new BadRequestException("Product not found");

            if (product.Stock < quantity)
                throw new BadRequestException($"Only {product.Stock} available");

            product.Stock -= quantity;

            var order = new OrderEntity
            {
                UserId = userId,
                Status = AppConstants.OrderStatus.Paid,
                TotalAmount = product.Price * quantity,
                StripePaymentIntentId = paymentIntentId,
                Items = new List<OrderItemEntity>
                {
                    new OrderItemEntity
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    }
                }
            };

            await _orderRepository.CreateOrderAsync(order);
            return new ConfirmOrderResponseDto(order.Id, "Order placed successfully.");
        }

        var cartItems = await _orderRepository.GetUserCartItemsAsync(userId);

        if (!cartItems.Any())
            throw new BadRequestException("Cart is empty.");

        if (intent.Status != AppConstants.PaymentStatus.Succeeded)
            throw new BadRequestException("Payment not completed.");

        var total = cartItems.Sum(c => c.Product!.Price * c.Quantity);

        var cartOrder = new OrderEntity
        {
            UserId = userId,
            Status = AppConstants.OrderStatus.Paid,
            TotalAmount = total,
            StripePaymentIntentId = paymentIntentId,
            Items = cartItems.Select(c => new OrderItemEntity
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Product!.Price
            }).ToList()
        };

        await _orderRepository.CreateOrderAsync(cartOrder);
        await _orderRepository.ClearUserCartAsync(userId);

        return new ConfirmOrderResponseDto(cartOrder.Id, "Order placed successfully.");
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
    {
        var orders = await _orderRepository.GetOrdersByUserAsync(userId);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
        return order is null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<AdminOrderDto>> GetAllOrdersAdminAsync()
    {
        var orders = await _orderRepository.GetAllOrdersAsync();

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

    private static OrderDto MapToDto(OrderEntity o) => new(
        o.Id,
        o.Status,
        o.TotalAmount,
        o.CreatedAt,
        o.Items.Select(MapOrderItemToDto).ToList()
    );

    private static OrderItemDto MapOrderItemToDto(OrderItemEntity i) => new(
        i.ProductId,
        i.Product!.Name,
        i.Quantity,
        i.UnitPrice
    );
}
