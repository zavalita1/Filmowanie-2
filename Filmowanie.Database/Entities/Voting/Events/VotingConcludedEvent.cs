using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting.Events;

public record VotingConcludedEvent(VotingSessionId VotingSessionId, TenantId Tenant, IReadOnlyEmbeddedMovieWithVotes[] MoviesWithVotes, NominationData[] NominationsData, DateTime VotingStarted) : IEvent { }