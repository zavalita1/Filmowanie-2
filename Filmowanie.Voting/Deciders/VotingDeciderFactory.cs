using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Deciders;

public interface IVotingDeciderFactory
{
    IVotingDecider ForRegularVoting();
    IVotingDecider ForTrashVoting();
}

public class VotingDeciderFactory : IVotingDeciderFactory
{
    public IVotingDecider ForRegularVoting() => new VotingDecider();
    public IVotingDecider ForTrashVoting() => new TrashVotingDecider();
}