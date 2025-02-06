using Filmowanie.Database.Contants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Filmowanie.Database.Entities;

namespace Filmowanie.Database.Contexts;

internal class PushSubscriptionsContext : DbContext
{
    public DbSet<ReadOnlyPushSubscriptionEntity> Subscriptions { get; set; }

    public PushSubscriptionsContext(DbContextOptions<PushSubscriptionsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ReadOnlyPushSubscriptionEntity>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}