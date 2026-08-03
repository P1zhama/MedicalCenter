using Common.Domain;
using Common.Domain.Exceptions;

namespace Services.Domain.ValueObjects;

public sealed class Price : ValueObject
{
    private Price(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; }

    public static Price Create(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("You've entered an invalid price");

        return new Price(decimal.Round(amount, 2, MidpointRounding.AwayFromZero));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }
}
