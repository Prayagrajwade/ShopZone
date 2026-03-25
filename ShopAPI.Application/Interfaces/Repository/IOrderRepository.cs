namespace ShopAPI.Interfaces.Repository
{
    public interface IOrderRepository
    {
        Task<IEnumerable<CartItemEntity>> GetUserCartItemsAsync(int userId);
        Task<ProductEntity?> GetProductAsync(int productId);
        Task<OrderEntity?> GetOrderByStripeIdAsync(string stripePaymentIntentId);
        Task CreateOrderAsync(OrderEntity order);
        Task UpdateOrderAsync(OrderEntity order);
        Task ClearUserCartAsync(int userId);
        Task<IEnumerable<OrderEntity>> GetOrdersByUserAsync(int userId);
        Task<OrderEntity?> GetOrderByIdAsync(int orderId, int userId);
        Task<IEnumerable<OrderEntity>> GetAllOrdersAsync();
        Task CreatePaymentLogAsync(PaymentLogEntity paymentLog);
        Task CreateOrderStatusLogAsync(OrderStatusLogEntity statusLog);
        Task<IEnumerable<PaymentLogEntity>> GetPaymentLogsByOrderAsync(int orderId);
        Task<IEnumerable<OrderStatusLogEntity>> GetStatusLogsByOrderAsync(int orderId);
        Task DeleteOldPaymentLogsAsync(int daysOld);
        Task DeleteOldOrderStatusLogsAsync(int daysOld);
    }
}
