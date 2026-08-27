using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class Restaurant : AggregateRoot
{
    private readonly List<string> _cuisineTypes = [];
    private readonly List<string> _features = [];
    private readonly List<string> _photoUrls = [];
    private readonly List<OpeningHours> _openingHours = [];
    private readonly List<MenuItem> _menuItems = [];

    public RestaurantId Id { get; private set; } = null!;
    public RestaurantName Name { get; private set; } = null!;
    public RestaurantSlug Slug { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public Address? Address { get; private set; }
    public ContactInformation? Contacts { get; private set; }
    public PriceCategory PriceCategory { get; private set; } = PriceCategory.Moderate;
    public RestaurantStatus Status { get; private set; } = RestaurantStatus.Draft;
    public Rating AverageRating { get; private set; } = Rating.Zero;
    public int ReviewCount { get; private set; }
    public int? Capacity { get; private set; }
    public bool AcceptsReservations { get; private set; }
    public bool OffersDelivery { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<string> CuisineTypes => _cuisineTypes.AsReadOnly();
    public IReadOnlyCollection<string> Features => _features.AsReadOnly();
    public IReadOnlyCollection<string> PhotoUrls => _photoUrls.AsReadOnly();
    public IReadOnlyCollection<OpeningHours> OpeningHours => _openingHours.AsReadOnly();
    public IReadOnlyCollection<MenuItem> MenuItems => _menuItems.AsReadOnly();

    private Restaurant() { }

    public static Restaurant Create(string name, string? slug = null)
    {
        var restaurantName = RestaurantName.Create(name);
        var restaurantSlug = string.IsNullOrWhiteSpace(slug)
            ? RestaurantSlug.FromName(restaurantName)
            : RestaurantSlug.Create(slug);

        var now = DateTime.UtcNow;
        var restaurant = new Restaurant
        {
            Id = RestaurantId.New(),
            Name = restaurantName,
            Slug = restaurantSlug,
            Status = RestaurantStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        restaurant.Raise(new RestaurantCreatedDomainEvent(restaurant.Id));
        return restaurant;
    }

    public void ChangeName(string name)
    {
        EnsureNotPermanentlyClosed();
        Name = RestaurantName.Create(name);
        Touch();
    }

    public void UpdateDescription(string description)
    {
        EnsureNotPermanentlyClosed();
        Description = description?.Trim() ?? string.Empty;
        Touch();
    }

    public void SetAddress(Address address)
    {
        EnsureNotPermanentlyClosed();
        Address = address;
        Touch();
    }

    public void SetContacts(ContactInformation contacts)
    {
        EnsureNotPermanentlyClosed();
        Contacts = contacts;
        Touch();
    }

    public void SetPriceCategory(PriceCategory category)
    {
        EnsureNotPermanentlyClosed();
        PriceCategory = category;
        Touch();
    }

    public void SetCuisineTypes(IEnumerable<string> cuisines)
    {
        EnsureNotPermanentlyClosed();
        _cuisineTypes.Clear();
        foreach (var cuisine in cuisines.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
            _cuisineTypes.Add(cuisine);
        Touch();
    }

    public void SetFeatures(IEnumerable<string> features)
    {
        EnsureNotPermanentlyClosed();
        _features.Clear();
        foreach (var feature in features.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
            _features.Add(feature);
        Touch();
    }

    public void SetOpeningHours(IEnumerable<OpeningHours> hours)
    {
        EnsureNotPermanentlyClosed();
        var list = hours.ToList();
        if (list.Select(h => h.Day).Distinct().Count() != list.Count)
            throw new DomainException("INVALID_HOURS", "Opening hours contain duplicate days.");

        _openingHours.Clear();
        _openingHours.AddRange(list);
        Touch();
    }

    public void SetPhotos(string? coverImageUrl, IEnumerable<string>? photoUrls)
    {
        EnsureNotPermanentlyClosed();
        CoverImageUrl = coverImageUrl;
        _photoUrls.Clear();
        if (photoUrls is not null)
            _photoUrls.AddRange(photoUrls.Where(u => !string.IsNullOrWhiteSpace(u)));
        Touch();
    }

    public void ConfigureOptions(bool acceptsReservations, bool offersDelivery, int? capacity)
    {
        EnsureNotPermanentlyClosed();
        if (capacity is <= 0)
            throw new DomainException("INVALID_CAPACITY", "Capacity must be positive.");

        AcceptsReservations = acceptsReservations;
        OffersDelivery = offersDelivery;
        Capacity = capacity;
        Touch();
    }

    public MenuItem AddMenuItem(
        string name,
        string description,
        decimal price,
        string? category = null,
        string? photoUrl = null,
        string? ingredients = null)
    {
        EnsureNotPermanentlyClosed();
        var item = MenuItem.Create(name, description, Money.Rub(price), category, photoUrl, ingredients);
        _menuItems.Add(item);
        Touch();
        return item;
    }

    public void UpdateMenuItem(
        MenuItemId id,
        string name,
        string description,
        decimal price,
        string? category,
        string? photoUrl,
        string? ingredients)
    {
        EnsureNotPermanentlyClosed();
        var item = GetMenuItem(id);
        item.Update(name, description, Money.Rub(price), category, photoUrl, ingredients);
        Touch();
    }

    public void SetMenuItemAvailability(MenuItemId id, bool available)
    {
        EnsureNotPermanentlyClosed();
        GetMenuItem(id).SetAvailability(available);
        Touch();
    }

    public MenuItem GetMenuItem(MenuItemId id) =>
        _menuItems.FirstOrDefault(m => m.Id == id)
        ?? throw new DomainException("MENU_ITEM_NOT_FOUND", "Menu item was not found.");

    public void Publish()
    {
        if (Status == RestaurantStatus.PermanentlyClosed)
            throw new DomainException("INVALID_STATUS_TRANSITION", "Permanently closed restaurant cannot be published.");

        if (Status is not (RestaurantStatus.Draft or RestaurantStatus.TemporarilyClosed))
            throw new DomainException("INVALID_STATUS_TRANSITION", $"Cannot publish from status {Status}.");

        EnsureReadyForPublish();
        Status = RestaurantStatus.Published;
        Touch();
        Raise(new RestaurantPublishedDomainEvent(Id));
    }

    public void Unpublish()
    {
        if (Status != RestaurantStatus.Published)
            throw new DomainException("INVALID_STATUS_TRANSITION", "Only published restaurants can be unpublished.");

        Status = RestaurantStatus.Draft;
        Touch();
    }

    public void CloseTemporarily()
    {
        if (Status != RestaurantStatus.Published)
            throw new DomainException("INVALID_STATUS_TRANSITION", "Only published restaurants can be temporarily closed.");

        Status = RestaurantStatus.TemporarilyClosed;
        Touch();
        Raise(new RestaurantClosedDomainEvent(Id, permanently: false));
    }

    public void ClosePermanently()
    {
        if (Status == RestaurantStatus.PermanentlyClosed)
            throw new DomainException("INVALID_STATUS_TRANSITION", "Restaurant is already permanently closed.");

        Status = RestaurantStatus.PermanentlyClosed;
        Touch();
        Raise(new RestaurantClosedDomainEvent(Id, permanently: true));
    }

    public bool IsOpenAt(DateTime utcDateTime)
    {
        if (Status != RestaurantStatus.Published)
            return false;

        var local = utcDateTime; // simplified: treat as local business time
        var hours = _openingHours.FirstOrDefault(h => h.Day == local.DayOfWeek);
        if (hours is null)
            return true; // no schedule configured => assume open when published

        return hours.IsOpenAt(TimeOnly.FromDateTime(local));
    }

    public bool CanAcceptReservation(DateTime dateTimeUtc, int guestCount)
    {
        if (!AcceptsReservations)
            return false;
        if (Status != RestaurantStatus.Published)
            return false;
        if (guestCount <= 0)
            return false;
        if (Capacity is int cap && guestCount > cap)
            return false;
        if (dateTimeUtc <= DateTime.UtcNow)
            return false;

        return IsOpenAt(dateTimeUtc);
    }

    public void ApplyReviewStats(decimal averageRating, int reviewCount)
    {
        if (reviewCount < 0)
            throw new DomainException("INVALID_REVIEW_STATS", "Review count cannot be negative.");

        AverageRating = reviewCount == 0 ? Rating.Zero : Rating.FromAverage(averageRating);
        ReviewCount = reviewCount;
        Touch();
    }

    private void EnsureReadyForPublish()
    {
        if (Address is null)
            throw new DomainException("RESTAURANT_NOT_READY", "Address is required before publish.");
        if (Contacts is null)
            throw new DomainException("RESTAURANT_NOT_READY", "Contacts are required before publish.");
        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("RESTAURANT_NOT_READY", "Description is required before publish.");
        if (_cuisineTypes.Count == 0)
            throw new DomainException("RESTAURANT_NOT_READY", "At least one cuisine is required before publish.");
        if (_menuItems.Count == 0)
            throw new DomainException("RESTAURANT_NOT_READY", "At least one menu item is required before publish.");
    }

    private void EnsureNotPermanentlyClosed()
    {
        if (Status == RestaurantStatus.PermanentlyClosed)
            throw new DomainException("RESTAURANT_CLOSED", "Permanently closed restaurant cannot be modified.");
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
