using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserLogin entity
/// </summary>
public class ApplicationUserLoginConfiguration : IEntityTypeConfiguration<ApplicationUserLogin>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
    {
        // Table name
        builder.ToTable("user_logins");
    }
}
