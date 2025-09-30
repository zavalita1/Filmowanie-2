using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Voting.Extensions;

// TODO UTs
public static class VoteTypeExtensions
{
    public static int GetVoteCount(this VoteType voteType) => voteType == VoteType.Trash ? 0 : (int)voteType;
}