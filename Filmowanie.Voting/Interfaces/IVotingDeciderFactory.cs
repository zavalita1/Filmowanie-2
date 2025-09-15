namespace Filmowanie.Voting.Interfaces;

public interface IVotingDeciderFactory
{
    IVotingDecider ForRegularVoting();
    IVotingDecider ForTrashVoting();
}