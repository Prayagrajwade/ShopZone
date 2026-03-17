namespace ShopAPI.Models
{
    public class OrderStatusLog
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string OldStatus { get; set; } = "";
        public string NewStatus { get; set; } = "";

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}
