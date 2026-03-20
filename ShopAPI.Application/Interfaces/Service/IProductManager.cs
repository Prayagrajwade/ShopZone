namespace ShopAPI.Application.Interfaces.Service;

public interface IProductManager
{
    Task<IEnumerable<ProductDto>> GetAllAsync(string? category, string? search);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<ProductDto>> GetAllAdminAsync();
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<List<TopProductDto>> GetTopSellingProducts();
    Task<bool> DeleteAsync(int id);
    Task<List<ProductEntity>> GetByIdsAsync(List<int> ids);
}
