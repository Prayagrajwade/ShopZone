namespace ShopAPI.Interfaces.Repository
{
    public interface IProductsRepository
    {
        Task<List<TopProductDto>> GetTopSellingProductAsync();
        Task<List<ProductEntity>> GetByIdsAsync(List<int> ids);
        Task<IEnumerable<ProductDto>> GetAllAsync(string? category, string? search);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<ProductDto>> GetAllAdminAsync();
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ProductEntity?> FindByIdAsync(int id);
        Task SeedProductsAsync(List<ProductEntity> products);
    }
}
