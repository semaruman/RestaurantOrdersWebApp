using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantOrders.Domain.Favorites;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Infrastructure.Persistence.Configurations;

internal sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorites");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(id => id.Value, v => FavoriteId.From(v));
        builder.Property(x => x.UserId).HasConversion(id => id.Value, v => UserId.From(v));
        builder.Property(x => x.RestaurantId).HasConversion(id => id.Value, v => RestaurantId.From(v));
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.UserId, x.RestaurantId }).IsUnique();
    }
}
