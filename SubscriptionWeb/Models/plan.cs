using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubscriptionWeb.Models
{
    public class Plan
    {
        [Key]
        public int PlanId { get; set; }

        [Required(ErrorMessage = "Название тарифа обязательно")]
        [Display(Name = "Название тарифа")]
        public string PlanName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите стоимость")]
        [Display(Name = "Стоимость (руб)")]
        public decimal Price { get; set; } // Базовая стоимость

        [Display(Name = "Период оплаты (в днях)")]
        public int BillingDays { get; set; } // Период оплаты в днях (позволяет отличать ежемесячные от годовых)

        [Display(Name = "Пробный период (в днях)")]
        public int TrialDays { get; set; } // Длительность пробного периода

        [Display(Name = "Лимит пользователей")]
        public int MaxUsers { get; set; } // Ограничение на количество пользователей

        // --- Связь с таблицей Сервисов ---

        [Required]
        public int ServiceId { get; set; } // Внешний ключ (связь с сервисом)

        [ForeignKey("ServiceId")]
        public Service? Service { get; set; } // Навигационное свойство (добавлен ? для избежания ошибок валидации при создании)

        // --- Связь с таблицей Пользователей (Аккаунтов) ---

        public int? UserId { get; set; } // Внешний ключ. Nullable, на случай если есть общие системные планы

        [ForeignKey("UserId")]
        public User? User { get; set; } // Навигационное свойство (добавлен ? чтобы EF Core не требовал его при валидации формы)

        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
    }
}