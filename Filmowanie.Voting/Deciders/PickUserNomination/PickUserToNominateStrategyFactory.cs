using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Deciders.PickUserNomination;

public interface IPickUserToNominateStrategyFactory
{
    IPickUserToNominateStrategy ForTrashVoting();
    IPickUserToNominateStrategy ForRegularVoting();
}

public sealed class PickUserToNominateStrategyFactory : IPickUserToNominateStrategyFactory
{
    private readonly ILogger<PickUserToNominateTrashStrategy> _trashLog;
    private readonly ILogger<PickUserToNominateStrategy> _log;

    public PickUserToNominateStrategyFactory(ILogger<PickUserToNominateTrashStrategy> trashLog, ILogger<PickUserToNominateStrategy> log)
    {
        _trashLog = trashLog;
        _log = log;
    }

    public IPickUserToNominateStrategy ForTrashVoting() => new PickUserToNominateTrashStrategy(_trashLog);
    public IPickUserToNominateStrategy ForRegularVoting() => new PickUserToNominateStrategy(_log);
}