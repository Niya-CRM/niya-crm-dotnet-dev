using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Tickets;
using OXDesk.DbContext.Data;

namespace OXDesk.Tickets.Data;

/// <summary>
/// Repository implementation for custom business hours data access operations.
/// </summary>
public class CustomBusinessHoursRepository : ICustomBusinessHoursRepository
{
    private readonly TenantDbContext _context;
    private readonly ILogger<CustomBusinessHoursRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomBusinessHoursRepository"/> class.
    /// </summary>
    /// <param name="context">The tenant database context.</param>
    /// <param name="logger">The logger.</param>
    public CustomBusinessHoursRepository(TenantDbContext context, ILogger<CustomBusinessHoursRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomBusinessHours>> GetByBusinessHourIdAsync(int businessHourId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomBusinessHours
            .Where(c => c.BusinessHourId == businessHourId && c.DeletedAt == null)
            .OrderBy(c => c.Day)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CustomBusinessHours> AddAsync(CustomBusinessHours customBusinessHours, CancellationToken cancellationToken = default)
    {
        await _context.CustomBusinessHours.AddAsync(customBusinessHours, cancellationToken);
        return customBusinessHours;
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<CustomBusinessHours> items, CancellationToken cancellationToken = default)
    {
        await _context.CustomBusinessHours.AddRangeAsync(items, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteByBusinessHourIdAsync(int businessHourId, int deletedBy, CancellationToken cancellationToken = default)
    {
        var entities = await _context.CustomBusinessHours
            .Where(c => c.BusinessHourId == businessHourId && c.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
            return false;

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.DeletedAt = now;
            entity.DeletedBy = deletedBy;
        }

        _context.CustomBusinessHours.UpdateRange(entities);
        return true;
    }
}
