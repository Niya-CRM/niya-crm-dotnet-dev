using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.AuditLogs.ChangeHistory;

namespace OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Entity Framework configuration for the ChangeHistoryLog entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class ChangeHistoryLogConfiguration : IEntityTypeConfiguration<ChangeHistoryLog>
    {
        public void Configure(EntityTypeBuilder<ChangeHistoryLog> builder)
        {            
            builder.HasKey(o => o.Id);
        }
    }
}
