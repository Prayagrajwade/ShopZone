using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;


    public ProductsController(IProductService productService) => _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? search)
    {
        try
        {
            var products = await _productService.GetAllAsync(category, search);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return product is null ? NotFound() : Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin()
    {
        try
        {
            var products = await _productService.GetAllAdminAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        try
        {
            var created = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        try
        {
            var updated = await _productService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _productService.DeleteAsync(id);
            return deleted ? Ok(new { message = "Product deleted." }) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts()
    {
        try
        {
            var products = await _productService.GetTopSellingProducts();

            if (products.Count < 5)
                return NoContent();

            return Ok(products);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("by-ids")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIds([FromBody] List<int> ids)
    {
        try
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { message = "Product IDs are required." });

            var products = await _productService.GetByIdsAsync(ids);

            return Ok(products);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
}
