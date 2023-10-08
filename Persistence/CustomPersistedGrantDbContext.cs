using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Persistence;

public class CustomPersistedGrantDbContext : PersistedGrantDbContext<CustomPersistedGrantDbContext>
{
    public CustomPersistedGrantDbContext(DbContextOptions options, OperationalStoreOptions storeOptions) : base(
        options, storeOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConvertTableNameToSnakeCase();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }
}