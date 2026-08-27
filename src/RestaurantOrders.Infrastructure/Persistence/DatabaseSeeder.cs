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
        await EnsureUserAsync("admin@restaurant.local", "Admin123!", "Platform Admin", Roles.Admin);
        await EnsureUserAsync("user@restaurant.local", "User123!", "Demo Guest", Roles.User);

        if (await db.Restaurants.AnyAsync(ct))
            return;

        var seeds = new[]
        {
            new Seed("Juniper & Rye", "juniper-and-rye", "Modern northern cuisine by a wood-fired hearth.",
                "12 Tverskaya Street", "Moscow", "+7 495 100-10-10", PriceCategory.Upscale,
                new[] { "Modern European", "Russian" }, new[] { "Chef's table", "Wine pairing" },
                "https://images.unsplash.com/photo-1517248135467-4c7edcad34c5"),
            new Seed("Saffron Courtyard", "saffron-courtyard", "Silk Road plates, fragrant spices and generous sharing menus.",
                "8 Rubinstein Street", "Saint Petersburg", "+7 812 200-20-20", PriceCategory.Moderate,
                new[] { "Georgian", "Middle Eastern" }, new[] { "Terrace", "Family friendly" },
                "https://images.unsplash.com/photo-1552566626-52f8b828add9"),
            new Seed("Casa Limone", "casa-limone", "A sunlit Italian kitchen focused on handmade pasta and seafood.",
                "21 Rozhdestvenskaya Street", "Nizhny Novgorod", "+7 831 300-30-30", PriceCategory.Moderate,
                new[] { "Italian", "Mediterranean" }, new[] { "Waterfront", "Vegetarian options" },
                "https://images.unsplash.com/photo-1555396273-367ea4eb4db5"),
            new Seed("Ember Table", "ember-table", "Seasonal produce and prime cuts cooked over open flame.",
                "4 Krasny Avenue", "Novosibirsk", "+7 383 400-40-40", PriceCategory.Upscale,
                new[] { "Steakhouse", "Contemporary" }, new[] { "Open kitchen", "Private dining" },
                "https://images.unsplash.com/photo-1414235077428-338989a2e8c0"),
            new Seed("Dacha Brunch", "dacha-brunch", "Comforting all-day breakfasts inspired by a countryside dacha.",
                "17 Lenina Street", "Kazan", "+7 843 500-50-50", PriceCategory.Budget,
                new[] { "Cafe", "Russian" }, new[] { "Pet friendly", "All-day breakfast" },
                "https://images.unsplash.com/photo-1533777857889-4be7c70b33f7"),
            new Seed("Blue Current", "blue-current", "Refined seafood, raw bar classics and bright coastal cocktails.",
                "30 Morskaya Street", "Sochi", "+7 862 600-60-60", PriceCategory.Luxury,
                new[] { "Seafood", "Japanese" }, new[] { "Sea view", "Raw bar" },
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
            restaurant.AddMenuItem("Signature tasting plate", "A seasonal selection from the kitchen.",
                seed.Price is PriceCategory.Upscale or PriceCategory.Luxury ? 2400 : 950, "Signatures");
            restaurant.AddMenuItem("Garden salad", "Market leaves, herbs and house vinaigrette.", 590, "Starters");
            restaurant.AddMenuItem("House dessert", "Chef's changing sweet course.", 520, "Desserts");
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
