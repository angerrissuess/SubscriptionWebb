using Microsoft.EntityFrameworkCore;
using SubscriptionWeb.Data;
using SubscriptionWeb.Models;

namespace SubscriptionWeb.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        // Исправляем GetAllAsync: добавляем обязательный фильтр по UserId
        public async Task<IEnumerable<Service>> GetAllAsync(int userId)
        {
            return await _context.Services
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        // Исправляем DeleteAsync: учитываем целостность данных
        public async Task DeleteAsync(int id, int userId)
        {
            // Находим сервис, принадлежащий именно этому пользователю
            var service = await _context.Services
                .Include(s => s.Plans) // Подгружаем связанные планы для корректного удаления
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.UserId == userId);

            if (service != null)
            {
                // Если есть связанные планы, удаляем их (если не настроен каскад в БД)
                if (service.Plans != null && service.Plans.Any())
                {
                    _context.Plans.RemoveRange(service.Plans);
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task<Service?> GetByIdAsync(int id, int userId)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.UserId == userId);
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }
    }
}