using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Reviews;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Infrastructure.Persistence.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(id => id.Value, v => ReviewId.From(v));
        builder.Property(x => x.RestaurantId).HasConversion(id => id.Value, v => RestaurantId.From(v));
        builder.Property(x => x.UserId).HasConversion(id => id.Value, v => UserId.From(v));
        builder.Property(x => x.Rating).HasConversion(r => r.Value, v => Rating.Create(v)).HasPrecision(3, 1);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.RestaurantId);
        builder.HasIndex(x => new { x.RestaurantId, x.UserId });
    }
}
