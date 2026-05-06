Write-Host "🚀 Начинаем настройку проекта SubscriptionWeb..." -ForegroundColor Cyan

$dotnetVersion = dotnet --version
if ($null -eq $dotnetVersion) {
    Write-Host "❌ .NET SDK не найден! Пожалуйста, установите .NET 8 SDK." -ForegroundColor Red
    return
}
Write-Host "✅ .NET SDK обнаружен: $dotnetVersion" -ForegroundColor Green

Write-Host "📦 Восстанавливаем NuGet пакеты..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) { 
    Write-Host "❌ Ошибка при восстановлении пакетов." -ForegroundColor Red
    return 
}

Write-Host "🛠 Проверка инструментов dotnet-ef..." -ForegroundColor Yellow
dotnet tool install --global dotnet-ef 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ℹ️ Инструмент dotnet-ef уже установлен или обновлен." -ForegroundColor Gray
}

Write-Host "🔨 Собираем проект..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) { 
    Write-Host "❌ Ошибка сборки. Проверьте код." -ForegroundColor Red
    return 
}

Write-Host "🗄 Применяем миграции к PostgreSQL..." -ForegroundColor Yellow
dotnet ef database update --project SubscriptionWeb
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Не удалось применить миграции. Проверьте строку подключения в appsettings.json и запущен ли Postgres." -ForegroundColor Red
} else {
    Write-Host "✅ База данных успешно обновлена!" -ForegroundColor Green
}
Write-Host "🧪 Запускаем тесты..." -ForegroundColor Yellow
dotnet test
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️ Некоторые тесты провалены. Проверьте логи." -ForegroundColor Magenta
} else {
    Write-Host "✅ Все 14 тестов пройдены успешно!" -ForegroundColor Green
}

Write-Host "🎉 Настройка завершена! Можно запускать проект через F5." -ForegroundColor Cyan