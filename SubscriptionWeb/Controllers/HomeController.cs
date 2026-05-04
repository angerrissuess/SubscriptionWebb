using Microsoft.AspNetCore.Mvc;

namespace SubscriptionWeb.Controllers
{
    public class HomeController : Controller
    {
        // Этот метод отвечает за отображение лендинга (главной страницы)
        public IActionResult Index()
        {
            // Если пользователь авторизован, мы можем передать это во View
            // чтобы показать ему персонализированные кнопки
            return View();
        }

        // Страница "О проекте" или "Контакты" (опционально)
        public IActionResult Privacy()
        {
            return View();
        }
    }
}