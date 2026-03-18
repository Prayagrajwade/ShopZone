using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("seed-invoice")]
        public async Task<IActionResult> SeedInvoice()
        {
            try
            {
              await _seeder.SeedInvoiceTemplateAsync();
              return Ok();
            }
            catch (Exception ex)
            {
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
        }

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
