using ShopAPI.Application.DTOs;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductManager _productManager;


    public ProductsController(IProductManager productManager) => _productManager = productManager;

    /// <summary>
    /// Retrieves a list of products, optionally filtered by category and/or search term.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllProduct([FromQuery] string? category, [FromQuery] string? search)
    {
        try
        {
            var products = await _productManager.GetAllProductsAsync(category, search);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Retrieves a single product by its ID. Returns 404 if the product is not found.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productManager.GetProductByIdAsync(id);
            return product is null ? NotFound() : Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Retrieves a list of all product categories. Returns 204 No Content if no categories are found.
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _productManager.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Admin-only endpoint to retrieve all products, including those that are inactive or hidden.
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin()
    {
        try
        {
            var products = await _productManager.GetAllProductAdminAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Admin-only endpoint to create a new product.
    /// Validates the input and returns 201 Created with the new product's details,
    /// or 400 Bad Request if validation fails.
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        try
        {
            var created = await _productManager.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Admin-only endpoint to update an existing product by its ID.
    /// Validates the input and returns 200 OK with the updated product's details,
    /// 404 Not Found if the product doesn't exist, or 400 Bad Request if validation fails.
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        try
        {
            var updated = await _productManager.UpdateProductAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Admin-only endpoint to delete a product by its ID. Returns 200 OK with a success message if deleted,
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _productManager.DeleteProductAsync(id);
            return deleted ? Ok(new { message = "Product deleted." }) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a list of the top 5 best-selling products based on sales data.
    /// </summary>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts()
    {
        try
        {
            var products = await _productManager.GetTopSellingProducts();

            if (products.Count < 5)
                return NoContent();

            return Ok(products);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a list of products based on a list of product IDs provided in the request body.
    /// </summary>
    [HttpPost("by-ids")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIds([FromBody] List<int> ids)
    {
        try
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { message = "Product IDs are required." });

            var products = await _productManager.GetProductByIdsAsync(ids);

            return Ok(products);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }
}
