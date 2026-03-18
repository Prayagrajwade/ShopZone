namespace ShopAPI.Interfaces.Repository
{
    public interface IProductsRepository
    {
        Task<List<TopProductDto>> GetTopSellingProductAsync();
    }
}
