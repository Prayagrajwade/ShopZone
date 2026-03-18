using ShopAPI.Migrations;

namespace ShopAPI.Interfaces.Repository
{
    public interface IEmailTemplateRepository
    {
        Task<TempleteModel?> GetByTypeAsync(string type);
        Task<bool> ExistsAsync(string type);
        Task AddAsync(TempleteModel template);
    }
}
