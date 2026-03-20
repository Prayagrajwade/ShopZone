namespace ShopAPI.Application.Interfaces.Service;

public interface IOrderManager
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(int userId);
    Task<PaymentIntentDto> CreateBuyNowPaymentIntentAsync(int userId, BuyNowDto dto);
    Task<ConfirmOrderResponseDto> ConfirmOrderAsync(int userId, string paymentIntentId);
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<IEnumerable<AdminOrderDto>> GetAllOrdersAdminAsync();
}
