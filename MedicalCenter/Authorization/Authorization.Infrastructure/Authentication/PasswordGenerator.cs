using System.Security.Cryptography;
using Authorization.Application.Common.Interfaces;

namespace Authorization.Infrastructure.Authentication;

public sealed class PasswordGenerator : IPasswordGenerator
{
    private const string Lowercase = "abcdefghijkmnpqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%*?";
    private const int Length = 12;

    private static readonly string All = Lowercase + Uppercase + Digits + Symbols;

    public string Generate()
    {
        var characters = new char[Length];

        characters[0] = Pick(Lowercase);
        characters[1] = Pick(Uppercase);
        characters[2] = Pick(Digits);
        characters[3] = Pick(Symbols);

        for (var i = 4; i < Length; i++)
            characters[i] = Pick(All);

        Shuffle(characters);

        return new string(characters);
    }

    private static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];

    private static void Shuffle(char[] characters)
    {
        for (var i = characters.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (characters[i], characters[j]) = (characters[j], characters[i]);
        }
    }
}
