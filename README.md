# Restaurant Orders Platform

Платформа для поиска ресторанов, бронирования столиков, заказов, отзывов и избранного — **модульный монолит**, ориентированный на production.

Стек: **.NET 8**, **Domain-Driven Design**, **Clean Architecture**, **маршрутизация через middleware** — без MVC Controllers.

Решение: `RestaurantOrdersPlatform.sln`

Веб-интерфейс на **русском языке**.

<div>
        <img width="1871" height="986" alt="image" src="https://github.com/user-attachments/assets/2c4cddc6-db99-4404-9d34-246312fcf2a9" />

</div>

---

## Возможности

### Гость / зарегистрированный пользователь

| Возможность | Описание |
|-------------|----------|
| **Каталог ресторанов** | Поиск и фильтрация по тексту, кухне, городу, ценовой категории, рейтингу, особенностям, «открыто сейчас», доступности бронирования |
| **Избранные места** | Подборка лучших ресторанов на главной странице |
| **Страница ресторана** | Полный профиль: адрес, контакты, часы работы, меню, фото, рейтинг, особенности |
| **Бронирования** | Забронировать стол (дата/время, число гостей, комментарий); просмотр и отмена своих броней |
| **Заказы** | Оформить заказ из меню; история заказов; отмена ожидающих заказов |
| **Отзывы** | Оставить оценку и комментарий к ресторану |
| **Избранное** | Добавлять и удалять любимые рестораны |
| **Аутентификация** | Регистрация, вход, выход, просмотр профиля (cookie-сессия) |

### Администратор

| Возможность | Описание |
|-------------|----------|
| **Управление ресторанами** | Создание, обновление, окончательное закрытие |
| **Жизненный цикл** | Публикация / снятие с публикации (правила готовности enforced в домене) |
| **Меню** | Добавление позиций в меню ресторана |
| **Заказы** | Смена статуса: подтвердить → готовится → готов → завершён |
| **Бронирования** | Подтверждение или отмена |
| **Модерация отзывов** | Публикация, отклонение, скрытие |
| **Статистика** | Сводка: рестораны, пользователи, заказы, бронирования, отзывы |

### Веб-интерфейс (Tailwind + Vanilla JS)

| Страница | Назначение |
|----------|------------|
| `/` | Главная — поиск и сетка ресторанов |
| `/restaurant.html?slug=...` | Детали ресторана, меню, избранное |
| `/login.html` | Вход |
| `/admin.html` | Панель администратора |

Документация API: `/swagger` · Проверка здоровья: `/health`

---

## Быстрый старт

```powershell
dotnet build RestaurantOrdersPlatform.sln
dotnet test RestaurantOrdersPlatform.sln
dotnet run --project src/RestaurantOrders.Web
```

Приложение откроется на `http://localhost:5000` (или порту из `launchSettings.json`).

**База данных:** по умолчанию SQLite (`restaurant.db` создаётся при первом запуске).  
Для MySQL задайте `ConnectionStrings__DefaultConnection` со строкой, содержащей `Server=` или `Host=`.

**Тестовые аккаунты:**

| Email | Пароль | Роль |
|-------|--------|------|
| `admin@restaurant.local` | `Admin123!` | Admin |
| `user@restaurant.local` | `User123!` | User |

**Демо-рестораны:** Кедр и Ржаной, Шафрановый дворик, Дом Лимона, Стол у огня, Дача-бранч, Синее течение — у каждого есть меню, часы работы и статус «опубликован».

> Чтобы пересоздать демо-данные после изменения сида, удалите `restaurant.db` в каталоге Web-проекта и перезапустите приложение.

---

## Обзор архитектуры

### Поток HTTP-запроса

```
HTTP-клиент (браузер / Postman)
        │
        ▼
┌───────────────────────────────────────┐
│  Web-слой (RestaurantOrders.Web)      │
│                                       │
│  ExceptionHandlingMiddleware          │
│  CorrelationIdMiddleware              │
│  RequestLoggingMiddleware             │
│  Authentication / Authorization       │
│  ApiRoutingMiddleware  ← без Controllers│
│  Статика (Tailwind frontend)          │
└───────────────┬───────────────────────┘
                │  bind request → handler
                ▼
┌───────────────────────────────────────┐
│  Application-слой                     │
│                                       │
│  Commands / Queries (CQRS)            │
│  Use-case handlers                    │
│  Порты репозиториев (интерфейсы)      │
│  Result<T> / таксономия Error         │
└───────────────┬───────────────────────┘
                │  оркестрация домена + портов
                ▼
┌───────────────────────────────────────┐
│  Domain-слой                          │
│                                       │
│  Aggregates, Entities, Value Objects  │
│  Domain Events, инварианты            │
│  DomainException                      │
└───────────────┬───────────────────────┘
                │  реализуется в
                ▼
┌───────────────────────────────────────┐
│  Infrastructure-слой                  │
│                                       │
│  EF Core + SQLite/MySQL               │
│  Repositories, Read Store             │
│  ASP.NET Identity (cookie auth)       │
│  Database seeding                     │
└───────────────────────────────────────┘
```

**Ключевой принцип:** middleware — это HTTP-адаптер. Он разбирает маршруты, привязывает DTO, вызывает handlers из Application и мапит `Result<T>` в HTTP. Бизнес-правил в middleware **нет**.

---

## Структура решения

```
src/
├── RestaurantOrders.Domain/          # Чистый домен — без зависимостей от инфраструктуры
│   ├── Common/                       # Entity, AggregateRoot, ValueObject, Money, Rating…
│   ├── Restaurants/                  # Агрегат Restaurant, MenuItem, OpeningHours…
│   ├── Orders/                       # Агрегат Order, OrderLine, жизненный цикл
│   ├── Reservations/                 # Агрегат Reservation, переходы статусов
│   ├── Reviews/                      # Агрегат Review, модерация
│   ├── Favorites/                    # Агрегат Favorite (уникальность User + Restaurant)
│   └── Users/                        # UserProfile, константы Roles, Permissions
│
├── RestaurantOrders.Application/     # Use cases — зависит только от Domain
│   ├── Abstractions/                 # IRestaurantRepository, IUnitOfWork, IRestaurantReadStore…
│   ├── Restaurants/                  # Commands + Queries + Handlers
│   ├── Orders/
│   ├── Reservations/
│   ├── Reviews/
│   ├── Favorites/
│   └── Users/
│
├── RestaurantOrders.Infrastructure/  # Технические детали
│   ├── Persistence/                  # AppDbContext, EF configurations, repositories
│   ├── Authentication/               # ApplicationUser (Identity)
│   └── DatabaseSeeder.cs
│
└── RestaurantOrders.Web/             # HTTP + frontend
    ├── Middleware/                   # ApiRoutingMiddleware, logging, errors
    └── wwwroot/                      # Tailwind UI, модульный JS (api/, pages/, components/)

tests/
├── RestaurantOrders.Domain.UnitTests/       # Инварианты агрегатов, правила жизненного цикла
├── RestaurantOrders.Architecture.Tests/     # Правила зависимостей (NetArchTest)
└── RestaurantOrders.Integration.Tests/      # Smoke-тест HTTP → middleware → БД
```

---

## Доменная модель

### Bounded contexts

| Контекст | Aggregate Root | Примечания |
|----------|----------------|------------|
| **Каталог** | `Restaurant` | Профиль, меню, часы работы, жизненный цикл (Draft → Published → Closed) |
| **Заказы** | `Order` | Позиции со снимком цены, workflow статусов |
| **Бронирования** | `Reservation` | Правила с учётом вместимости |
| **Отзывы** | `Review` | Модерация: Pending → Published / Rejected / Hidden |
| **Избранное** | `Favorite` | Уникальная пара (UserId, RestaurantId) |
| **Identity** | `UserProfile` | Доменный профиль, связанный с пользователем ASP.NET Identity |

### Value Objects (примеры)

`RestaurantId`, `UserId`, `OrderId`, `Money`, `EmailAddress`, `PhoneNumber`, `Rating`, `Address`, `RestaurantName`, `RestaurantSlug`, `OpeningHours`

### Domain events (примеры)

`RestaurantPublished`, `RestaurantClosed`, `OrderPlaced`, `OrderCancelled`, `ReservationCreated`, `ReviewPublished`

События живут в Domain. Нет связи с MediatR, EF Core или ASP.NET.

### Бизнес-правила в Domain

- Ресторан **нельзя опубликовать** без адреса, контактов, описания, кухни и хотя бы одной позиции меню
- **Закрытый** ресторан не принимает бронирования
- **Отменённое** бронирование нельзя подтвердить
- **Завершённый** заказ нельзя изменить
- Цены в строках заказа **фиксируются** на момент оформления
- Дубликаты в избранном отклоняются на уровне Application; Domain защищает инварианты агрегата

---

## Application-слой (CQRS)

Логическое разделение **Command / Query** без MediatR и generic repository:

```
CreateRestaurantCommand  → CreateRestaurantHandler  → IRestaurantRepository
SearchRestaurantsQuery   → SearchRestaurantsHandler → IRestaurantReadStore
```

Handlers:
- Координируют поведение домена
- Используют **порты** репозиториев (интерфейсы в `Application.Abstractions`)
- Управляют транзакцией через `IUnitOfWork` (`AppDbContext.SaveChangesAsync`)
- Возвращают `Result` / `Result<T>` с типизированным `Error` (Validation, NotFound, Conflict, BusinessRule…)

**Чтение vs запись:** `IRestaurantReadStore` обслуживает поиск, списки и проекции деталей; запись идёт через репозитории агрегатов.

---

## HTTP-слой — middleware, не Controllers

`ApiRoutingMiddleware` сопоставляет пути `/api/v1/*` и вызывает handlers из Application:

```
GET  /api/v1/restaurants                    → поиск (фильтры, пагинация)
GET  /api/v1/restaurants/featured           → избранная подборка
GET  /api/v1/restaurants/{id|slug}          → детали
POST /api/v1/restaurants                    → создать (Admin)
PUT  /api/v1/restaurants/{id}               → обновить (Admin)
POST /api/v1/restaurants/{id}/publish       → опубликовать (Admin)
POST /api/v1/restaurants/{id}/menu          → добавить позицию меню (Admin)

POST /api/v1/auth/login | register | logout
GET  /api/v1/auth/me

POST /api/v1/reservations
GET  /api/v1/reservations
POST /api/v1/reservations/{id}/confirm|cancel

POST /api/v1/orders
GET  /api/v1/orders
POST /api/v1/orders/{id}/cancel|confirm|prepare|ready|complete

POST /api/v1/reviews
POST /api/v1/reviews/{id}/publish|reject|hide

GET  /api/v1/favorites
POST /api/v1/favorites/{restaurantId}
DELETE /api/v1/favorites/{restaurantId}

GET  /api/v1/admin/stats                    → панель администратора
```

Ошибки оформляются по **RFC 7807 Problem Details** с полями `code`, `traceId` и HTTP-статусом из `ErrorType`. Сообщения для пользователя — на русском.

Cross-cutting middleware:

| Middleware | Ответственность |
|------------|-----------------|
| `ExceptionHandlingMiddleware` | Необработанные исключения → Problem Details |
| `CorrelationIdMiddleware` | Заголовок `X-Correlation-ID`, трассировка |
| `RequestLoggingMiddleware` | Структурированное логирование запросов |

---

## Безопасность

- **Аутентификация:** ASP.NET Identity с **cookie-сессиями** (удобно для same-origin frontend)
- **Авторизация:** Policy-based — `Permissions.RestaurantManage`, `Permissions.ReviewModerate`, роль `Admin` и др.
- **Пароль:** минимум 8 символов, обязательны верхний/нижний регистр и цифра
- API возвращает `401` / `403` (без редиректов) для неавторизованных запросов

Роли в Domain: `Admin`, `User`, `RestaurantOwner`, `Moderator`, `Manager`.

---

## Персистентность

- **EF Core 8** с Fluent API в Infrastructure (в Domain нет EF-атрибутов)
- **SQLite** по умолчанию; **MySQL** (Pomelo), если строка подключения указывает на MySQL
- Индексы: slug (unique), favorites (UserId + RestaurantId), reservations по ресторану/дате
- JSON-колонки для типов кухни, особенностей, URL фото
- Owned entities: Address, Contacts, MenuItem, OrderLine, OpeningHours
- Optimistic concurrency через `UpdatedAtUtc` на ключевых агрегатах

---

## Тестирование

| Слой | Что проверяется |
|------|-----------------|
| **Domain unit tests** | Правила публикации, суммы заказа, переходы бронирований, границы рейтинга, уникальность избранного |
| **Architecture tests** | Domain ↛ Application/Infrastructure/Web; Application ↛ Infrastructure/Web |
| **Integration tests** | Полный HTTP-пайплайн через middleware до БД |

```powershell
dotnet test RestaurantOrdersPlatform.sln
# 12 тестов: 9 domain + 2 architecture + 1 integration
```

---

## Технологии

| Компонент | Технология |
|-----------|------------|
| Runtime | .NET 8 |
| HTTP | ASP.NET Core Middleware (без Controllers) |
| Domain | DDD — Aggregates, Value Objects, Domain Events |
| Application | CQRS handlers, Result pattern |
| ORM | Entity Framework Core 8 |
| БД | SQLite (по умолчанию) / MySQL |
| Auth | ASP.NET Identity (cookie) |
| API docs | Swagger / OpenAPI |
| Frontend | HTML, Tailwind CSS (CDN), Vanilla JS modules, UI на русском |
| Tests | xUnit, FluentAssertions, NetArchTest, WebApplicationFactory |

---

## Архитектурные решения

| Решение | Обоснование |
|---------|-------------|
| **Middleware вместо Controllers** | Явный контроль HTTP-пайплайна; бизнес-логика остаётся в Application/Domain |
| **Без MediatR** | Handlers регистрируются в DI напрямую — меньше магии, проще трассировка |
| **Без generic `IRepository<T>`** | Репозитории отражают границы агрегатов и реальные сценарии |
| **Отдельный read store** | Поиск и списки не загружают полные агрегаты без необходимости |
| **Cookie auth вместо JWT** | Same-origin HTML frontend; проще модель сессий для монолита |
| **Модульный монолит** | Один деплой; границы слоёв проверяются architecture-тестами |
| **Domain events без event bus** | События на агрегатах; готовность к outbox/handlers позже без лишней сложности сейчас |
