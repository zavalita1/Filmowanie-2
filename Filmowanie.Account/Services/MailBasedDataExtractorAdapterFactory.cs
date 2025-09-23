using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;

namespace Filmowanie.Account.Services;

internal sealed class LoginDataExtractorFactory : ILoginDataExtractorAdapterFactory
{
    private readonly ILoginResultDataExtractorDecorator loginResultDataExtractorDecorator;
    private readonly ILoginResultDataExtractor loginResultDataExtractor;
    private readonly IHashHelper hashHelper;

    public LoginDataExtractorFactory(ILoginResultDataExtractorDecorator loginResultDataExtractorDecorator, ILoginResultDataExtractor loginResultDataExtractor, IHashHelper hashHelper)
    {
        this.loginResultDataExtractorDecorator = loginResultDataExtractorDecorator;
        this.loginResultDataExtractor = loginResultDataExtractor;
        this.hashHelper = hashHelper;
    }

    public ILoginDataExtractorAdapter<T> GetAdapter<T>() where T : IMailBasedUserData
    {
        if (typeof(T) == typeof(GoogleUserData))
            return (new GoogleDataExtractorAdapter(this.loginResultDataExtractorDecorator) as ILoginDataExtractorAdapter<T>)!;
        else if (typeof(T) == typeof(BasicAuthUserData))
            return (new BasicAuthDataExtractorAdapter(this.hashHelper, this.loginResultDataExtractor) as ILoginDataExtractorAdapter<T>)!;

        throw new NotImplementedException("Unknown type requested! " + typeof(T).Name);
    }

    public ILoginResultDataExtractor GetExtractor() => this.loginResultDataExtractor;
}
