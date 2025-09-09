using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Decorators
{
    internal sealed class MovieQueryRepositoryDecorator : IMovieQueryRepository
    {
        private readonly IMovieQueryRepository _decorated;
        private readonly ICurrentUserAccessor _currentUserAccessor;

        public MovieQueryRepositoryDecorator(IMovieQueryRepository decorated, ICurrentUserAccessor currentUserAccessor)
        {
            _decorated = decorated;
            _currentUserAccessor = currentUserAccessor;
        }

        public Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, CancellationToken cancelToken)
        {
            var currentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
            var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
            return _decorated.GetMoviesAsync(newPredicate, cancelToken);
        }

        public Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEventsAsync(Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>> predicate,
            CancellationToken cancelToken)
        {
            var currentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
            var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
            return _decorated.GetMoviesThatCanBeNominatedAgainEventsAsync(newPredicate, cancelToken);
        }

        public Task<IReadOnlyNominatedMovieEvent[]> GetMoviesNominatedAgainEventsAsync(Expression<Func<IReadOnlyNominatedMovieEvent, bool>> predicate,
            CancellationToken cancelToken)
        {
            var currentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void).Result;
            var newPredicate = GetPredicateWithTenantFilter(predicate, currentUser.Tenant);
            return _decorated.GetMoviesNominatedAgainEventsAsync(newPredicate, cancelToken);
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
