using System.Security.Cryptography;
using Common.Abstractions.Providers;

namespace Common.Infrastructure.Providers;

public sealed class RandomProvider : IRandomProvider
{
    public string GenerateToken(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes);
    }
}