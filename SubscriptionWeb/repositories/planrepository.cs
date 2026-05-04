using Microsoft.EntityFrameworkCore;
using SubscriptionWeb.Data;
using SubscriptionWeb.Models;

namespace SubscriptionWeb.Repositories
{
    public class PlanRepository : IPlanRepository
    {
        private readonly AppDbContext _context;

        public PlanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Plan>> GetAllAsync(int userId)
        {
            // Фильтруем по UserId и подтягиваем данные о сервисе для отображения названия[cite: 9, 10]
            return await _context.Plans
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
        }

        public async Task<Plan> GetByIdAsync(int id, int userId)
        {
            // Ищем план по ID, проверяя, что он принадлежит текущему пользователю
            return await _context.Plans
                .Include(p => p.Service)
                .FirstOrDefaultAsync(p => p.PlanId == id && p.UserId == userId);
        }

        public async Task AddAsync(Plan plan)
        {
            await _context.Plans.AddAsync(plan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Plan plan)
        {
            _context.Plans.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            // Находим план, убеждаясь, что удаляем именно свою запись[cite: 10]
            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.PlanId == id && p.UserId == userId);

            if (plan != null)
            {
                _context.Plans.Remove(plan);
                await _context.SaveChangesAsync();
            }
        }
    }
}