using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class EmbeddedUser : IReadOnlyEmbeddedUser
{
    public string id { get; set; }

    public string Name { get; set; }

    public int TenantId { get; set; }
}

public class EmbeddedMovie : IReadOnlyEmbeddedMovie
{
    public string id { get; set; }

    public string Name { get; set; }
}

public class EmbeddedMovieWithVotes : EmbeddedMovie, IReadOnlyEmbeddedMovieWithVotes
{
    public IEnumerable<Vote> Votes { get; set; }
}

public sealed record Vote(IReadOnlyEmbeddedUser User, VoteType VoteType);