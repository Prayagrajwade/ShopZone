namespace ShopAPI.Application.Interfaces.Service;

public interface IStripeWebhookManager
{
    Task HandleEventAsync(string json, string signatureHeader);
}
