using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities.Events;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Filmowanie.Database.Contexts;

internal class EventsContext : DbContext
{
    public DbSet<VotingStartedEvent> VotingStatedEvents { get; set; }
    public DbSet<VotingConcludedEvent> VotingConcludedEvents { get; set; }
    public DbSet<VoteAddedEvent> VoteAddedEvents { get; set; }
    public DbSet<VoteRemovedEvent> VoteRemovedEvents { get; set; }

    public EventsContext(DbContextOptions<EventsContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<VotingStartedEvent>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<VotingConcludedEvent>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<VoteAddedEvent>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<VoteRemovedEvent>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}