namespace ShopAPI.Domain.Entity
{
    public class TempleteEntity
    {
        public int Id { get; set; }
        public string Type { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
