using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Repositories;
using Filmowanie.Database.Repositories.Internal;

namespace Filmowanie.Database.Decorators;

// TODO tests
internal sealed class VotingSessionQueryRepositoryInUserlessContextProvider : IVotingSessionQueryRepositoryInUserlessContextProvider, IMovieQueryRepositoryInUserslessContextProvider
{
    private readonly VotingResultsContext votingResultsCtx;
    private readonly MoviesContext moviesCtx;

    public VotingSessionQueryRepositoryInUserlessContextProvider(VotingResultsContext votingResultsCtx, MoviesContext moviesCtx)
    {
        this.votingResultsCtx = votingResultsCtx;
        this.moviesCtx = moviesCtx;
    }

    public IVotingSessionQueryRepositoryInUserlessContext GetRepository(TenantId tenantId)
    {
        var userAccessor = new FixedTenantUserAccessor(tenantId);
        var repo = new VotingSessionQueryRepository(this.votingResultsCtx);
        var result = new VotingSessionQueryRepositoryDecorator(repo, userAccessor);
        return result;
    }

    IMovieQueryRepository IMovieQueryRepositoryInUserslessContextProvider.GetRepository(TenantId tenantId)
    {
        var userAccessor = new FixedTenantUserAccessor(tenantId);
        var repo = new MovieQueryRepository(this.moviesCtx);
        var result = new MovieQueryRepositoryDecorator(repo, userAccessor);
        return result;
    }

    private sealed class FixedTenantUserAccessor : ICurrentUserAccessor
    {
        private readonly TenantId tenant;

        public FixedTenantUserAccessor(TenantId tenantId)
        {
            tenant = tenantId;
        }

        public Maybe<DomainUser> GetDomainUser(Maybe<VoidResult> maybe)
        {
            var result = new DomainUser { Tenant = tenant };
            return result.AsMaybe();
        }
    }
}
