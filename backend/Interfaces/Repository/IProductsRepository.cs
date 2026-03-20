namespace ShopAPI.Interfaces.Repository
{
    public interface IProductsRepository
    {
        Task<List<TopProductDto>> GetTopSellingProductAsync();
        Task<List<Product>> GetByIdsAsync(List<int> ids);
    }
}
