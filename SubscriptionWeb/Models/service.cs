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
        public string Website { get; set; } 

        public string Description { get; set; } 

        public ICollection<Plan> Plans { get; set; } 
        
        public int? UserId { get; set; } 
        public User User { get; set; } 
    }
}