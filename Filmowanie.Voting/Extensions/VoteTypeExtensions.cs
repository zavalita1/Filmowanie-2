using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Voting.Extensions;

public static class VoteTypeExtensions
{
    public static int GetVoteCount(this VoteType voteType) => voteType == VoteType.Thrash ? 0 : (int)voteType;
}