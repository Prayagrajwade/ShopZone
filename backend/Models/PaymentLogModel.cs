namespace ShopAPI.Models
{
    public class PaymentLog
    {
        public int Id { get; set; }

        public int? OrderId { get; set; }

        public string StripeEventId { get; set; } = "";
        public string EventType { get; set; } = "";
        public string PaymentIntentId { get; set; } = "";

        public string Status { get; set; } = ""; 

        public string RawJson { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}
