using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Entities.Voting;

namespace Filmowanie.Nomination.Models;

internal sealed record CurrentNominationsData(VotingSessionId VotingSessionId, Guid CorrelationId, NominationData[] NominationData);
