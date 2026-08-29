using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using RestaurantOrders.Domain.Restaurants;

using RestaurantOrders.Domain.Users;

using RestaurantOrders.Infrastructure.Authentication;



namespace RestaurantOrders.Infrastructure.Persistence;



public sealed class DatabaseSeeder(

    AppDbContext db,

    UserManager<ApplicationUser> userManager,

    RoleManager<IdentityRole<Guid>> roleManager)

{

    public async Task InitializeAsync(CancellationToken ct = default)

    {

        await db.Database.EnsureCreatedAsync(ct);

        await EnsureRoleAsync(Roles.Admin);

        await EnsureRoleAsync(Roles.User);

        await EnsureUserAsync("admin@restaurant.local", "Admin123!", "Администратор платформы", Roles.Admin);

        await EnsureUserAsync("user@restaurant.local", "User123!", "Демо-гость", Roles.User);



        if (await db.Restaurants.AnyAsync(ct))

            return;



        var seeds = new[]

        {

            new Seed("Кедр и Ржаной", "juniper-and-rye", "Современная северная кухня у очага из дубовых дров.",

                "ул. Тверская, 12", "Москва", "+7 495 100-10-10", PriceCategory.Upscale,

                new[] { "Европейская", "Русская" }, new[] { "Стол шеф-повара", "Винная карта" },

                "https://images.unsplash.com/photo-1517248135467-4c7edcad34c5"),

            new Seed("Шафрановый дворик", "saffron-courtyard", "Блюда Шёлкового пути, ароматные специи и щедкие меню для компании.",

                "ул. Рубинштейна, 8", "Санкт-Петербург", "+7 812 200-20-20", PriceCategory.Moderate,

                new[] { "Грузинская", "Ближневосточная" }, new[] { "Терраса", "Для семей" },

                "https://images.unsplash.com/photo-1552566626-52f8b828add9"),

            new Seed("Дом Лимона", "casa-limone", "Солнечная итальянская кухня с домашней пастой и морепродуктами.",

                "ул. Рождественская, 21", "Нижний Новгород", "+7 831 300-30-30", PriceCategory.Moderate,

                new[] { "Итальянская", "Средиземноморская" }, new[] { "У воды", "Вегетарианские блюда" },

                "https://images.unsplash.com/photo-1555396273-367ea4eb4db5"),

            new Seed("Стол у огня", "ember-table", "Сезонные продукты и отборные стейки на открытом огне.",

                "пр. Красный, 4", "Новосибирск", "+7 383 400-40-40", PriceCategory.Upscale,

                new[] { "Стейк-хаус", "Современная" }, new[] { "Открытая кухня", "Приватный зал" },

                "https://images.unsplash.com/photo-1414235077428-338989a2e8c0"),

            new Seed("Дача-бранч", "dacha-brunch", "Уютные завтраки и бранчи в духе загородной дачи.",

                "ул. Ленина, 17", "Казань", "+7 843 500-50-50", PriceCategory.Budget,

                new[] { "Кафе", "Русская" }, new[] { "Можно с питомцами", "Весь день — завтраки" },

                "https://images.unsplash.com/photo-1533777857889-4be7c70b33f7"),

            new Seed("Синее течение", "blue-current", "Изысканные морепродукты, raw bar и освежающие прибрежные коктейли.",

                "ул. Морская, 30", "Сочи", "+7 862 600-60-60", PriceCategory.Luxury,

                new[] { "Морепродукты", "Японская" }, new[] { "Вид на море", "Raw bar" },

                "https://images.unsplash.com/photo-1515003197210-e0cd71810b5f")

        };



        foreach (var seed in seeds)

        {

            var restaurant = Restaurant.Create(seed.Name, seed.Slug);

            restaurant.UpdateDescription(seed.Description);

            restaurant.SetAddress(Address.Create(seed.Street, seed.City));

            restaurant.SetContacts(ContactInformation.Create(seed.Phone, $"{seed.Slug}@restaurant.local"));

            restaurant.SetPriceCategory(seed.Price);

            restaurant.SetCuisineTypes(seed.Cuisines);

            restaurant.SetFeatures(seed.Features);

            restaurant.SetPhotos(seed.Image, new[] { seed.Image });

            restaurant.ConfigureOptions(true, true, 80);

            restaurant.SetOpeningHours(Enum.GetValues<DayOfWeek>()

                .Select(day => OpeningHours.Open(day, new TimeOnly(10, 0), new TimeOnly(23, 0))));

            restaurant.AddMenuItem("Фирменная дегустационная тарелка", "Сезонная подборка от шеф-повара.",

                seed.Price is PriceCategory.Upscale or PriceCategory.Luxury ? 2400 : 950, "Фирменное");

            restaurant.AddMenuItem("Салат из огорода", "Листья с рынка, свежие травы и домашняя заправка.", 590, "Закуски");

            restaurant.AddMenuItem("Десерт от шефа", "Сладкое блюдо дня.", 520, "Десерты");

            restaurant.Publish();

            db.Restaurants.Add(restaurant);

        }



        await db.SaveChangesAsync(ct);

    }



    private async Task EnsureRoleAsync(string role)

    {

        if (!await roleManager.RoleExistsAsync(role))

            await roleManager.CreateAsync(new IdentityRole<Guid>(role));

    }



    private async Task EnsureUserAsync(string email, string password, string displayName, string role)

    {

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)

        {

            user = new ApplicationUser

            {

                Id = Guid.NewGuid(),

                UserName = email,

                Email = email,

                EmailConfirmed = true,

                DisplayName = displayName

            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)

                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));

            db.UserProfiles.Add(UserProfile.Create(UserId.From(user.Id), displayName, email));

            await db.SaveChangesAsync();

        }

        if (!await userManager.IsInRoleAsync(user, role))

            await userManager.AddToRoleAsync(user, role);

    }



    private sealed record Seed(

        string Name, string Slug, string Description, string Street, string City, string Phone,

        PriceCategory Price, string[] Cuisines, string[] Features, string Image);

}

