using Microsoft.EntityFrameworkCore;
using SubscriptionWeb.Data;
using SubscriptionWeb.Models;
using SubscriptionWeb.Repositories;
using Xunit;

namespace SubscriptionWeb.Tests
{
    public class PlanRepositoryTests
    {
        // Метод для создания настроек контекста в памяти
        private DbContextOptions<AppDbContext> GetOptions(string dbName)
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByUserId()
        {
            // Arrange: Создаем уникальную БД для этого теста
            var options = GetOptions("GetAllAsync_FilterTest");
            using (var context = new AppDbContext(options))
            {
                // Добавляем данные разных пользователей
                context.Plans.AddRange(
                    new Plan { PlanId = 1, PlanName = "User1_Plan", UserId = 1 },
                    new Plan { PlanId = 2, PlanName = "User1_Plan2", UserId = 1 },
                    new Plan { PlanId = 3, PlanName = "User2_Plan", UserId = 2 }
                );
                await context.SaveChangesAsync();
            }

            // Act: Используем репозиторий для получения планов пользователя 1
            using (var context = new AppDbContext(options))
            {
                var repository = new PlanRepository(context);
                var results = await repository.GetAllAsync(1);

                // Assert: Должно вернуться 2 плана, и оба принадлежат UserId = 1
                Assert.Equal(2, results.Count());
                Assert.All(results, p => Assert.Equal(1, p.UserId));
                Assert.DoesNotContain(results, p => p.PlanId == 3);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_IfPlanBelongsToOtherUser()
        {
            // Arrange
            var options = GetOptions("GetById_SecurityTest");
            using (var context = new AppDbContext(options))
            {
                context.Plans.Add(new Plan { PlanId = 10, PlanName = "SecretPlan", UserId = 99 });
                await context.SaveChangesAsync();
            }

            // Act: Пытаемся получить план юзера 99, притворяясь юзером 1
            using (var context = new AppDbContext(options))
            {
                var repository = new PlanRepository(context);
                var result = await repository.GetByIdAsync(10, 1);

                // Assert: Репозиторий должен вернуть null, так как UserId не совпадает
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task AddAsync_ShouldActuallySaveToDatabase()
        {
            // Arrange
            var options = GetOptions("AddAsync_SaveTest");
            var newPlan = new Plan { PlanName = "Cloud Save", UserId = 1, Price = 500 };

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new PlanRepository(context);
                await repository.AddAsync(newPlan);
            }

            // Assert: Проверяем, что в базе реально появилась запись
            using (var context = new AppDbContext(options))
            {
                Assert.Equal(1, await context.Plans.CountAsync());
                var savedPlan = await context.Plans.FirstAsync();
                Assert.Equal("Cloud Save", savedPlan.PlanName);
            }
        }
    }
}