using ShopAPI.Interfaces.Email;

namespace ShopAPI.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTemplateController : ControllerBase
    {
        private readonly IEmailService _seeder;

        public EmailTemplateController(IEmailService seeder)
        {
            _seeder = seeder;
        }

        /// <summary>
        /// Sends a test email to verify that the email service is working correctly.
        /// </summary>
        [HttpGet("test-email")]
        [AllowAnonymous]
        public async Task<IActionResult> TestEmail()
        {
            await _seeder.SendEmailAsync(
                "test@yopmail.com",
                "Test Subject",
                "<h1>Hello Test</h1>"
            );

            return Ok("Email sent");
        }
    }
}
