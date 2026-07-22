using Authorization.Application.Common.Models;

namespace Authorization.Application.Common.Interfaces;

public interface IEmailConfirmationTokenGenerator
{
    EmailConfirmationTokenDescriptor Generate(DateTimeOffset issuedAt);
}
