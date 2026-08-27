using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(id => id.Value, v => UserId.From(v));
        builder.Property(x => x.DisplayName).HasMaxLength(120);
        builder.Property(x => x.Email)
            .HasConversion(e => e.Value, v => EmailAddress.Create(v))
            .HasMaxLength(200);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
