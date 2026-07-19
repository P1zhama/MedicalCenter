using Common.Domain.Exceptions;

namespace Common.Domain;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} must not be empty.");
        }

        return value;
    }

    public static string MaxLength(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
        {
            throw new DomainException($"{parameterName} must not exceed {maxLength} characters.");
        }

        return value;
    }

    public static T NotNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
        {
            throw new DomainException($"{parameterName} must not be null.");
        }

        return value;
    }

    public static Guid NotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{parameterName} must not be empty.");
        }

        return value;
    }
}
