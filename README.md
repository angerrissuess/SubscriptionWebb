# 📦 SubscriptionWeb — Система управления подписками

![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169e1?style=for-the-badge&logo=postgresql&logoColor=white)
![xUnit](https://img.shields.io/badge/Tests-14%20Passed-brightgreen?style=for-the-badge)

**SubscriptionWeb** — это веб-приложение на базе **ASP.NET Core MVC**, созданное для удобного контроля за вашими платными подписками и сервисами. Проект обеспечивает полную изоляцию данных: каждый пользователь управляет только своими записями.

---

## ✨ Основные возможности

*   **🔒 Приватность**: Жесткая фильтрация данных по `UserId`. Никто не увидит ваши подписки.
*   **📂 Сервисы**: Добавление любых платформ (Spotify, Netflix, YouTube и др.).
*   **📊 Гибкие тарифы**: Настройка цены, валюты и дат списания.
*   **🦾 Надежность**: Код покрыт тестами (Unit & Integration), что гарантирует стабильность.

---

## 🛠 Технологический стек

*   **Ядро**: C# / .NET 8
*   **База данных**: PostgreSQL
*   **ORM**: Entity Framework Core
*   **Тесты**: xUnit, Moq, InMemory Database

---

## 🚀 Инструкция по запуску

### 1. Подготовка
Убедитесь, что у вас установлен **.NET 8 SDK** и запущен **PostgreSQL**.

# Если нужно пересоздать решение и связи проектов
dotnet new sln -n SubscriptionSystem
dotnet sln add SubscriptionWeb/SubscriptionWeb.csproj
dotnet sln add SubscriptionWeb.Tests/SubscriptionWeb.Tests.csproj
dotnet add SubscriptionWeb.Tests/SubscriptionWeb.Tests.csproj reference SubscriptionWeb/SubscriptionWeb.csproj

### 2. Клонирование и настройка
git clone [https://github.com/angerrissuess/SubscriptionWebb.git]
cd SubscriptionWeb

 3. Настройка БД

Откройте SubscriptionWeb/appsettings.json и впишите свой пароль от Postgres:
JSON

"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=subscription_db;Username=postgres;Password=ВАШ_ПАРОЛЬ"
}
4. Миграции и запуск
Bash

# Создать таблицы
dotnet ef database update

# Запустить проект
dotnet run --project SubscriptionWeb

🧪 ТестированиеВ проекте настроено 14 автоматизированных тестов.
Они проверяют всё: от логики контроллеров до корректности запросов в базу данных.  
Запуск всех тестов:
Bashdotnet test

🏗 Архитектура

Проект использует паттерн Repository, что делает код чистым и удобным для расширения:

    PlanRepository / ServiceRepository — отвечают за работу с данными.

    AppDbContext — описывает связи (Cascade Delete) и индексы (Unique Email).
