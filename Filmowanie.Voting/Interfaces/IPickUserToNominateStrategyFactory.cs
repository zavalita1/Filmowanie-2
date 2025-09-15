namespace Filmowanie.Voting.Interfaces;

public interface IPickUserToNominateStrategyFactory
{
    IPickUserToNominateStrategy ForTrashVoting();
    IPickUserToNominateStrategy ForRegularVoting();
}