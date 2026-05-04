using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionWeb.Data;
using SubscriptionWeb.Models;
using System.Security.Claims;

namespace SubscriptionWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        #region Регистрация
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User model, IFormFile? avatarFile)
        {
            if (ModelState.IsValid)
            {
                // Проверка на уникальность Email
                var userExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
                if (userExists)
                {
                    ModelState.AddModelError("Email", "Пользователь с такой почтой уже существует.");
                    return View(model);
                }

                // Логика сохранения аватара при регистрации
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    model.AvatarPath = await SaveAvatar(avatarFile);
                }

                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                await Authenticate(model);
                return RedirectToAction("Index", "Service");
            }
            return View(model);
        }
        #endregion

        #region Профиль и Дашборд
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            // Загружаем планы и сервисы для счетчика дней
            var userPlans = await _context.Plans
                .Include(p => p.Service)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            ViewBag.ServicesCount = await _context.Services.CountAsync(s => s.UserId == userId);
            ViewBag.PlansCount = userPlans.Count;
            ViewBag.UserPlans = userPlans;

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            var userId = GetUserId(); // Используем твой вспомогательный метод
            var user = await _context.Users.FindAsync(userId);

            if (user != null && avatarFile != null && avatarFile.Length > 0)
            {
                // 1. Удаляем старую картинку с диска
                if (!string.IsNullOrEmpty(user.AvatarPath))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                // 2. Сохраняем новую картинку (используем твой метод SaveAvatar)
                user.AvatarPath = await SaveAvatar(avatarFile);

                // 3. Сохраняем изменения в БД
                await _context.SaveChangesAsync();

                // 4. МАГИЯ: Обновляем куку авторизации прямо сейчас
                // Это перезапишет Claim "AvatarPath" новым значением
                await Authenticate(user);
            }

            // После редиректа браузер получит новую куку и отрисует актуальное фото в шапке
            return RedirectToAction("Profile");
        }
        #endregion

        #region Авторизация и Выход
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                await Authenticate(user);
                return RedirectToAction("Index", "Service");
            }

            ModelState.AddModelError("", "Неверный логин или пароль");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Вспомогательные методы
        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.FullName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("AvatarPath", user.AvatarPath ?? "")
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        }

        // Вынес логику сохранения файла в отдельный метод, чтобы не дублировать код
        private async Task<string> SaveAvatar(IFormFile file)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");

            // Создаем папку, если её нет
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/avatars/" + fileName;
        }
        #endregion
    }
}