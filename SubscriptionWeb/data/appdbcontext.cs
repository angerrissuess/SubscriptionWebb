using Microsoft.EntityFrameworkCore;
using SubscriptionWeb.Models;

namespace SubscriptionWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Service> Services { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Уникальный Email для пользователей
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 2. Связь Один-ко-многим: Один Сервис -> Много Планов
            modelBuilder.Entity<Plan>()
                .HasOne(p => p.Service)
                .WithMany(s => s.Plans)
                .HasForeignKey(p => p.ServiceId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении сервиса удаляются его планы

            // 3. Связь Пользователь -> Сервисы
            modelBuilder.Entity<Service>()
                .HasOne(s => s.User)
                .WithMany() // У пользователя может не быть коллекции сервисов в модели, это нормально
                .HasForeignKey(s => s.UserId);
        }
    }
}