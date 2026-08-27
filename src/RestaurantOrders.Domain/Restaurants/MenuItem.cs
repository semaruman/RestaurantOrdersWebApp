using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class MenuItemId : StronglyTypedId
{
    private MenuItemId(Guid value) : base(value) { }

    public static MenuItemId New() => new(Guid.NewGuid());

    public static MenuItemId From(Guid value) => new(value);
}

public sealed class MenuItem : Entity
{
    public MenuItemId Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? Category { get; private set; }
    public Money Price { get; private set; } = null!;
    public string? PhotoUrl { get; private set; }
    public bool IsAvailable { get; private set; }
    public string? Ingredients { get; private set; }

    private MenuItem() { }

    internal static MenuItem Create(
        string name,
        string description,
        Money price,
        string? category = null,
        string? photoUrl = null,
        string? ingredients = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("INVALID_MENU_ITEM", "Menu item name is required.");

        return new MenuItem
        {
            Id = MenuItemId.New(),
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Category = category?.Trim(),
            Price = price,
            PhotoUrl = photoUrl,
            Ingredients = ingredients?.Trim(),
            IsAvailable = true
        };
    }

    internal void Update(string name, string description, Money price, string? category, string? photoUrl, string? ingredients)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("INVALID_MENU_ITEM", "Menu item name is required.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Category = category?.Trim();
        PhotoUrl = photoUrl;
        Ingredients = ingredients?.Trim();
    }

    internal void SetAvailability(bool available) => IsAvailable = available;
}
