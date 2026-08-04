using System.Text;

namespace Common.Domain;

public static class TextNormalization
{
    public static string CollapseWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        var pendingSeparator = false;

        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSeparator = builder.Length > 0;
                continue;
            }

            if (pendingSeparator)
            {
                builder.Append(' ');
                pendingSeparator = false;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }
}
