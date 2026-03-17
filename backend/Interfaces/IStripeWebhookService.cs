namespace ShopAPI.Interfaces
{
    public interface IStripeWebhookService
    {
        Task HandleEventAsync(string json, string signatureHeader);
    }
}
