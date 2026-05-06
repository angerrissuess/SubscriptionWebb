using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionWeb.Data;
using SubscriptionWeb.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Linq;
using System;

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
        [ValidateAntiForgeryToken]
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
                    try
                    {
                        model.AvatarPath = await SaveAvatar(avatarFile);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Avatar", ex.Message);
                        return View(model);
                    }
                }

                // Хешируем пароль перед сохранением (PBKDF2)
                model.Password = HashPassword(model.Password);

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
        [ValidateAntiForgeryToken]
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
                try
                {
                    user.AvatarPath = await SaveAvatar(avatarFile);
                }
                catch (Exception ex)
                {
                    // Сохраняем сообщение об ошибке и возвращаемся в профиль
                    TempData["AvatarError"] = ex.Message;
                    return RedirectToAction("Profile");
                }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && VerifyPassword(password, user.Password))
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
            var claim = User.FindFirst("UserId");
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            return userId;
        }

        // Вынес логику сохранения файла в отдельный метод, чтобы не дублировать код
        private async Task<string> SaveAvatar(IFormFile file)
        {
            // Ограничения
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

            if (file.Length <= 0 || file.Length > maxFileSize)
                throw new InvalidOperationException("Файл слишком большой или пустой (макс. 5MB)");

            if (!allowedTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Недопустимый тип файла. Разрешены только изображения.");

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

        // --- Простая реализация PBKDF2 для хеширования паролей ---
        private static string HashPassword(string password)
        {
            const int iterations = 100000;
            var salt = RandomNumberGenerator.GetBytes(16);
            using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);
            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var parts = storedHash.Split('.');
                if (parts.Length != 3) return false;
                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var hash = Convert.FromBase64String(parts[2]);

                using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var computed = derive.GetBytes(hash.Length);
                return CryptographicOperations.FixedTimeEquals(computed, hash);
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}