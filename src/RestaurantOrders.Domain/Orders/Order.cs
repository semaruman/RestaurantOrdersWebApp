using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Domain.Orders;

public enum OrderStatus
{
    Draft = 0,
    Pending = 1,
    Confirmed = 2,
    Preparing = 3,
    Ready = 4,
    Completed = 5,
    Cancelled = 6
}

public sealed class OrderPlacedDomainEvent(OrderId orderId, RestaurantId restaurantId) : IDomainEvent
{
    public OrderId OrderId { get; } = orderId;
    public RestaurantId RestaurantId { get; } = restaurantId;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class OrderCancelledDomainEvent(OrderId orderId) : IDomainEvent
{
    public OrderId OrderId { get; } = orderId;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class OrderLine : Entity
{
    public Guid Id { get; private set; }
    public MenuItemId MenuItemId { get; private set; } = null!;
    public string NameSnapshot { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private OrderLine() { }

    internal static OrderLine Create(MenuItemId menuItemId, string name, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("INVALID_QUANTITY", "Quantity must be greater than zero.");

        return new OrderLine
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItemId,
            NameSnapshot = name,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    internal void IncreaseQuantity(int by)
    {
        if (by <= 0)
            throw new DomainException("INVALID_QUANTITY", "Quantity increment must be positive.");
        Quantity += by;
    }
}

public sealed class Order : AggregateRoot
{
    private readonly List<OrderLine> _lines = [];

    public OrderId Id { get; private set; } = null!;
    public RestaurantId RestaurantId { get; private set; } = null!;
    public UserId? UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public Money Total => _lines.Aggregate(Money.Rub(0), (sum, line) => sum.Add(line.LineTotal));

    private Order() { }

    public static Order CreateDraft(RestaurantId restaurantId, UserId? userId = null)
    {
        var now = DateTime.UtcNow;
        return new Order
        {
            Id = OrderId.New(),
            RestaurantId = restaurantId,
            UserId = userId,
            Status = OrderStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void AddItem(MenuItem menuItem, int quantity)
    {
        EnsureEditable();
        if (!menuItem.IsAvailable)
            throw new DomainException("MENU_ITEM_UNAVAILABLE", "Menu item is not available.");

        var existing = _lines.FirstOrDefault(l => l.MenuItemId == menuItem.Id);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _lines.Add(OrderLine.Create(menuItem.Id, menuItem.Name, menuItem.Price, quantity));
        }

        Touch();
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("INVALID_ORDER_STATUS", "Only draft orders can be submitted.");
        if (_lines.Count == 0)
            throw new DomainException("EMPTY_ORDER", "Order must contain at least one item.");

        Status = OrderStatus.Pending;
        Touch();
        Raise(new OrderPlacedDomainEvent(Id, RestaurantId));
    }

    public void Confirm() => Transition(OrderStatus.Pending, OrderStatus.Confirmed);
    public void StartPreparing() => Transition(OrderStatus.Confirmed, OrderStatus.Preparing);
    public void MarkReady() => Transition(OrderStatus.Preparing, OrderStatus.Ready);
    public void Complete() => Transition(OrderStatus.Ready, OrderStatus.Completed);

    public void Cancel()
    {
        if (Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Ready)
            throw new DomainException("INVALID_ORDER_STATUS", $"Cannot cancel order in status {Status}.");

        Status = OrderStatus.Cancelled;
        Touch();
        Raise(new OrderCancelledDomainEvent(Id));
    }

    public void SetNotes(string? notes)
    {
        EnsureEditable();
        Notes = notes?.Trim();
        Touch();
    }

    private void Transition(OrderStatus from, OrderStatus to)
    {
        if (Status != from)
            throw new DomainException("INVALID_ORDER_STATUS", $"Cannot transition from {Status} to {to}.");

        Status = to;
        Touch();
    }

    private void EnsureEditable()
    {
        if (Status is not (OrderStatus.Draft or OrderStatus.Pending))
            throw new DomainException("ORDER_LOCKED", "Order can no longer be modified.");
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
