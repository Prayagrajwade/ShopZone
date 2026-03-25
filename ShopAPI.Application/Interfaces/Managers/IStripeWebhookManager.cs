namespace ShopAPI.Application.Interfaces.Managers;

public interface IStripeWebhookManager
{
    Task HandleEventAsync(string json, string signatureHeader);
}
