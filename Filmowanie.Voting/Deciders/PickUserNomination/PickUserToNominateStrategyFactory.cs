using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Deciders.PickUserNomination;

public sealed class PickUserToNominateStrategyFactory : IPickUserToNominateStrategyFactory
{
    private readonly ILogger<PickUserToNominateTrashStrategy> trashLog;
    private readonly ILogger<PickUserToNominateStrategy> log;

    public PickUserToNominateStrategyFactory(ILogger<PickUserToNominateTrashStrategy> trashLog, ILogger<PickUserToNominateStrategy> log)
    {
        this.trashLog = trashLog;
        this.log = log;
    }

    public IPickUserToNominateStrategy ForTrashVoting() => new PickUserToNominateTrashStrategy(this.trashLog);
    public IPickUserToNominateStrategy ForRegularVoting() => new PickUserToNominateStrategy(this.log);
}