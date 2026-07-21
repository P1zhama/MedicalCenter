namespace Authorization.Application.Common.Interfaces;

public interface ITokenHashGenerator
{
    string Hash(string token);
}
