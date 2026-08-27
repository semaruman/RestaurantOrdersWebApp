namespace RestaurantOrders.Domain.Restaurants;

public enum RestaurantStatus
{
    Draft = 0,
    Published = 1,
    TemporarilyClosed = 2,
    PermanentlyClosed = 3
}

public enum PriceCategory
{
    Budget = 1,
    Moderate = 2,
    Upscale = 3,
    Luxury = 4
}
