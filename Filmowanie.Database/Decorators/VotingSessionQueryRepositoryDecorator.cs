using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Decorators
{
    internal sealed class VotingSessionQueryRepositoryDecorator : IVotingSessionQueryRepository
    {
        private readonly IVotingSessionQueryRepository _decorated;

        public VotingSessionQueryRepositoryDecorator(IVotingSessionQueryRepository decorated)
        {
            _decorated = decorated;
        }

        public Task<IReadonlyVotingResult?> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, TenantId tenant, CancellationToken cancellationToken)
        {
            var newPredicate = GetPredicateWithTenantFilter(predicate, tenant);
            return _decorated.Get(newPredicate, tenant, cancellationToken);
        }

        public Task<IEnumerable<IReadonlyVotingResult>> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, TenantId tenant, Expression<Func<IReadonlyVotingResult, object>> sortBy, int take,
            CancellationToken cancellationToken)
        {
            var newPredicate = GetPredicateWithTenantFilter(predicate, tenant);
            return _decorated.Get(newPredicate, tenant, sortBy, take, cancellationToken);
        }

        public Task<IEnumerable<T>> Get<T>(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, T>> selector, TenantId tenant,
            CancellationToken cancellationToken) where T : class
        {
            var newPredicate = GetPredicateWithTenantFilter(predicate, tenant);
            return _decorated.Get(newPredicate, selector, tenant, cancellationToken);
        }

        private static Expression<Func<T, bool>> GetPredicateWithTenantFilter<T>(Expression<Func<T, bool>> predicate, TenantId user) where T: IReadOnlyEntity
        {
            Expression<Func<T, bool>> tenantCheck = x => x.TenantId == user.Id;
            var newPredicateBody = Expression.AndAlso(tenantCheck.Body, Expression.Invoke(predicate, tenantCheck.Parameters.Single()));
            var newPredicate = Expression.Lambda<Func<T, bool>>(newPredicateBody, tenantCheck.Parameters);
            return newPredicate;
        }
    }
}
