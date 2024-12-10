using Filmowanie.Abstractions;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IEnrichMoviesForVotingSessionWithPlaceholdersVisitor : IOperationAsyncVisitor<(MovieDTO[], VotingSessionId), MovieDTO[]>;

public interface IGetVotingSessionResultVisitor : IOperationAsyncVisitor<(TenantId Tenant, VotingSessionId? VotingSessionId), VotingResultDTO>;
internal interface IGetVotingSessionsMetadataVisitor : IOperationAsyncVisitor<TenantId, VotingMetadata[]>;

internal interface IWinnersMetadataMapperVisitor : IOperationAsyncVisitor<(VotingMetadata[], TenantId), WinnerMetadata[]>;
internal interface IHistoryDTOMapperVisitor : IOperationVisitor<WinnerMetadata[], HistoryDTO>;
internal interface IHistoryStandingsDTOMapperVisitor : IOperationAsyncVisitor<TenantId, MovieVotingStandingsListDTO>;
