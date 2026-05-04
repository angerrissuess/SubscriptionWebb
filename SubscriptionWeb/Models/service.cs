using System.ComponentModel.DataAnnotations;

namespace SubscriptionWeb.Models
{
    public class Service
    {
        // Уникальный ИД сервиса
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Название сервиса обязательно!")]
        [StringLength(200, ErrorMessage = "Слишком длинное название")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Url(ErrorMessage = "Введите корректный URL-адрес сайта")]
        public string Website { get; set; } // URL-адрес сайта[cite: 1]

        public string Description { get; set; } // Краткое описание[cite: 1]

        // Навигационное свойство: у одного сервиса может быть много тарифов
        public ICollection<Plan> Plans { get; set; } 
        // Кто создал этот сервис
        public int? UserId { get; set; } 
        public User User { get; set; } 
    }
}