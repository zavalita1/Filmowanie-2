using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace Filmowanie.Database.Contexts;

internal class IdentityDbContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<UserEntity> Users { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    { }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UserEntity>().ToContainer(DbContainerNames.Entities)
            .HasPartitionKey(x => x.id)
            .HasDiscriminator(x => x.Type);

        builder.Entity<DataProtectionKey>().ToContainer(DbContainerNames.Entities)
        .HasPartitionKey(x => x.Id)
        .HasDiscriminator(_ => "DataProtectionKey");

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}