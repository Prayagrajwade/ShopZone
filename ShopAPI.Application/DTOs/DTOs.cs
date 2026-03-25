namespace ShopAPI.Application.DTOs;

// Auth
public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Role, string Name, int UserId);

// Product
public record ProductDto(int Id, string Name, string Description, decimal Price, int Stock, string Category, string ImageUrl, bool IsActive);
public record CreateProductDto(string Name, string Description, decimal Price, int Stock, string Category, string ImageUrl);
public record UpdateProductDto(string Name, string Description, decimal Price, int Stock, string Category, string ImageUrl, bool IsActive);

// Cart
public record AddToCartDto(int ProductId, int Quantity);
public record CartItemDto(int Id, int ProductId, string ProductName, decimal Price, int Quantity, string ImageUrl, decimal Subtotal, int Stock);

// Order
public record OrderDto(int Id, string Status, decimal TotalAmount, DateTime CreatedAt, List<OrderItemDto> Items);
public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public record AdminOrderDto(int Id, string Status, decimal TotalAmount, DateTime CreatedAt, string UserName, string UserEmail, List<OrderItemDto> Items);
public record ConfirmOrderResponseDto(int OrderId, string Message);
public record OrderDetailDto(int Id, string Status, decimal TotalAmount, DateTime CreatedAt, List<OrderItemDto> Items, List<PaymentLogDto> PaymentLogs, List<StatusLogDto> StatusLogs);

// Order Logs
public record PaymentLogDto(int Id, string StripeEventId, string EventType, string PaymentIntentId, string Status, DateTime CreatedAt);
public record StatusLogDto(int Id, string OldStatus, string NewStatus, string? Note, DateTime CreatedAt);

// Payment
public record PaymentIntentDto(string ClientSecret, string PaymentIntentId, decimal Amount);

public record UserDto(string Name, string Email);

public record TopProductDto(int Id, string Name, decimal Price, int TotalSold);

public record BuyNowDto(int ProductId, int Quantity);
