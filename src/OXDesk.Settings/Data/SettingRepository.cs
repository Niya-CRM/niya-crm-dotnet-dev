using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Settings;
using OXDesk.DbContext.Data;

namespace OXDesk.Settings.Data;

/// <summary>
/// Repository implementation for setting data access operations.
/// </summary>
public class SettingRepository : ISettingRepository
{
    private readonly TenantDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingRepository"/> class.
    /// </summary>
    /// <param name="context">The tenant database context.</param>
    public SettingRepository(TenantDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Setting> AddAsync(Setting entity, CancellationToken cancellationToken = default)
    {
        await _context.Settings.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public async Task<Setting> UpdateAsync(Setting entity, CancellationToken cancellationToken = default)
    {
        _context.Settings.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
