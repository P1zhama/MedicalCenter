using System.Security.Cryptography;
using System.Text;
using Authorization.Application.Common.Interfaces;

namespace Authorization.Infrastructure.Authentication;

public sealed class TokenHashGenerator : ITokenHashGenerator
{
    public string Hash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(hash);
    }
}
