namespace Filmowanie.Account.Interfaces;

public interface IHashHelper
{
    string GetHash(string secret, string saltSeed);
    bool DoesHashEqual(string expectedHash, string secret);
}