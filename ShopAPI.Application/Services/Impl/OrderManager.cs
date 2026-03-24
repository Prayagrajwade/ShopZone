using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<OrderManager> _logger;

    public OrderManager(IOrderRepository orderRepository, IProductsRepository productsRepository,
        IConfiguration config, IOptions<PaymentSettings> paymentOptions, ILogger<OrderManager> logger)
    {
        _orderRepository = orderRepository;
        _productsRepository = productsRepository;
        _config = config;
        _logger = logger;
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        _paymentSettings = paymentOptions.Value;
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Creating payment intent for UserId: {UserId}", userId);

            var cartItems = await _orderRepository.GetUserCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                _logger.LogWarning("Cart is empty for UserId: {UserId}", userId);
                throw new InvalidOperationException("Cart is empty.");
            }

            var total = cartItems.Sum(c => c.Product!.Price * c.Quantity);
            var amountCents = (long)(total * 100);

            _logger.LogInformation("Creating Stripe PaymentIntent for UserId: {UserId}, Amount: {Amount} {Currency}", 
                userId, total, _paymentSettings.Currency);

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

            _logger.LogInformation("PaymentIntent created successfully for UserId: {UserId}, PaymentIntentId: {PaymentIntentId}", 
                userId, intent.Id);

            return new PaymentIntentDto(intent.ClientSecret, intent.Id, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<PaymentIntentDto> CreateBuyNowPaymentIntentAsync(int userId, BuyNowDto dto)
    {
        try
        {
            _logger.LogInformation("Creating BuyNow payment intent for UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}", 
                userId, dto.ProductId, dto.Quantity);

            var product = await _productsRepository.GetByIdAsync(dto.ProductId);
            var productEntity = await _orderRepository.GetProductAsync(dto.ProductId);

            if (productEntity == null || !productEntity.IsActive)
            {
                _logger.LogWarning("Product not available for BuyNow: ProductId: {ProductId}", dto.ProductId);
                throw new Exception("Product not available");
            }

            if (dto.Quantity > productEntity.Stock)
            {
                _logger.LogWarning("Insufficient stock for ProductId: {ProductId}, Required: {Required}, Available: {Available}", 
                    dto.ProductId, dto.Quantity, productEntity.Stock);
                throw new Exception($"Only {productEntity.Stock} available");
            }

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

            _logger.LogInformation("BuyNow PaymentIntent created successfully for UserId: {UserId}, PaymentIntentId: {PaymentIntentId}", 
                userId, intent.Id);

            return new PaymentIntentDto(intent.ClientSecret, intent.Id, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating BuyNow payment intent for UserId: {UserId}, ProductId: {ProductId}", 
                userId, dto.ProductId);
            throw;
        }
    }

    public async Task<ConfirmOrderResponseDto> ConfirmOrderAsync(int userId, string paymentIntentId)
    {
        try
        {
            _logger.LogInformation("Confirming order for UserId: {UserId}, PaymentIntentId: {PaymentIntentId}", 
                userId, paymentIntentId);

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

            var service = new PaymentIntentService();
            var intent = await service.GetAsync(paymentIntentId);

            if (intent.Metadata != null &&
                intent.Metadata.TryGetValue("type", out var type) &&
                type == "buynow")
            {
                _logger.LogInformation("Processing BuyNow order for UserId: {UserId}", userId);

                if (intent.Status != AppConstants.PaymentStatus.Succeeded)
                {
                    _logger.LogWarning("Payment not completed for BuyNow order: UserId: {UserId}, Status: {Status}", 
                        userId, intent.Status);
                    throw new BadRequestException("Payment not completed.");
                }

                var productId = int.Parse(intent.Metadata["productId"]);
                var quantity = int.Parse(intent.Metadata["quantity"]);

                var product = await _orderRepository.GetProductAsync(productId);

                if (product == null)
                {
                    _logger.LogWarning("Product not found for BuyNow order: ProductId: {ProductId}", productId);
                    throw new BadRequestException("Product not found");
                }

                if (product.Stock < quantity)
                {
                    _logger.LogWarning("Insufficient stock for BuyNow order: ProductId: {ProductId}, Required: {Required}, Available: {Available}", 
                        productId, quantity, product.Stock);
                    throw new BadRequestException($"Only {product.Stock} available");
                }

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
                _logger.LogInformation("BuyNow order created successfully: OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}", 
                    order.Id, userId, order.TotalAmount);

                return new ConfirmOrderResponseDto(order.Id, "Order placed successfully.");
            }

            _logger.LogInformation("Processing Cart order for UserId: {UserId}", userId);

            var cartItems = await _orderRepository.GetUserCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                _logger.LogWarning("Cart is empty for Cart order: UserId: {UserId}", userId);
                throw new BadRequestException("Cart is empty.");
            }

            if (intent.Status != AppConstants.PaymentStatus.Succeeded)
            {
                _logger.LogWarning("Payment not completed for Cart order: UserId: {UserId}, Status: {Status}", 
                    userId, intent.Status);
                throw new BadRequestException("Payment not completed.");
            }

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

            _logger.LogInformation("Cart order created successfully: OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}, ItemCount: {ItemCount}", 
                cartOrder.Id, userId, cartOrder.TotalAmount, cartOrder.Items.Count);

            return new ConfirmOrderResponseDto(cartOrder.Id, "Order placed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order for UserId: {UserId}, PaymentIntentId: {PaymentIntentId}", 
                userId, paymentIntentId);
            throw;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching orders for UserId: {UserId}", userId);
            var orders = await _orderRepository.GetOrdersByUserAsync(userId);
            _logger.LogInformation("Retrieved {OrderCount} orders for UserId: {UserId}", orders.Count(), userId);
            return orders.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders for UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        try
        {
            _logger.LogInformation("Fetching order OrderId: {OrderId} for UserId: {UserId}", orderId, userId);
            var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
            if (order is null)
            {
                _logger.LogWarning("Order not found: OrderId: {OrderId}, UserId: {UserId}", orderId, userId);
                return null;
            }
            return MapToDto(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order OrderId: {OrderId} for UserId: {UserId}", orderId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<AdminOrderDto>> GetAllOrdersAdminAsync()
    {
        try
        {
            _logger.LogInformation("Admin: Fetching all orders");
            var orders = await _orderRepository.GetAllOrdersAsync();
            _logger.LogInformation("Admin: Retrieved {OrderCount} orders", orders.Count());

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all orders for admin");
            throw;
        }
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
