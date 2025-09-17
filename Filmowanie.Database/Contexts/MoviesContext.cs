using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Filmowanie.Database.Contexts;

internal class MoviesContext : DbContext
{
    public DbSet<MovieEntity> Movies { get; set; }
    public DbSet<CanNominateMovieAgainEvent> CanNominateMovieAgainEvents { get; set; }
    public DbSet<NominatedMovieEvent> NominatedMovieEvents { get; set; }

    public MoviesContext(DbContextOptions<MoviesContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<MovieEntity>().ToContainer(DbContainerNames.Entities)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<CanNominateMovieAgainEvent>()
            .HasDefaultTimeToLive(60*60*24*365) // a year
            .ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<NominatedMovieEvent>()
            .ToContainer(DbContainerNames.Events)
            .HasDefaultTimeToLive(60*60*24*365) // a year
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}