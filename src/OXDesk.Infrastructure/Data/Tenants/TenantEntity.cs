using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tenants;

namespace OXDesk.Infrastructure.Data.Tenants;

/// <summary>
/// Entity Framework configuration for the Tenant entity.
/// Configures database table and column mappings.
/// </summary>
public class TenantEntity : IEntityTypeConfiguration<Tenant>
{
    /// <summary>
    /// Configures the Tenant entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Table name
        builder.ToTable("tenants");

        // Primary key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        // Name property
        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        // Host property
        builder.Property(t => t.Host)
            .HasColumnName("host")
            .HasMaxLength(100)
            .IsRequired();

        // Email property
        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired();

        // TimeZone property
        builder.Property(t => t.TimeZone)
            .HasColumnName("time_zone")
            .HasMaxLength(100)
            .IsRequired();

        // Database name property
        builder.Property(t => t.DatabaseName)
            .HasColumnName("database_name")
            .HasMaxLength(100)
            .IsRequired(false);

        // IsActive property
        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // CreatedAt property
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // LastModifiedAt property
        builder.Property(t => t.LastModifiedAt)
            .HasColumnName("last_modified_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // CreatedBy property
        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        // LastModifiedBy property
        builder.Property(t => t.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.Host)
            .IsUnique()
            .HasDatabaseName("ix_tenants_host");

        builder.HasIndex(t => t.Name)
            .HasDatabaseName("ix_tenants_name");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("ix_tenants_is_active");
    }
}
