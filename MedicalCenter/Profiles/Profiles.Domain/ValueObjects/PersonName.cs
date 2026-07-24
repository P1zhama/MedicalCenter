using Common.Domain;
using ErrorOr;

namespace Profiles.Domain.ValueObjects;

public sealed class PersonName : ValueObject
{
    private PersonName(string firstName, string lastName, string? middleName)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    public string FirstName { get; }

    public string LastName { get; }

    public string? MiddleName { get; }

    public static ErrorOr<PersonName> Create(string firstName, string lastName, string? middleName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Error.Validation("PersonName.FirstName", "Please, enter the first name");

        if (string.IsNullOrWhiteSpace(lastName))
            return Error.Validation("PersonName.LastName", "Please, enter the last name");

        return new PersonName(
            firstName.Trim(),
            lastName.Trim(),
            string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return MiddleName;
    }
}
