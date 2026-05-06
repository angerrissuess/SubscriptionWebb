using System.ComponentModel.DataAnnotations;

namespace SubscriptionWeb.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Название сервиса обязательно!")]
        [StringLength(200, ErrorMessage = "Слишком длинное название")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Url(ErrorMessage = "Введите корректный URL-адрес сайта")]
        [Display(Name = "Вебсайт")]
        public string Website { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Инициализируем коллекцию, чтобы не получить NRE при обращении
        public ICollection<Plan> Plans { get; set; } = new List<Plan>();
        
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}