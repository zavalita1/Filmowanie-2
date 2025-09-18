using Filmowanie.Database.Contants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Filmowanie.Database.Entities.Voting;

namespace Filmowanie.Database.Contexts;

/// <summary>
/// The way this data model is defined, EF core can't handle tracking these entities, as they're missing shadow-id for one-to-many relation between embedded movie and it's votes.
/// </summary>
internal class VotingResultsContext : DbContext
{
    public DbSet<VotingResult> VotingResults { get; set; }

    public VotingResultsContext(DbContextOptions<VotingResultsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<VotingResult>()
            .ToContainer(DbContainerNames.Entities)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}