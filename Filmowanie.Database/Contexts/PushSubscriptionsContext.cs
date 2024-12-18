using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Filmowanie.Database.Contexts;

internal class PushSubscriptionsContext : DbContext
{
    public DbSet<PushSubscriptionEntity> Subscriptions { get; set; }

    public PushSubscriptionsContext(DbContextOptions<PushSubscriptionsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<PushSubscriptionEntity>().ToContainer(DbContainerNames.Events)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}