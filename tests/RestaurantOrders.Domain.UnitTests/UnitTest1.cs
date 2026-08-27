using FluentAssertions;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Favorites;
using RestaurantOrders.Domain.Orders;
using RestaurantOrders.Domain.Reservations;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Reviews;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Domain.UnitTests;

public class RestaurantAndOrderTests
{
    [Fact]
    public void Publish_WhenRestaurantIsComplete_ChangesStatusAndRaisesEvent()
    {
        var restaurant = CompleteRestaurant();

        restaurant.Publish();

        restaurant.Status.Should().Be(RestaurantStatus.Published);
        restaurant.DomainEvents.Should().ContainSingle(x => x is RestaurantPublishedDomainEvent);
    }

    [Fact]
    public void Publish_WhenMenuIsMissing_RejectsTransition()
    {
        var restaurant = Restaurant.Create("No Menu");
        restaurant.UpdateDescription("A restaurant that is not ready yet.");
        restaurant.SetAddress(Address.Create("1 Test Street", "Moscow"));
        restaurant.SetContacts(ContactInformation.Create("+7 495 000-00-00"));
        restaurant.SetCuisineTypes(["European"]);

        var act = restaurant.Publish;

        act.Should().Throw<DomainException>().Which.Code.Should().Be("RESTAURANT_NOT_READY");
    }

    [Fact]
    public void ClosedRestaurant_CannotAcceptReservation()
    {
        var restaurant = CompleteRestaurant();
        restaurant.Publish();
        restaurant.CloseTemporarily();

        restaurant.CanAcceptReservation(DateTime.UtcNow.AddDays(1), 2).Should().BeFalse();
    }

    [Fact]
    public void CancelledReservation_CannotBeConfirmed()
    {
        var restaurant = CompleteRestaurant();
        restaurant.Publish();
        var reservation = Reservation.Create(restaurant, UserId.New(), DateTime.UtcNow.AddDays(2).Date.AddHours(19), 2);
        reservation.Cancel();

        var act = reservation.Confirm;

        act.Should().Throw<DomainException>().Which.Code.Should().Be("INVALID_RESERVATION_STATUS");
    }

    [Fact]
    public void CompletedOrder_CannotBeModified()
    {
        var restaurant = CompleteRestaurant();
        var item = restaurant.MenuItems.Single();
        var order = Order.CreateDraft(restaurant.Id, UserId.New());
        order.AddItem(item, 1);
        order.Submit();
        order.Confirm();
        order.StartPreparing();
        order.MarkReady();
        order.Complete();

        var act = () => order.AddItem(item, 1);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("ORDER_LOCKED");
    }

    [Fact]
    public void Rating_MustBeWithinValidRange()
    {
        var act = () => Rating.Create(6);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("INVALID_RATING");
    }

    [Fact]
    public void Favorite_StoresUniqueUserRestaurantPair()
    {
        var userId = UserId.New();
        var restaurantId = RestaurantId.New();

        var favorite = Favorite.Create(userId, restaurantId);

        favorite.UserId.Should().Be(userId);
        favorite.RestaurantId.Should().Be(restaurantId);
    }

    [Fact]
    public void Order_AddingSameMenuItem_CombinesQuantityAndCalculatesTotal()
    {
        var restaurant = CompleteRestaurant();
        var item = restaurant.MenuItems.Single();
        var order = Order.CreateDraft(restaurant.Id, UserId.New());

        order.AddItem(item, 2);
        order.AddItem(item, 1);
        order.Submit();

        order.Lines.Should().ContainSingle().Which.Quantity.Should().Be(3);
        order.Total.Should().Be(Money.Rub(3600));
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Review_SubmitAndPublish_RaisesDomainEvent()
    {
        var restaurant = CompleteRestaurant();
        restaurant.Publish();
        var review = Review.Submit(restaurant.Id, UserId.New(), 5, "Wonderful tasting menu.");

        review.Publish();

        review.Status.Should().Be(ReviewStatus.Published);
        review.DomainEvents.Should().Contain(x => x is ReviewPublishedDomainEvent);
    }

    private static Restaurant CompleteRestaurant()
    {
        var restaurant = Restaurant.Create("Juniper Test");
        restaurant.UpdateDescription("A complete restaurant ready to welcome guests.");
        restaurant.SetAddress(Address.Create("1 Test Street", "Moscow"));
        restaurant.SetContacts(ContactInformation.Create("+7 495 000-00-00"));
        restaurant.SetCuisineTypes(["Modern European"]);
        restaurant.ConfigureOptions(true, true, 50);
        restaurant.SetOpeningHours([
            OpeningHours.Open(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(23, 0)),
            OpeningHours.Open(DayOfWeek.Tuesday, new TimeOnly(12, 0), new TimeOnly(23, 0)),
            OpeningHours.Open(DayOfWeek.Wednesday, new TimeOnly(12, 0), new TimeOnly(23, 0)),
            OpeningHours.Open(DayOfWeek.Thursday, new TimeOnly(12, 0), new TimeOnly(23, 0)),
            OpeningHours.Open(DayOfWeek.Friday, new TimeOnly(12, 0), new TimeOnly(23, 0)),
            OpeningHours.Open(DayOfWeek.Saturday, new TimeOnly(12, 0), new TimeOnly(23, 0)),
            OpeningHours.Open(DayOfWeek.Sunday, new TimeOnly(12, 0), new TimeOnly(22, 0))
        ]);
        restaurant.AddMenuItem("Seasonal plate", "Market ingredients.", 1200);
        return restaurant;
    }
}
