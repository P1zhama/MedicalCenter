using System.Text.RegularExpressions;
using Common.Domain;
using ErrorOr;

namespace Authorization.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private const int MaxLength = 254;

    public string Value { get; }

    private Email(string value) => Value = value;

    public static ErrorOr<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("Email.Empty", "Email must not be empty.");

        value = value.Trim().ToLowerInvariant();

        if (value.Length > MaxLength)
            return Error.Validation("Email.TooLong", $"Email must not exceed {MaxLength} characters.");

        if (!EmailRegex().IsMatch(value))
            return Error.Validation("Email.Invalid", "Email format is invalid.");

        return new Email(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
