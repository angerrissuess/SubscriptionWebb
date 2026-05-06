using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SubscriptionWeb.Models;
using SubscriptionWeb.Repositories;
using System.Security.Claims;

namespace SubscriptionWeb.Controllers
{
    [Authorize]
    public class PlanController : Controller
    {
        private readonly IPlanRepository _planRepository;
        private readonly IServiceRepository _serviceRepository;

        public PlanController(IPlanRepository planRepository, IServiceRepository serviceRepository)
        {
            _planRepository = planRepository;
            _serviceRepository = serviceRepository;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            return userId;
        }

        public async Task<IActionResult> Index()
        {
            // Получаем планы текущего пользователя
            var plans = await _planRepository.GetAllAsync(GetCurrentUserId());
            return View(plans);
        }

        public async Task<IActionResult> Create()
        {
            // Загружаем только сервисы этого пользователя для выпадающего списка
            var services = await _serviceRepository.GetAllAsync(GetCurrentUserId());
            ViewBag.Services = new SelectList(services, "ServiceId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plan plan)
        {
            plan.UserId = GetCurrentUserId();

            // КРИТИЧНО: Устанавливаем время в UTC для PostgreSQL
            plan.StartDate = DateTime.UtcNow;

            // Убираем навигационные свойства из валидации, так как они заполняются БД
            ModelState.Remove("Service");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                await _planRepository.AddAsync(plan);
                return RedirectToAction(nameof(Index));
            }

            var services = await _serviceRepository.GetAllAsync(GetCurrentUserId());
            ViewBag.Services = new SelectList(services, "ServiceId", "Name");
            return View(plan);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id, GetCurrentUserId());
            if (plan == null)
            {
                return NotFound();
            }

            var services = await _serviceRepository.GetAllAsync(GetCurrentUserId());
            ViewBag.Services = new SelectList(services, "ServiceId", "Name", plan.ServiceId);

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Plan plan)
        {
            plan.UserId = GetCurrentUserId();

            ModelState.Remove("Service");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                // При обновлении важно сохранить исходную дату начала, 
                // если она не передается из скрытого поля формы.
                await _planRepository.UpdateAsync(plan);
                return RedirectToAction(nameof(Index));
            }

            var services = await _serviceRepository.GetAllAsync(GetCurrentUserId());
            ViewBag.Services = new SelectList(services, "ServiceId", "Name", plan.ServiceId);
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id, GetCurrentUserId());
            if (plan == null)
            {
                return NotFound();
            }

            await _planRepository.DeleteAsync(id, GetCurrentUserId());
            return RedirectToAction(nameof(Index));
        }
    }
}