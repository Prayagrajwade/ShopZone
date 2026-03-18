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
    }
}
