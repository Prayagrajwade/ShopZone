namespace ShopAPI.Application.Interfaces.Managers;

public interface IOrderManager
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(int userId);
    Task<PaymentIntentDto> CreateBuyNowPaymentIntentAsync(int userId, BuyNowDto dto);
    Task<ConfirmOrderResponseDto> ConfirmOrderAsync(int userId, string paymentIntentId);
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<IEnumerable<AdminOrderDto>> GetAllOrdersAdminAsync();
    Task<OrderDetailDto?> GetOrderWithLogsAsync(int userId, int orderId);
    Task<OrderDetailDto?> GetOrderWithLogsAdminAsync(int orderId);
    Task CleanupOldLogsAsync(int paymentLogsDaysOld = 90, int statusLogsDaysOld = 180);
}
