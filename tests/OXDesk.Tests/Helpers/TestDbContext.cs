using Microsoft.EntityFrameworkCore;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.Identity;
using OXDesk.Core.Tenants;
using OXDesk.Core.ValueLists;
using System;

namespace OXDesk.Tests.Helpers
{
    /// <summary>
    /// A test double for ApplicationDbContext that can be used in unit tests
    /// </summary>
    public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public TestDbContext() { }

        // Add DbSet properties for all entities used in tests
        public DbSet<ChangeHistoryLog> ChangeHistoryLogs { get; set; } = null!;
        public DbSet<OXDesk.Core.Tenants.Tenant> Tenants { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<DynamicObject> DynamicObjects { get; set; } = null!;
        public DbSet<ValueList> ValueLists { get; set; } = null!;
        public DbSet<ValueListItem> ValueListItems { get; set; } = null!;
        public DbSet<ApplicationUser> Users { get; set; } = null!;
        public DbSet<ApplicationRole> Roles { get; set; } = null!;

        // Override to avoid actual database operations
        public override int SaveChanges() => 1;
        public override int SaveChanges(bool acceptAllChangesOnSuccess) => 1;
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}
