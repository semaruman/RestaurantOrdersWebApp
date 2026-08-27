using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Users;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string RestaurantOwner = "RestaurantOwner";
    public const string Moderator = "Moderator";
    public const string Manager = "Manager";
}

public static class Permissions
{
    public const string RestaurantRead = "restaurant.read";
    public const string RestaurantManage = "restaurant.manage";
    public const string RestaurantPublish = "restaurant.publish";
    public const string RestaurantDelete = "restaurant.delete";
    public const string ReservationCreate = "reservation.create";
    public const string ReservationManage = "reservation.manage";
    public const string OrderCreate = "order.create";
    public const string OrderManage = "order.manage";
    public const string ReviewCreate = "review.create";
    public const string ReviewModerate = "review.moderate";
    public const string AdminUsersManage = "admin.users.manage";
}

public sealed class UserProfile : AggregateRoot
{
    public UserId Id { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(UserId id, string displayName, string email)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("INVALID_USER", "Display name is required.");

        return new UserProfile
        {
            Id = id,
            DisplayName = displayName.Trim(),
            Email = EmailAddress.Create(email),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void ChangeDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("INVALID_USER", "Display name is required.");

        DisplayName = displayName.Trim();
    }
}
