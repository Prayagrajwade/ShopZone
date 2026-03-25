using Microsoft.Extensions.Logging;
using ShopAPI.Application.Interfaces.Managers;
using ShopAPI.Common;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Application.Managers;

public class SeedManager : ISeedManager
{
    private readonly IUserRopository _userRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly ILogger<SeedManager> _logger;

    public SeedManager(IUserRopository userRepository, IProductsRepository productsRepository,
        IEmailTemplateRepository emailTemplateRepository, ILogger<SeedManager> logger)
    {
        _userRepository = userRepository;
        _productsRepository = productsRepository;
        _emailTemplateRepository = emailTemplateRepository;
        _logger = logger;
    }

    public async Task SeedAdminUserAsync(string email = "admin@shop.com", string password = "admin123")
    {
        try
        {
            _logger.LogInformation("Checking if admin user exists with email: {Email}", email);

            var adminExists = await _userRepository.UserExistsByEmailAsync(email);

            if (adminExists)
            {
                _logger.LogWarning("Admin user already exists with email: {Email}. Skipping seed.", email);
                return;
            }

            var adminUser = new UserEntity
            {
                Name = "Admin",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = AppConstants.Roles.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.CreateUserAsync(adminUser);
            _logger.LogInformation("Admin user created successfully with email: {Email}, ID: {UserId}", 
                email, adminUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding admin user with email: {Email}", email);
            throw;
        }
    }

    public async Task SeedProductsAsync()
    {
        try
        {
            _logger.LogInformation("Checking if products already exist");

            var existingProducts = await _productsRepository.GetAllAsync(null, null);

            if (existingProducts.Any())
            {
                _logger.LogWarning("Products already exist in database ({Count} found). Skipping seed.", 
                    existingProducts.Count());
                return;
            }

            var products = new List<ProductEntity>
            {
                new ProductEntity
                {
                    Name = "Wireless Headphones",
                    Description = "Premium noise-cancelling wireless headphones with 30hr battery",
                    Price = 79.99m,
                    Stock = 50,
                    Category = "Electronics",
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductEntity
                {
                    Name = "Smart Watch",
                    Description = "Feature-packed smartwatch with health tracking",
                    Price = 149.99m,
                    Stock = 30,
                    Category = "Electronics",
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductEntity
                {
                    Name = "Running Shoes",
                    Description = "Lightweight performance running shoes",
                    Price = 89.99m,
                    Stock = 75,
                    Category = "Footwear",
                    ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductEntity
                {
                    Name = "Coffee Maker",
                    Description = "12-cup programmable coffee maker with thermal carafe",
                    Price = 49.99m,
                    Stock = 40,
                    Category = "Kitchen",
                    ImageUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductEntity
                {
                    Name = "Leather Wallet",
                    Description = "Slim genuine leather bifold wallet with RFID blocking",
                    Price = 29.99m,
                    Stock = 100,
                    Category = "Accessories",
                    ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductEntity
                {
                    Name = "Backpack",
                    Description = "Durable 30L travel backpack with laptop compartment",
                    Price = 59.99m,
                    Stock = 60,
                    Category = "Bags",
                    ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _productsRepository.SeedProductsAsync(products);
            _logger.LogInformation("Successfully seeded {ProductCount} products", products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding products");
            throw;
        }
    }

    public async Task SeedInvoiceTemplateAsync()
    {
        try
        {
            _logger.LogInformation("Checking if Invoice template already exists");

            var templateExists = await _emailTemplateRepository.ExistsAsync("Invoice");

            if (templateExists)
            {
                _logger.LogWarning("Invoice template already exists. Skipping seed.");
                return;
            }

            var template = new TempleteEntity
            {
                Type = "Invoice",
                Subject = "Invoice - Order {{OrderId}}",
                Body = @"
                    <h2>Order Invoice</h2>

                    <p>Hello {{UserName}},</p>

                    <p>Your order <b>#{{OrderId}}</b> is confirmed.</p>

                    <table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>
                        <thead>
                            <tr>
                                <th>Product</th>
                                <th>Price</th>
                                <th>Quantity</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {{ProductRows}}
                        </tbody>
                    </table>

                    <h3>Total Amount: ₹{{Amount}}</h3>

                    <p>Thanks for shopping!</p>
                ",
                CreatedAt = DateTime.UtcNow
            };

            await _emailTemplateRepository.AddAsync(template);
            _logger.LogInformation("Invoice template seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding Invoice template");
            throw;
        }
    }

    public async Task SeedAllAsync()
    {
        try
        {
            _logger.LogInformation("Starting to seed all data...");

            await SeedAdminUserAsync();
            await SeedProductsAsync();
            await SeedInvoiceTemplateAsync();

            _logger.LogInformation("All data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding all data");
            throw;
        }
    }
}
