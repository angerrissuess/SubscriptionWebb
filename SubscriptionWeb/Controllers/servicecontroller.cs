using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionWeb.Models;
using SubscriptionWeb.Repositories;
using System.Security.Claims;

namespace SubscriptionWeb.Controllers
{
    // [Authorize] гарантирует, что только вошедшие пользователи увидят этот контроллер
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly IServiceRepository _repository;

        public ServiceController(IServiceRepository repository)
        {
            _repository = repository;
        }

        // Вспомогательный метод для получения ID текущего юзера
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public async Task<IActionResult> Index()
        {
            // Передаем ID юзера, чтобы репозиторий отфильтровал только его записи
            var items = await _repository.GetAllAsync(GetCurrentUserId());
            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Service model)
        {
            // Принудительно привязываем новый сервис к текущему пользователю
            model.UserId = GetCurrentUserId();

            ModelState.Remove("Plans");
            ModelState.Remove("User"); // Убираем проверку навигационного свойства

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Ищем сервис, проверяя, что он принадлежит именно этому юзеру
            var service = await _repository.GetByIdAsync(id, GetCurrentUserId());
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Service model)
        {
            model.UserId = GetCurrentUserId();
            ModelState.Remove("Plans");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Удаляем только если ID сервиса и ID юзера совпали в базе
            await _repository.DeleteAsync(id, GetCurrentUserId());
            return RedirectToAction("Index");
        }
    }
}