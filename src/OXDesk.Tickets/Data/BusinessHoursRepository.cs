using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Tickets;
using OXDesk.DbContext.Data;

namespace OXDesk.Tickets.Data;

/// <summary>
/// Repository implementation for business hours data access operations.
/// </summary>
public class BusinessHoursRepository : IBusinessHoursRepository
{
    private readonly TenantDbContext _context;
    private readonly ILogger<BusinessHoursRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessHoursRepository"/> class.
    /// </summary>
    /// <param name="context">The tenant database context.</param>
    /// <param name="logger">The logger.</param>
    public BusinessHoursRepository(TenantDbContext context, ILogger<BusinessHoursRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BusinessHours>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BusinessHours
            .Where(b => b.DeletedAt == null)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BusinessHours?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.BusinessHours
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BusinessHours> AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default)
    {
        await _context.BusinessHours.AddAsync(businessHours, cancellationToken);
        return businessHours;
    }

    /// <inheritdoc />
    public Task<BusinessHours> UpdateAsync(BusinessHours businessHours, CancellationToken cancellationToken = default)
    {
        _context.BusinessHours.Update(businessHours);
        return Task.FromResult(businessHours);
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _context.BusinessHours
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, cancellationToken);

        if (entity == null)
            return false;

        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        _context.BusinessHours.Update(entity);
        return true;
    }

    /// <inheritdoc />
    public async Task ClearDefaultsAsync(int excludeId, int updatedBy, CancellationToken cancellationToken = default)
    {
        var defaults = await _context.BusinessHours
            .Where(b => b.IsDefault && b.Id != excludeId && b.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var entity in defaults)
        {
            entity.IsDefault = false;
            entity.UpdatedBy = updatedBy;
            entity.UpdatedAt = now;
        }

        if (defaults.Count > 0)
            _context.BusinessHours.UpdateRange(defaults);
    }

    /// <inheritdoc />
    public async Task<bool> AnyExistAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BusinessHours
            .AnyAsync(b => b.DeletedAt == null, cancellationToken);
    }
}
