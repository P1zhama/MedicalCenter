using Authorization.Application.Common.Models;
using Authorization.Domain;

namespace Authorization.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    AccessToken Generate(Account account);
}
