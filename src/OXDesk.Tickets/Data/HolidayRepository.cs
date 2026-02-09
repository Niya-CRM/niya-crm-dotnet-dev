using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Tickets;
using OXDesk.DbContext.Data;

namespace OXDesk.Tickets.Data;

/// <summary>
/// Repository implementation for holiday data access operations.
/// </summary>
public class HolidayRepository : IHolidayRepository
{
    private readonly TenantDbContext _context;
    private readonly ILogger<HolidayRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HolidayRepository"/> class.
    /// </summary>
    /// <param name="context">The tenant database context.</param>
    /// <param name="logger">The logger.</param>
    public HolidayRepository(TenantDbContext context, ILogger<HolidayRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Holiday>> GetByBusinessHourIdAsync(int businessHourId, CancellationToken cancellationToken = default)
    {
        return await _context.Holidays
            .Where(h => h.BusinessHourId == businessHourId && h.DeletedAt == null)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Holiday> AddAsync(Holiday holiday, CancellationToken cancellationToken = default)
    {
        await _context.Holidays.AddAsync(holiday, cancellationToken);
        return holiday;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Holidays
            .FirstOrDefaultAsync(h => h.Id == id && h.DeletedAt == null, cancellationToken);

        if (entity == null)
            return false;

        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        _context.Holidays.Update(entity);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteByBusinessHourIdAsync(int businessHourId, int deletedBy, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Holidays
            .Where(h => h.BusinessHourId == businessHourId && h.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
            return false;

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.DeletedAt = now;
            entity.DeletedBy = deletedBy;
        }

        _context.Holidays.UpdateRange(entities);
        return true;
    }
}
