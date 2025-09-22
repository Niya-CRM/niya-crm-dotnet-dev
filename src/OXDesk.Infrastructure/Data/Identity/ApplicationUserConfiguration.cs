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
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Start identity values for user Ids at 10001
            builder.Property(u => u.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            // Location: required, max length 60, default empty string to support existing rows during migration
            builder.Property(u => u.Location)
                   .HasMaxLength(60)
                   .HasColumnType("varchar(60)")
                   .IsRequired()
                   .HasDefaultValue(string.Empty);
                   
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
