using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for UserSignature entity.
/// </summary>
public class UserSignatureConfiguration : IEntityTypeConfiguration<UserSignature>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserSignature> builder)
    {
        builder.ToTable("user_signatures");

        builder.HasKey(us => us.Id);

        builder.Property(us => us.Id)
               .UseIdentityByDefaultColumn()
               .HasIdentityOptions(startValue: 10001L);

        builder.HasIndex(us => us.UserId)
               .HasDatabaseName("ix_user_signatures_user_id");

        builder.Property(us => us.FreeStyleSignature)
               .HasMaxLength(30000);

        builder.HasOne(us => us.User)
               .WithMany()
               .HasForeignKey(us => us.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
