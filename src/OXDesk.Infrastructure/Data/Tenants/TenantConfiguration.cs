using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tenants;

namespace OXDesk.Infrastructure.Data.Tenants
{
    /// <summary>
    /// Entity Framework configuration for the Tenant entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("tenants");

            builder.HasKey(t => t.Id);

            builder.HasIndex(t => t.Host)
                   .HasDatabaseName("ix_tenants_host")
                   .IsUnique();

            builder.HasIndex(t => t.Email)
                   .HasDatabaseName("ix_tenants_email");
        }
    }
}
