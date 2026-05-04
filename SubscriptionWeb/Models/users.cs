using System.ComponentModel.DataAnnotations;

namespace SubscriptionWeb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } 

        [Required(ErrorMessage = "Введите имя пользователя")]
        [Display(Name = "Имя и Фамилия")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Дата регистрации")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Придумайте пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Аватар")]
        public string? AvatarPath { get; set; }
        // В будущем здесь появится связь с таблицей Подписок
    }
}