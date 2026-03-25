using Microsoft.Extensions.Logging;
using ShopAPI.Application.Interfaces.Managers;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Application.Managers;

public class ProductManager : IProductManager
{
    private readonly IProductsRepository _productsRepository;
    private readonly ILogger<ProductManager> _logger;

    public ProductManager(IProductsRepository productsRepository, ILogger<ProductManager> logger)
    {
        _productsRepository = productsRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? category, string? search)
    {
        try
        {
            _logger.LogInformation("Fetching all products with category: {Category}, search: {Search}", category ?? "none", search ?? "none");
            var products = await _productsRepository.GetAllAsync(category, search);
            _logger.LogInformation("Retrieved {ProductCount} products", products.Count());
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all products");
            throw;
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching product with ID: {ProductId}", id);
            var product = await _productsRepository.GetByIdAsync(id);
            if (product == null)
                _logger.LogWarning("Product not found with ID: {ProductId}", id);
            else
                _logger.LogInformation("Product found: {ProductName}", product.Name);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all categories");
            var categories = await _productsRepository.GetCategoriesAsync();
            _logger.LogInformation("Retrieved {CategoryCount} categories", categories.Count());
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            throw;
        }
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductAdminAsync()
    {
        try
        {
            _logger.LogInformation("Admin: Fetching all products");
            var products = await _productsRepository.GetAllAdminAsync();
            _logger.LogInformation("Admin: Retrieved {ProductCount} products", products.Count());
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all products for admin");
            throw;
        }
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", dto.Name);
            var product = await _productsRepository.CreateAsync(dto);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", dto.Name);
            throw;
        }
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        try
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);
            var product = await _productsRepository.UpdateAsync(id, dto);
            if (product == null)
                _logger.LogWarning("Product not found for update with ID: {ProductId}", id);
            else
                _logger.LogInformation("Product updated successfully: {ProductName}", product.Name);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<List<TopProductDto>> GetTopSellingProducts()
    {
        try
        {
            _logger.LogInformation("Fetching top selling products");
            var topProducts = await _productsRepository.GetTopSellingProductAsync();
            _logger.LogInformation("Retrieved {TopProductCount} top selling products", topProducts.Count);
            return topProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top selling products");
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            var result = await _productsRepository.DeleteAsync(id);
            if (result)
                _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
            else
                _logger.LogWarning("Product deletion failed for ID: {ProductId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<List<ProductEntity>> GetProductByIdsAsync(List<int> ids)
    {
        try
        {
            _logger.LogInformation("Fetching products by IDs: {ProductIds}", string.Join(", ", ids));
            var products = await _productsRepository.GetByIdsAsync(ids);
            _logger.LogInformation("Retrieved {ProductCount} products from {RequestedCount} IDs", products.Count, ids.Count);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products by IDs");
            throw;
        }
    }
}
