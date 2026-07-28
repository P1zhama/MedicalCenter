using Common.Domain;

namespace Offices.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    private Address(string city, string street, string houseNumber, string? officeNumber)
    {
        City = city;
        Street = street;
        HouseNumber = houseNumber;
        OfficeNumber = officeNumber;
    }

    public string City { get; }

    public string Street { get; }

    public string HouseNumber { get; }

    public string? OfficeNumber { get; }

    public static Address Create(string city, string street, string houseNumber, string? officeNumber)
    {
        Guard.NotNullOrWhiteSpace(city, nameof(city));
        Guard.NotNullOrWhiteSpace(street, nameof(street));
        Guard.NotNullOrWhiteSpace(houseNumber, nameof(houseNumber));

        return new Address(
            city.Trim(),
            street.Trim(),
            houseNumber.Trim(),
            string.IsNullOrWhiteSpace(officeNumber) ? null : officeNumber.Trim());
    }

    public string Format()
    {
        var parts = new List<string> { City, Street, HouseNumber };

        if (!string.IsNullOrWhiteSpace(OfficeNumber))
            parts.Add(OfficeNumber);

        return string.Join(", ", parts);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return Street;
        yield return HouseNumber;
        yield return OfficeNumber;
    }
}
