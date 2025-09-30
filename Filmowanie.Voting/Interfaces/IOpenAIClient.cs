namespace Filmowanie.Voting.Interfaces;

public interface IOpenAIClient
{
    Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken);
}
