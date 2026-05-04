using SubscriptionWeb.Models;

namespace SubscriptionWeb.Repositories
{
    public interface IServiceRepository
    {
        // Получаем список только для конкретного пользователя
        Task<IEnumerable<Service>> GetAllAsync(int userId);

        Task AddAsync(Service service);

        // Поиск и удаление теперь тоже требуют проверки владельца
        Task<Service> GetByIdAsync(int id, int userId);

        Task UpdateAsync(Service service);

        Task DeleteAsync(int id, int userId);
    }
}