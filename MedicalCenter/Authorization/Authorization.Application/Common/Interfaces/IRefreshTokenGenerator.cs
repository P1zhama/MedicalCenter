using Authorization.Application.Common.Models;

namespace Authorization.Application.Common.Interfaces;

public interface IRefreshTokenGenerator
{
    RefreshTokenDescriptor Generate(DateTimeOffset issuedAt);
}
