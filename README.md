# Restaurant Orders Platform

Production-oriented **modular monolith** for restaurant discovery, reservations, ordering, reviews, and favorites.

Built on **.NET 8** with **Domain-Driven Design**, **Clean Architecture**, and **middleware-based HTTP routing** — without MVC Controllers.

Solution: `RestaurantOrdersPlatform.sln`

---

## What you can do

### Guest / registered user

| Capability | Description |
|------------|-------------|
| **Browse restaurants** | Search and filter by text, cuisine, city, price category, rating, features, open-now, reservation availability |
| **Featured listings** | Curated top restaurants on the home page |
| **Restaurant details** | Full profile: address, contacts, opening hours, menu, photos, rating, features |
| **Reservations** | Book a table (date/time, guest count, notes); view and cancel own reservations |
| **Orders** | Place an order from menu items; view order history; cancel pending orders |
| **Reviews** | Submit a rating and comment for a restaurant |
| **Favorites** | Save and remove favorite restaurants |
| **Authentication** | Register, login, logout, view current profile (cookie-based session) |

### Administrator

| Capability | Description |
|------------|-------------|
| **Restaurant management** | Create, update, permanently close restaurants |
| **Lifecycle** | Publish / unpublish restaurants (domain-enforced readiness rules) |
| **Menu management** | Add menu items to a restaurant |
| **Order workflow** | Advance order status: confirm → prepare → ready → complete |
| **Reservation workflow** | Confirm or cancel reservations |
| **Review moderation** | Publish, reject, or hide reviews |
| **Dashboard stats** | Overview counts: restaurants, users, orders, reservations, reviews |

### Web UI (Tailwind + Vanilla JS)

| Page | Purpose |
|------|---------|
| `/` | Home — hero search, restaurant grid |
| `/restaurant.html?slug=...` | Restaurant details, menu, favorites |
| `/login.html` | Sign in |
| `/admin.html` | Admin dashboard stats |

API documentation: `/swagger` · Health check: `/health`

---

## Quick start

```powershell
dotnet build RestaurantOrdersPlatform.sln
dotnet test RestaurantOrdersPlatform.sln
dotnet run --project src/RestaurantOrders.Web
```

**Database:** SQLite by default (`restaurant.db` created on first run).  
For MySQL, set `ConnectionStrings__DefaultConnection` with `Server=` or `Host=`.

**Seeded accounts:**

| Email | Password | Role |
|-------|----------|------|
| `admin@restaurant.local` | `Admin123!` | Admin |
| `user@restaurant.local` | `User123!` | User |

**Seeded restaurants:** Juniper & Rye, Saffron Courtyard, Casa Limone, Ember Table, Dacha Brunch, Blue Current — each with menu, opening hours, and published status.

---

## Architecture overview

### High-level request flow

```
HTTP Client (browser / Postman)
        │
        ▼
┌───────────────────────────────────────┐
│  Web Layer (RestaurantOrders.Web)     │
│                                       │
│  ExceptionHandlingMiddleware          │
│  CorrelationIdMiddleware              │
│  RequestLoggingMiddleware             │
│  Authentication / Authorization       │
│  ApiRoutingMiddleware  ← no Controllers│
│  Static files (Tailwind frontend)     │
└───────────────┬───────────────────────┘
                │  bind request → dispatch handler
                ▼
┌───────────────────────────────────────┐
│  Application Layer                    │
│                                       │
│  Commands / Queries (CQRS)            │
│  Use-case handlers                    │
│  Repository ports (interfaces)        │
│  Result<T> / Error taxonomy           │
└───────────────┬───────────────────────┘
                │  orchestrate domain + ports
                ▼
┌───────────────────────────────────────┐
│  Domain Layer                         │
│                                       │
│  Aggregates, Entities, Value Objects  │
│  Domain Events, Invariants            │
│  DomainException                      │
└───────────────┬───────────────────────┘
                │  implemented by
                ▼
┌───────────────────────────────────────┐
│  Infrastructure Layer                 │
│                                       │
│  EF Core + SQLite/MySQL               │
│  Repositories, Read Store             │
│  ASP.NET Identity (cookie auth)       │
│  Database seeding                     │
└───────────────────────────────────────┘
```

**Key principle:** middleware is the HTTP adapter. It parses routes, binds DTOs, calls Application handlers, and maps `Result<T>` to HTTP — it does **not** contain business rules.

---

## Solution structure

```
src/
├── RestaurantOrders.Domain/          # Pure domain — zero infrastructure deps
│   ├── Common/                       # Entity, AggregateRoot, ValueObject, Money, Rating…
│   ├── Restaurants/                  # Restaurant aggregate, MenuItem, OpeningHours…
│   ├── Orders/                       # Order aggregate, OrderLine, lifecycle
│   ├── Reservations/                 # Reservation aggregate, status transitions
│   ├── Reviews/                      # Review aggregate, moderation lifecycle
│   ├── Favorites/                    # Favorite aggregate (User + Restaurant uniqueness)
│   └── Users/                        # UserProfile, Roles, Permissions constants
│
├── RestaurantOrders.Application/     # Use cases — depends on Domain only
│   ├── Abstractions/                 # IRestaurantRepository, IUnitOfWork, IRestaurantReadStore…
│   ├── Restaurants/                  # Commands + Queries + Handlers
│   ├── Orders/
│   ├── Reservations/
│   ├── Reviews/
│   ├── Favorites/
│   └── Users/
│
├── RestaurantOrders.Infrastructure/  # Technical details
│   ├── Persistence/                  # AppDbContext, EF configurations, repositories
│   ├── Authentication/               # ApplicationUser (Identity)
│   └── DatabaseSeeder.cs
│
└── RestaurantOrders.Web/             # HTTP + frontend
    ├── Middleware/                   # ApiRoutingMiddleware, logging, errors
    └── wwwroot/                      # Tailwind UI, modular JS (api/, pages/, components/)

tests/
├── RestaurantOrders.Domain.UnitTests/       # Aggregate invariants, lifecycle rules
├── RestaurantOrders.Architecture.Tests/     # NetArchTest dependency rules
└── RestaurantOrders.Integration.Tests/      # HTTP → middleware → DB smoke test
```

---

## Domain model

### Bounded contexts

| Context | Aggregate Root | Notes |
|---------|---------------|-------|
| **Catalog** | `Restaurant` | Profile, menu items, opening hours, lifecycle (Draft → Published → Closed) |
| **Ordering** | `Order` | Lines with price snapshots, status workflow |
| **Reservations** | `Reservation` | Capacity-aware booking rules |
| **Reviews** | `Review` | Moderation: Pending → Published / Rejected / Hidden |
| **Favorites** | `Favorite` | Unique (UserId, RestaurantId) pair |
| **Identity** | `UserProfile` | Domain profile linked to ASP.NET Identity user |

### Value Objects (examples)

`RestaurantId`, `UserId`, `OrderId`, `Money`, `EmailAddress`, `PhoneNumber`, `Rating`, `Address`, `RestaurantName`, `RestaurantSlug`, `OpeningHours`

### Domain events (examples)

`RestaurantPublished`, `RestaurantClosed`, `OrderPlaced`, `OrderCancelled`, `ReservationCreated`, `ReviewPublished`

Events live in Domain. No coupling to MediatR, EF Core, or ASP.NET.

### Business rules enforced in Domain

- Restaurant cannot be **published** without address, contacts, description, cuisine, and at least one menu item
- **Closed** restaurant cannot accept reservations
- **Cancelled** reservation cannot be confirmed
- **Completed** order cannot be modified
- Order line prices are **snapshotted** at order time
- Duplicate favorites are rejected at Application layer; Domain protects aggregate invariants

---

## Application layer (CQRS)

Logical **Command / Query** separation without MediatR or a generic repository:

```
CreateRestaurantCommand  → CreateRestaurantHandler  → IRestaurantRepository
SearchRestaurantsQuery   → SearchRestaurantsHandler → IRestaurantReadStore
```

Handlers:
- Coordinate domain behavior
- Use repository **ports** (interfaces in `Application.Abstractions`)
- Manage transaction boundary via `IUnitOfWork` (backed by `AppDbContext.SaveChangesAsync`)
- Return `Result` / `Result<T>` with typed `Error` (Validation, NotFound, Conflict, BusinessRule…)

**Read vs write:** `IRestaurantReadStore` serves search/listing/details projections; write path goes through aggregate repositories.

---

## HTTP layer — middleware, not Controllers

`ApiRoutingMiddleware` matches `/api/v1/*` paths and dispatches to Application handlers:

```
GET  /api/v1/restaurants                    → search (filters, pagination)
GET  /api/v1/restaurants/featured           → featured list
GET  /api/v1/restaurants/{id|slug}          → details
POST /api/v1/restaurants                    → create (Admin)
PUT  /api/v1/restaurants/{id}               → update (Admin)
POST /api/v1/restaurants/{id}/publish       → publish (Admin)
POST /api/v1/restaurants/{id}/menu          → add menu item (Admin)

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

GET  /api/v1/admin/stats                    → Admin dashboard
```

Errors follow **RFC 7807 Problem Details** with `code`, `traceId`, and HTTP status mapped from `ErrorType`.

Cross-cutting middleware:

| Middleware | Responsibility |
|------------|----------------|
| `ExceptionHandlingMiddleware` | Unhandled exceptions → Problem Details |
| `CorrelationIdMiddleware` | `X-Correlation-ID` header, trace propagation |
| `RequestLoggingMiddleware` | Structured request/response logging |

---

## Security

- **Authentication:** ASP.NET Identity with **cookie sessions** (same-origin friendly for SPA-like frontend)
- **Authorization:** Policy-based — `Permissions.RestaurantManage`, `Permissions.ReviewModerate`, role `Admin`, etc.
- **Password policy:** min 8 chars, upper/lower/digit required
- API returns `401` / `403` (not redirects) for unauthenticated API calls

Extensible roles defined in Domain: `Admin`, `User`, `RestaurantOwner`, `Moderator`, `Manager`.

---

## Persistence

- **EF Core 8** with Fluent API configurations in Infrastructure (Domain has no EF attributes)
- **SQLite** default; **MySQL** (Pomelo) when connection string indicates it
- Indexes on slug (unique), favorites (UserId + RestaurantId), reservations by restaurant/date
- JSON columns for cuisine types, features, photo URLs
- Owned entities for Address, Contacts, MenuItem, OrderLine, OpeningHours
- Optimistic concurrency via `UpdatedAtUtc` on key aggregates

---

## Testing strategy

| Layer | What is tested |
|-------|----------------|
| **Domain unit tests** | Publish rules, order totals, reservation transitions, rating bounds, favorite uniqueness |
| **Architecture tests** | Domain ↛ Application/Infrastructure/Web; Application ↛ Infrastructure/Web |
| **Integration tests** | Full HTTP pipeline through middleware to in-memory/SQLite database |

```powershell
dotnet test RestaurantOrdersPlatform.sln
# 12 tests: 9 domain + 2 architecture + 1 integration
```

---

## Tech stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 8 |
| HTTP | ASP.NET Core Middleware (no Controllers) |
| Domain | DDD — Aggregates, Value Objects, Domain Events |
| Application | CQRS handlers, Result pattern |
| ORM | Entity Framework Core 8 |
| Database | SQLite (default) / MySQL |
| Auth | ASP.NET Identity (cookie) |
| API docs | Swagger / OpenAPI |
| Frontend | HTML, Tailwind CSS (CDN), Vanilla JS modules |
| Tests | xUnit, FluentAssertions, NetArchTest, WebApplicationFactory |

---

## Design decisions (for review)

| Decision | Rationale |
|----------|-----------|
| **Middleware instead of Controllers** | Explicit HTTP pipeline control; business logic stays in Application/Domain |
| **No MediatR** | Handlers registered in DI directly — less magic, easier to trace |
| **No generic `IRepository<T>`** | Repositories reflect aggregate boundaries and real use cases |
| **Separate read store** | Search/listing queries don't load full aggregates unnecessarily |
| **Cookie auth over JWT** | Same-origin HTML frontend; simpler session model for demo/production monolith |
| **Modular monolith** | Single deployable unit; clear layer boundaries enforced by architecture tests |
| **Domain events without event bus** | Events raised on aggregates; ready for outbox/handlers later without premature complexity |
