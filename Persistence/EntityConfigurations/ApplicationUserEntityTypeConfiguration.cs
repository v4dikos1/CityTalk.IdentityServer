using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

internal class ApplicationUserEntityTypeConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.Surname)
            .IsRequired();

        builder.Property(x => x.PhoneNumberSequence)
            .IsRequired()
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.DateBirth)
            .IsRequired(false);

        builder.Property(x => x.IsActivated)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.SendingCodeTime)
            .IsRequired(false);
    }
}