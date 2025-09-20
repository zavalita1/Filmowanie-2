using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Account.Interfaces;

internal interface ILoginDataExtractorAdapterFactory
{
    public ILoginDataExtractorAdapter<T> GetAdapter<T>() where T : IMailBasedUserData;

    public ILoginResultDataExtractor GetExtractor();
}
