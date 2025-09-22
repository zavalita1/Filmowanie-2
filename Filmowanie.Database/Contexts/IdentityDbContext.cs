using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Filmowanie.Database.Contexts;

internal class IdentityDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UserEntity>().ToContainer(DbContainerNames.Entities)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}