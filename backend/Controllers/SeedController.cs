using ShopAPI.Application.Interfaces.Managers;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Controllers;

/// <summary>
/// Controller for managing database seeding operations.
/// All seed endpoints require admin authorization for security.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class SeedController : ControllerBase
{
    private readonly ISeedManager _seedManager;
    private readonly ILogger<SeedController> _logger;

    public SeedController(ISeedManager seedManager, ILogger<SeedController> logger)
    {
        _seedManager = seedManager;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the admin user if it doesn't already exist.
    /// Requires admin authorization.
    /// </summary>
    [HttpPost("admin")]
    public async Task<IActionResult> SeedAdminUser([FromQuery] string email = "admin@shop.com", [FromQuery] string password = "admin123")
    {
        try
        {
            _logger.LogInformation("Admin seed request received for email: {Email}", email);
            await _seedManager.SeedAdminUserAsync(email, password);
            return Ok(new { message = "Admin user seeded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding admin user");
            return StatusCode(500, new { message = "Error seeding admin user", error = ex.Message });
        }
    }

    /// <summary>
    /// Seeds sample products if they don't already exist.
    /// Requires admin authorization.
    /// </summary>
    [HttpPost("products")]
    public async Task<IActionResult> SeedProducts()
    {
        try
        {
            _logger.LogInformation("Products seed request received");
            await _seedManager.SeedProductsAsync();
            return Ok(new { message = "Products seeded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding products");
            return StatusCode(500, new { message = "Error seeding products", error = ex.Message });
        }
    }

    /// <summary>
    /// Seeds the email invoice template if it doesn't already exist.
    /// Requires admin authorization.
    /// </summary>
    [HttpPost("email-template")]
    public async Task<IActionResult> SeedEmailTemplate()
    {
        try
        {
            _logger.LogInformation("Email template seed request received");
            await _seedManager.SeedInvoiceTemplateAsync();
            return Ok(new { message = "Email template seeded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding email template");
            return StatusCode(500, new { message = "Error seeding email template", error = ex.Message });
        }
    }

    /// <summary>
    /// Seeds all initial data (admin user, products, and email templates) if they don't already exist.
    /// Useful for initial setup of the application.
    /// Requires admin authorization.
    /// </summary>
    [HttpPost("all")]
    public async Task<IActionResult> SeedAll()
    {
        try
        {
            _logger.LogInformation("Full seed request received");
            await _seedManager.SeedAllAsync();
            return Ok(new { message = "All data seeded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding all data");
            return StatusCode(500, new { message = "Error seeding all data", error = ex.Message });
        }
    }
}

