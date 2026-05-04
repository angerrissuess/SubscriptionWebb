using SubscriptionWeb.Models;

namespace SubscriptionWeb.Repositories
{
    public interface IPlanRepository
    {
        // Получаем планы только конкретного пользователя[cite: 8]
        Task<IEnumerable<Plan>> GetAllAsync(int userId);

        // Поиск с проверкой владельца[cite: 8]
        Task<Plan> GetByIdAsync(int id, int userId);

        Task AddAsync(Plan plan);
        Task UpdateAsync(Plan plan);

        // Удаление с проверкой владельца[cite: 8]
        Task DeleteAsync(int id, int userId);
    }
}