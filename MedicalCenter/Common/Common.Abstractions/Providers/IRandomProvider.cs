namespace Common.Abstractions.Providers;

public interface IRandomProvider
{
    string GenerateToken(int byteLength = 32);
}
