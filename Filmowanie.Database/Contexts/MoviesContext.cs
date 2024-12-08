using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Filmowanie.Database.Contexts;

internal class MoviesContext : DbContext
{
    public DbSet<MovieEntity> Movies { get; set; }
    public DbSet<CanNominateMovieAgainEvent> CanNominateMovieAgainEvents { get; set; }
    public DbSet<NominatedMovieAgainEvent> NominatedMovieAgainEvents { get; set; }

    public MoviesContext(DbContextOptions<MoviesContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<MovieEntity>().ToContainer(DbContainerNames.Entities)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<CanNominateMovieAgainEvent>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<NominatedMovieAgainEvent>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}