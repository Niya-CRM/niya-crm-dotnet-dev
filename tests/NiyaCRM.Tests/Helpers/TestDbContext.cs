using Microsoft.EntityFrameworkCore;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Core.ChangeHistory;
using NiyaCRM.Core.DynamicObjects;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Referentials;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.ValueLists;
using NiyaCRM.Infrastructure.Data;
using System;

namespace NiyaCRM.Tests.Helpers
{
    /// <summary>
    /// A test double for ApplicationDbContext that can be used in unit tests
    /// </summary>
    public class TestDbContext : DbContext
    {
        public TestDbContext() { }

        // Add DbSet properties for all entities used in tests
        public DbSet<ChangeHistoryLog> ChangeHistoryLogs { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
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
