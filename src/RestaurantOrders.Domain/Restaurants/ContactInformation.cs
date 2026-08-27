using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class ContactInformation : ValueObject
{
    public PhoneNumber Phone { get; }
    public EmailAddress? Email { get; }
    public string? Website { get; }

    private ContactInformation(PhoneNumber phone, EmailAddress? email, string? website)
    {
        Phone = phone;
        Email = email;
        Website = website;
    }

    public static ContactInformation Create(string phone, string? email = null, string? website = null)
    {
        return new ContactInformation(
            PhoneNumber.Create(phone),
            string.IsNullOrWhiteSpace(email) ? null : EmailAddress.Create(email),
            string.IsNullOrWhiteSpace(website) ? null : website.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Phone;
        yield return Email;
        yield return Website;
    }
}
