using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Filmowanie.Database.Contexts;

internal class MoviesContext : DbContext
{
    public DbSet<MovieEntity> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<MovieEntity>().ToContainer(DbContainerNames.Entities)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}