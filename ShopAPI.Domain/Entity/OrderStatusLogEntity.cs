namespace ShopAPI.Domain.Entity
{
    public class OrderStatusLogEntity
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string OldStatus { get; set; } = "";
        public string NewStatus { get; set; } = "";

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OrderEntity? Order { get; set; }
    }
}
