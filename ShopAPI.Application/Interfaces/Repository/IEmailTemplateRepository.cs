namespace ShopAPI.Interfaces.Repository
{
    public interface IEmailTemplateRepository
    {
        Task<TempleteEntity?> GetByTypeAsync(string type);
        Task<bool> ExistsAsync(string type);
        Task AddAsync(TempleteEntity template);
    }
}
