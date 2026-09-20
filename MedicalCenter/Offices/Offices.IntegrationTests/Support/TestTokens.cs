using System.Security.Claims;
using System.Security.Cryptography;
using Common.Abstractions.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Offices.Domain.Constants;

namespace Offices.IntegrationTests.Support;

public static class TestTokens
{
    public const string Issuer = "MedicalCenter.Tests";
    public const string Audience = "MedicalCenter.TestClients";

    public static readonly Guid ReceptionistId = TestOffice.ReceptionistId;
    public static readonly Guid DoctorId = new("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a05");

    private static readonly RSA Key = RSA.Create(2048);

    public static string PublicKey => Convert.ToBase64String(Key.ExportSubjectPublicKeyInfo());

    public static string Receptionist() => Issue(ReceptionistId, Roles.Receptionist, TimeSpan.FromMinutes(10));

    public static string Doctor() => Issue(DoctorId, Roles.Doctor, TimeSpan.FromMinutes(10));

    public static string Expired() => Issue(ReceptionistId, Roles.Receptionist, TimeSpan.FromMinutes(-10));

    private static string Issue(Guid userId, string role, TimeSpan lifetime)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.Add(lifetime),
            IssuedAt = DateTime.UtcNow.AddMinutes(-11),
            NotBefore = DateTime.UtcNow.AddMinutes(-11),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtClaimTypes.Subject, userId.ToString()),
                new Claim(JwtClaimTypes.Role, role)
            ]),
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(Key), SecurityAlgorithms.RsaSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
