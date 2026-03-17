using ShopAPI.DTOs;

namespace ShopAPI.Interfaces;

public interface IOrderService
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(int userId);
    Task<ConfirmOrderResponseDto> ConfirmOrderAsync(int userId, string paymentIntentId);
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<IEnumerable<AdminOrderDto>> GetAllOrdersAdminAsync();
}
