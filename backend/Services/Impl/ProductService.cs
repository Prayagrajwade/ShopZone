using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Services.Impl;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly IProductsRepository _productsRepository;

    public ProductService(AppDbContext db, IProductsRepository productsRepository){
        _db = db;
        _productsRepository = productsRepository;
    } 

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? category, string? search)
    {
        var query = _db.Products.AsQueryable().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search) || p.Description.Contains(search));

        return await query
            .OrderBy(p => p.Name)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _db.Products
            .Where(p => p.IsActive)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAdminAsync()
    {
        return await _db.Products
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name       = dto.Name,
            Description = dto.Description,
            Price      = dto.Price,
            Stock      = dto.Stock,
            Category   = dto.Category,
            ImageUrl   = dto.ImageUrl,
            IsActive   = true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return null;

        product.Name        = dto.Name;
        product.Description = dto.Description;
        product.Price       = dto.Price;
        product.Stock       = dto.Stock;
        product.Category    = dto.Category;
        product.ImageUrl    = dto.ImageUrl;
        product.IsActive    = dto.IsActive;

        await _db.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task<List<TopProductDto>> GetTopSellingProducts()
    {
        return await _productsRepository.GetTopSellingProductAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return false;

        product.IsActive = false;  
        await _db.SaveChangesAsync();
        return true;
    }

    private static ProductDto MapToDto(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.ImageUrl, p.IsActive);
}
