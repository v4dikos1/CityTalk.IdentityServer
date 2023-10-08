using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Domain.Entities;
using Persistence.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Persistence;

public class IdentityContext : IdentityDbContext<ApplicationUser>
{
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityContext).GetTypeInfo().Assembly);
        builder.ConvertTableNameToSnakeCase();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }
}