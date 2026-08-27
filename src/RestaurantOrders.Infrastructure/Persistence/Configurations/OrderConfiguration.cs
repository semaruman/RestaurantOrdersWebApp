using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantOrders.Domain.Orders;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(id => id.Value, v => OrderId.From(v));
        builder.Property(x => x.RestaurantId).HasConversion(id => id.Value, v => RestaurantId.From(v));
        builder.Property(x => x.UserId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                v => v == null ? null : UserId.From(v.Value));
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => x.RestaurantId);
        builder.HasIndex(x => x.UserId);

        builder.OwnsMany<OrderLine>("_lines", line =>
        {
            line.ToTable("order_lines");
            line.WithOwner().HasForeignKey("OrderId");
            line.HasKey(x => x.Id);
            line.Property(x => x.MenuItemId).HasConversion(id => id.Value, v => MenuItemId.From(v));
            line.Property(x => x.NameSnapshot).HasMaxLength(200);
            line.Property(x => x.Quantity);
            line.Ignore(x => x.DomainEvents);
            line.Ignore(x => x.LineTotal);
            line.OwnsOne(x => x.UnitPrice, p =>
            {
                p.Property(x => x.Amount).HasColumnName("unit_price").HasPrecision(18, 2);
                p.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(8);
            });
            line.Navigation(x => x.UnitPrice).IsRequired();
        });

        builder.Navigation("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(x => x.Lines);
        builder.Ignore(x => x.Total);
        builder.Ignore(x => x.DomainEvents);
    }
}
