using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;
using System.Text.Json;

namespace RestaurantOrders.Infrastructure.Persistence.Configurations;

internal sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, v => RestaurantId.From(v));

        builder.Property(x => x.Name)
            .HasConversion(n => n.Value, v => RestaurantName.Create(v))
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasConversion(s => s.Value, v => RestaurantSlug.Create(v))
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.PriceCategory).HasConversion<string>().HasMaxLength(32);

        builder.Property(x => x.AverageRating)
            .HasConversion(r => r.Value, v => v == 0 ? Rating.Zero : Rating.FromAverage(v))
            .HasPrecision(3, 1);

        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.UpdatedAtUtc).IsConcurrencyToken();
        builder.Property(x => x.AcceptsReservations);
        builder.Property(x => x.OffersDelivery);
        builder.Property(x => x.Capacity);
        builder.Property(x => x.ReviewCount);
        builder.Ignore(x => x.RowVersion);

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Street).HasColumnName("street").HasMaxLength(200);
            a.Property(p => p.City).HasColumnName("city").HasMaxLength(100);
            a.Property(p => p.PostalCode).HasColumnName("postal_code").HasMaxLength(32);
            a.Property(p => p.Country).HasColumnName("country").HasMaxLength(80);
            a.Property(p => p.Latitude).HasColumnName("latitude");
            a.Property(p => p.Longitude).HasColumnName("longitude");
        });

        builder.Navigation(x => x.Address).IsRequired(false);

        builder.OwnsOne(x => x.Contacts, c =>
        {
            c.Property(p => p.Phone)
                .HasConversion(p => p.Value, v => PhoneNumber.Create(v))
                .HasColumnName("phone")
                .HasMaxLength(32);
            c.Property(p => p.Email)
                .HasConversion(
                    e => e == null ? null : e.Value,
                    v => string.IsNullOrWhiteSpace(v) ? null : EmailAddress.Create(v))
                .HasColumnName("email")
                .HasMaxLength(200);
            c.Property(p => p.Website).HasColumnName("website").HasMaxLength(300);
        });

        builder.Navigation(x => x.Contacts).IsRequired(false);

        var stringListComparer = new ValueComparer<List<string>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList());

        builder.Property<List<string>>("_cuisineTypes")
            .HasField("_cuisineTypes")
            .HasColumnName("cuisines_json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(stringListComparer);

        builder.Property<List<string>>("_features")
            .HasField("_features")
            .HasColumnName("features_json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(stringListComparer);

        builder.Property<List<string>>("_photoUrls")
            .HasField("_photoUrls")
            .HasColumnName("photos_json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(stringListComparer);

        builder.OwnsMany<OpeningHours>("_openingHours", oh =>
        {
            oh.ToTable("restaurant_opening_hours");
            oh.WithOwner().HasForeignKey("RestaurantId");
            oh.Property<Guid>("Id").ValueGeneratedOnAdd();
            oh.HasKey("Id");
            oh.Property(x => x.Day).HasConversion<string>().HasMaxLength(16);
            oh.Property(x => x.OpenTime);
            oh.Property(x => x.CloseTime);
            oh.Property(x => x.IsClosed);
        });

        builder.OwnsMany<MenuItem>("_menuItems", m =>
        {
            m.ToTable("menu_items");
            m.WithOwner().HasForeignKey("RestaurantId");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).HasConversion(id => id.Value, v => MenuItemId.From(v));
            m.Property(x => x.Name).HasMaxLength(200).IsRequired();
            m.Property(x => x.Description).HasMaxLength(2000);
            m.Property(x => x.Category).HasMaxLength(100);
            m.Property(x => x.PhotoUrl).HasMaxLength(500);
            m.Property(x => x.Ingredients).HasMaxLength(1000);
            m.Property(x => x.IsAvailable);
            m.Ignore(x => x.DomainEvents);
            m.OwnsOne(x => x.Price, p =>
            {
                p.Property(x => x.Amount).HasColumnName("price_amount").HasPrecision(18, 2);
                p.Property(x => x.Currency).HasColumnName("price_currency").HasMaxLength(8);
            });
            m.Navigation(x => x.Price).IsRequired();
        });

        builder.Navigation("_openingHours").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_menuItems").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.CuisineTypes);
        builder.Ignore(x => x.Features);
        builder.Ignore(x => x.PhotoUrls);
        builder.Ignore(x => x.OpeningHours);
        builder.Ignore(x => x.MenuItems);
    }
}
