using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Decorators
{
    internal sealed class VotingSessionQueryRepositoryDecorator : IVotingSessionQueryRepository
    {
        private readonly IVotingSessionQueryRepository _decorated;
        private readonly ICurrentUserAccessor _currentUserAccessor;

        public VotingSessionQueryRepositoryDecorator(IVotingSessionQueryRepository decorated, ICurrentUserAccessor currentUserAccessor)
        {
            _decorated = decorated;
            _currentUserAccessor = currentUserAccessor;
        }

        public Task<IReadOnlyVotingResult?> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, CancellationToken cancelToken)
        {
            var currentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
            var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
            return _decorated.Get(newPredicate, cancelToken);
        }

        public Task<IEnumerable<IReadOnlyVotingResult>> GetAll(Expression<Func<IReadOnlyVotingResult, bool>> predicate, CancellationToken cancelToken)
        {
            var currentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
            var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
            return _decorated.GetAll(newPredicate, cancelToken);
        }

        public Task<IEnumerable<IReadOnlyVotingResult>> GetVotingResultAsync(Expression<Func<IReadOnlyVotingResult, bool>> predicate, Expression<Func<IReadOnlyVotingResult, object>> sortBy, int take,
            CancellationToken cancelToken)
        {
            var currentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
            var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
            return _decorated.GetVotingResultAsync(newPredicate, sortBy, take, cancelToken);
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
