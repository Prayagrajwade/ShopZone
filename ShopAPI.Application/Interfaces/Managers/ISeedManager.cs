namespace ShopAPI.Application.Interfaces.Managers;

public interface ISeedManager
{
    Task SeedAdminUserAsync(string email = "admin@shop.com", string password = "admin123");
    Task SeedProductsAsync();
    Task SeedInvoiceTemplateAsync();
    Task SeedAllAsync();
}
