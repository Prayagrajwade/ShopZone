using ShopAPI.Interfaces.Repository;

namespace ShopAPI.Repository
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly AppDbContext _db;

        public ProductsRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<TopProductDto>> GetTopSellingProductAsync()
        {
            return await _db.Set<TopProductDto>()
                        .FromSqlRaw("EXEC GetTopSellingProducts")
                        .ToListAsync();
        }

        public async Task<List<Product>> GetByIdsAsync(List<int> ids)
        {
            return await _db.Products
                .Where(p => ids.Contains(p.Id) && p.IsActive)
                .ToListAsync();
        }
    }
}
