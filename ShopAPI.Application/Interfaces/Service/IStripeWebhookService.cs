namespace ShopAPI.Application.Interfaces.Service
{
    public interface IStripeWebhookService
    {
        Task HandleEventAsync(string json, string signatureHeader);
    }
}
