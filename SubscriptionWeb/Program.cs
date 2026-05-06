using Microsoft.EntityFrameworkCore;
using SubscriptionWeb.Data;
using SubscriptionWeb.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;

// 1. СНАЧАЛА создаем builder
var builder = WebApplication.CreateBuilder(args);

// 2. ЗАТЕМ добавляем в него все настройки
builder.Services.AddControllersWithViews();
// Антифрод и логирование
builder.Services.AddAntiforgery();
builder.Services.AddLogging();

// Настраиваем куки (авторизацию)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Куда перекидывать, если не авторизован
    });

// Подключаем базу данных PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрируем репозитории
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();

// 3. СОБИРАЕМ ПРИЛОЖЕНИЕ (все builder.Services строго до этой строки)
var app = builder.Build();

// 4. НАСТРАИВАЕМ ПОВЕДЕНИЕ (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ЭТИ ДВЕ СТРОКИ ДОЛЖНЫ ИДТИ ИМЕННО В ТАКОМ ПОРЯДКЕ
app.UseAuthentication(); // Сначала узнаем, КТО пришел (Логин)
app.UseAuthorization();  // Затем проверяем, что ему МОЖНО делать (Права)

// Маршрут по умолчанию
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();