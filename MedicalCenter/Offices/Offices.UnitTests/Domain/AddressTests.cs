using Common.Domain.Exceptions;
using Offices.Domain.ValueObjects;

namespace Offices.UnitTests.Domain;

public sealed class AddressTests
{
    [Fact]
    public void Create_ValuesWithSurroundingSpaces_TrimsEveryField()
    {
        var address = Address.Create("  Москва ", " Тверская ", " 12 ", " 101 ");

        address.City.Should().Be("Москва");
        address.Street.Should().Be("Тверская");
        address.HouseNumber.Should().Be("12");
        address.OfficeNumber.Should().Be("101");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankOfficeNumber_StoresNull(string? officeNumber)
    {
        var address = Address.Create("Москва", "Тверская", "12", officeNumber);

        address.OfficeNumber.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankCity_ThrowsDomainException(string? city)
    {
        var act = () => Address.Create(city!, "Тверская", "12", "101");

        act.Should().Throw<DomainException>().WithMessage("city *");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankStreet_ThrowsDomainException(string? street)
    {
        var act = () => Address.Create("Москва", street!, "12", "101");

        act.Should().Throw<DomainException>().WithMessage("street *");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankHouseNumber_ThrowsDomainException(string? houseNumber)
    {
        var act = () => Address.Create("Москва", "Тверская", houseNumber!, "101");

        act.Should().Throw<DomainException>().WithMessage("houseNumber *");
    }

    [Fact]
    public void Format_WithOfficeNumber_JoinsAllPartsWithComma()
    {
        var address = Address.Create("Москва", "Тверская", "12", "101");

        var formatted = address.Format();

        formatted.Should().Be("Москва, Тверская, 12, 101");
    }

    [Fact]
    public void Format_WithoutOfficeNumber_OmitsOfficeNumber()
    {
        var address = Address.Create("Москва", "Тверская", "12", null);

        var formatted = address.Format();

        formatted.Should().Be("Москва, Тверская, 12");
    }

    [Fact]
    public void Equals_SameValuesAfterTrimming_AreEqual()
    {
        var address = Address.Create("Москва", "Тверская", "12", "101");
        var sameAddressWithSpaces = Address.Create(" Москва", "Тверская ", " 12", "101 ");

        address.Should().Be(sameAddressWithSpaces);
        address.GetHashCode().Should().Be(sameAddressWithSpaces.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentOfficeNumber_AreNotEqual()
    {
        var address = Address.Create("Москва", "Тверская", "12", "101");
        var neighbourOffice = Address.Create("Москва", "Тверская", "12", "102");

        address.Should().NotBe(neighbourOffice);
    }
}
