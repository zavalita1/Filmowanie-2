using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Filmowanie.Database.Contexts;

internal class VotingResultsContext : DbContext
{
    public DbSet<VotingResult> VotingResults { get; set; }

    public VotingResultsContext(DbContextOptions<VotingResultsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<VotingResult>().ToContainer(DbContainerNames.Entities)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}