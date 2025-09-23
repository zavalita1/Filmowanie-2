using System.Linq.Expressions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Decorators;

internal sealed class VotingSessionQueryRepositoryDecorator : IVotingSessionQueryRepositoryInUserlessContext
{
    private readonly IVotingSessionQueryRepository decorated;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public VotingSessionQueryRepositoryDecorator(IVotingSessionQueryRepository decorated, ICurrentUserAccessor currentUserAccessor)
    {
        this.decorated = decorated;
        this.currentUserAccessor = currentUserAccessor;
    }

    public Task<IReadOnlyVotingResult?> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, CancellationToken cancelToken)
    {
        var currentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
        var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
        return decorated.Get(newPredicate, cancelToken);
    }

    public Task<IEnumerable<IReadOnlyVotingResult>> GetAll(Expression<Func<IReadOnlyVotingResult, bool>> predicate, CancellationToken cancelToken)
    {
        var currentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
        var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
        return decorated.GetAll(newPredicate, cancelToken);
    }

    public Task<IEnumerable<IReadOnlyVotingResult>> GetVotingResultAsync(Expression<Func<IReadOnlyVotingResult, bool>> predicate, Expression<Func<IReadOnlyVotingResult, object>> sortBy, int take,
        CancellationToken cancelToken)
    {
        var currentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
        var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
        return decorated.GetVotingResultAsync(newPredicate, sortBy, take, cancelToken);
    }

    private static Expression<Func<T, bool>> GetPredicateWithTenantFilter<T>(Expression<Func<T, bool>> predicate, TenantId user) where T : IReadOnlyEntity
    {
        var userId = user.Id;
        Expression<Func<T, bool>> tenantCheck = x => x.TenantId == userId;
        var newPredicateBody = Expression.AndAlso(tenantCheck.Body, Expression.Invoke(predicate, tenantCheck.Parameters.Single()));
        var newPredicate = Expression.Lambda<Func<T, bool>>(newPredicateBody, tenantCheck.Parameters);
        return newPredicate;
    }
}