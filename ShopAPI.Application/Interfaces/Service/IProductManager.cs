namespace ShopAPI.Application.Interfaces.Service;

public interface IProductManager
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? category, string? search);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<ProductDto>> GetAllProductAdminAsync();
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<List<TopProductDto>> GetTopSellingProducts();
    Task<bool> DeleteProductAsync(int id);
    Task<List<ProductEntity>> GetProductByIdsAsync(List<int> ids);
}
