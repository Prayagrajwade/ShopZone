using ShopAPI.Application.DTOs;

[ApiController]
[Route("api/admin/users")]
public class UserController : ControllerBase
{
    private readonly IUserManager _service;

    public UserController(IUserManager service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves a paginated list of users,
    /// optionally filtered by a search term that matches the user's name or email.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers(string? search, int page = 1, int pageSize = 10)
    {
        try
        {
            var result = await _service.GetUsers(search, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new user with the provided details.
    /// The request body should contain the user's name, email, password, and role.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        try
        {
            await _service.CreateUser(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates the details of an existing user identified by their ID.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        try
        {
            await _service.UpdateUser(id, dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Soft deletes a user by marking them as inactive rather than removing their record from the database.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await _service.SoftDeleteUser(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Forces a user to log out by invalidating their security stamp,
    /// which will invalidate all active sessions for that user.
    /// </summary>

    [HttpPost("{id}/force-logout")]
    public async Task<IActionResult> ForceLogoutUser(int id)
    {
        try
        {
            await _service.ForceLogoutUser(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}