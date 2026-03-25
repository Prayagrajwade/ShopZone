using ShopAPI.Application.DTOs;
using ShopAPI.Application.Interfaces.Managers;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthManager authManager, ILogger<AuthController> logger)
    {
        _authManager = authManager;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user with the provided details
    /// and returns an authentication token upon successful registration.
    /// </summary>

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            _logger.LogInformation("Register request received for email: {Email}", dto.Email);
            var result = await _authManager.RegisterAsync(dto);
            _logger.LogInformation("User registered successfully: {UserId}", result.UserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Authenticates a user with the provided email and password,
    /// returning an authentication token if the credentials are valid.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            _logger.LogInformation("Login request received for email: {Email}", dto.Email);
            var result = await _authManager.LoginAsync(dto);
            _logger.LogInformation("User logged in successfully: {UserId}", result.UserId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during login" });
        }
    }
}
