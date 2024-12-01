using Filmowanie.Database.Contants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Filmowanie.Database.Entities.Voting;

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