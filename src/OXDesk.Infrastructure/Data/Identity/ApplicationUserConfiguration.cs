using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace OXDesk.Infrastructure.Data.Identity
{
    /// <summary>
    /// EF Core configuration for ApplicationUser.
    /// </summary>
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Configure Id to start from 10001
            builder.Property(u => u.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            // Location: required, max length 60, default empty string to support existing rows during migration
            builder.Property(u => u.Location)
                   .HasMaxLength(60)
                   .HasColumnType("varchar(60)")
                   .IsRequired()
                   .HasDefaultValue(string.Empty);
                   
            // MiddleName: nullable, max length 30
            builder.Property(u => u.MiddleName)
                   .HasMaxLength(30)
                   .HasColumnType("varchar(30)");

            // MobileNumber: nullable, max length 20
            builder.Property(u => u.MobileNumber)
                   .HasMaxLength(20)
                   .HasColumnType("varchar(20)");

            // JobTitle: nullable, max length 100
            builder.Property(u => u.JobTitle)
                   .HasMaxLength(100)
                   .HasColumnType("varchar(100)");

            // Language: nullable, max length 10 (value list item key, e.g. "en-US")
            builder.Property(u => u.Language)
                   .HasMaxLength(10)
                   .HasColumnType("varchar(10)");

            // Add tenant_id index
            builder.HasIndex(u => u.TenantId)
                .HasDatabaseName("ix_asp_net_users_tenant_id");
                
            // Add composite indexes with tenant_id
            builder.HasIndex(u => new { u.TenantId, u.NormalizedUserName })
                .HasDatabaseName("ix_asp_net_users_tenant_id_normalized_user_name")
                .IsUnique();
                
            builder.HasIndex(u => new { u.TenantId, u.NormalizedEmail })
                .HasDatabaseName("ix_asp_net_users_tenant_id_normalized_email");
        }
    }
}
