using ShopAPI.Application.Interfaces.Service;
using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Application.Services.Impl;

public class ProductManager : IProductManager
{
    private readonly IProductsRepository _productsRepository;

    public ProductManager(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? category, string? search)
    {
        return await _productsRepository.GetAllAsync(category, search);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        return await _productsRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _productsRepository.GetCategoriesAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAdminAsync()
    {
        return await _productsRepository.GetAllAdminAsync();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        return await _productsRepository.CreateAsync(dto);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        return await _productsRepository.UpdateAsync(id, dto);
    }

    public async Task<List<TopProductDto>> GetTopSellingProducts()
    {
        return await _productsRepository.GetTopSellingProductAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _productsRepository.DeleteAsync(id);
    }

    public async Task<List<ProductEntity>> GetByIdsAsync(List<int> ids)
    {
        return await _productsRepository.GetByIdsAsync(ids);
    }
}
