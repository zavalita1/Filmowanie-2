namespace Filmowanie.Account.Interfaces;

internal interface IHashHelper
{
    string GetHash(string secret, string saltSeed);
    bool DoesHashEqual(string expectedHash, string secret);
}