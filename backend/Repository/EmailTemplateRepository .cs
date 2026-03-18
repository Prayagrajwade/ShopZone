using ShopAPI.Interfaces.Repository;
using ShopAPI.Migrations;

namespace ShopAPI.Repository
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly AppDbContext _db;

        public EmailTemplateRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TempleteModel?> GetByTypeAsync(string type)
        {
            return await _db.EmailTemplete
                .FirstOrDefaultAsync(t => t.Type == type);
        }

        public async Task<bool> ExistsAsync(string type)
        {
            return await _db.EmailTemplete
                .AnyAsync(t => t.Type == type);
        }

        public async Task AddAsync(TempleteModel template)
        {
            _db.EmailTemplete.Add(template);
            await _db.SaveChangesAsync();
        }
    }
}
