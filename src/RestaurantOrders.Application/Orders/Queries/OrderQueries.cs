using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Orders.Queries;

public sealed record OrderDto(
    Guid Id,
    Guid RestaurantId,
    string Status,
    decimal Total,
    string Currency,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderLineDto> Lines);

public sealed record OrderLineDto(Guid MenuItemId, string Name, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record GetUserOrdersQuery(Guid UserId);
public sealed class GetUserOrdersHandler(IOrderRepository orders)
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetUserOrdersQuery query, CancellationToken ct = default)
    {
        var list = await orders.GetByUserAsync(UserId.From(query.UserId), ct);
        return list.Select(o => new OrderDto(
            o.Id.Value,
            o.RestaurantId.Value,
            o.Status.ToString(),
            o.Total.Amount,
            o.Total.Currency,
            o.CreatedAtUtc,
            o.Lines.Select(l => new OrderLineDto(
                l.MenuItemId.Value,
                l.NameSnapshot,
                l.UnitPrice.Amount,
                l.Quantity,
                l.LineTotal.Amount)).ToList())).ToList();
    }
}
