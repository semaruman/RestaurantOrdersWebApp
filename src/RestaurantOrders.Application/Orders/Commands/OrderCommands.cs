using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Orders;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Orders.Commands;

public sealed record CreateOrderItemDto(Guid MenuItemId, int Quantity);

public sealed record CreateOrderCommand(
    Guid RestaurantId,
    Guid? UserId,
    IReadOnlyList<CreateOrderItemDto> Items,
    string? Notes);

public sealed class CreateOrderHandler(IRestaurantRepository restaurants, IOrderRepository orders, IUnitOfWork uow)
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure<Guid>(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            if (restaurant.Status != RestaurantStatus.Published)
                return Result.Failure<Guid>(new Error("RESTAURANT_NOT_PUBLISHED", "Restaurant is not accepting orders.", ErrorType.BusinessRule));

            var order = Order.CreateDraft(restaurant.Id, cmd.UserId is Guid uid ? UserId.From(uid) : null);
            foreach (var item in cmd.Items)
            {
                var menuItem = restaurant.GetMenuItem(MenuItemId.From(item.MenuItemId));
                order.AddItem(menuItem, item.Quantity);
            }

            order.SetNotes(cmd.Notes);
            order.Submit();

            await orders.AddAsync(order, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(order.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record CancelOrderCommand(Guid OrderId);
public sealed class CancelOrderHandler(IOrderRepository orders, IUnitOfWork uow)
{
    public async Task<Result> Handle(CancelOrderCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var order = await orders.GetByIdAsync(OrderId.From(cmd.OrderId), ct);
            if (order is null)
                return Result.Failure(new Error("ORDER_NOT_FOUND", "Order not found.", ErrorType.NotFound));

            order.Cancel();
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record UpdateOrderStatusCommand(Guid OrderId, string Action);
public sealed class UpdateOrderStatusHandler(IOrderRepository orders, IUnitOfWork uow)
{
    public async Task<Result> Handle(UpdateOrderStatusCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var order = await orders.GetByIdAsync(OrderId.From(cmd.OrderId), ct);
            if (order is null)
                return Result.Failure(new Error("ORDER_NOT_FOUND", "Order not found.", ErrorType.NotFound));

            switch (cmd.Action.ToLowerInvariant())
            {
                case "confirm": order.Confirm(); break;
                case "prepare": order.StartPreparing(); break;
                case "ready": order.MarkReady(); break;
                case "complete": order.Complete(); break;
                default:
                    return Result.Failure(new Error("INVALID_ACTION", "Unknown order action.", ErrorType.Validation));
            }

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}
