using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Decorators
{
    internal sealed class MovieQueryRepositoryDecorator : IMovieQueryRepository
    {
        private readonly IMovieQueryRepository _decorated;

        public MovieQueryRepositoryDecorator(IMovieQueryRepository decorated)
        {
            _decorated = decorated;
        }

        public Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, TenantId tenant, CancellationToken cancellationToken)
        {
            var newPredicate = GetPredicateWithTenantFilter(predicate, tenant);
            return _decorated.GetMoviesAsync(newPredicate, tenant, cancellationToken);
        }

        public Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEventsAsync(Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>> predicate, TenantId tenant,
            CancellationToken cancellationToken)
        {
            var newPredicate = GetPredicateWithTenantFilter(predicate, tenant);
            return _decorated.GetMoviesThatCanBeNominatedAgainEventsAsync(newPredicate, tenant, cancellationToken);
        }

        public Task<IReadOnlyNominatedMovieAgainEvent[]> GetMoviesNominatedAgainEventsAsync(Expression<Func<IReadOnlyNominatedMovieAgainEvent, bool>> predicate, TenantId tenant,
            CancellationToken cancellationToken)
        {
            var newPredicate = GetPredicateWithTenantFilter(predicate, tenant);
            return _decorated.GetMoviesNominatedAgainEventsAsync(newPredicate, tenant, cancellationToken);
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
