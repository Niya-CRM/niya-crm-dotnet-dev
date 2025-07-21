using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.Common;

namespace NiyaCRM.Infrastructure.Data.Tenants;

/// <summary>
/// Repository implementation for tenant data access operations using Entity Framework.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TenantRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the TenantRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    public TenantRepository(ApplicationDbContext context, ILogger<TenantRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by ID: {TenantId}", id);
        
        return await _context.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByHostAsync(string host, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        _logger.LogDebug("Getting tenant by host: {Host}", host);
        
        var normalizedHost = host.Trim().ToLowerInvariant();
        return await _context.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Host == normalizedHost, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        _logger.LogDebug("Getting tenant by email: {Email}", email);
        
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Email == normalizedEmail, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active tenants");
        
        return await _context.Set<Tenant>()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        if (pageNumber < CommonConstant.PAGE_NUMBER_DEFAULT)
            throw new ArgumentException($"Page number must be greater than {CommonConstant.PAGE_NUMBER_DEFAULT}.", nameof(pageNumber));
        
        if (pageSize < CommonConstant.PAGE_SIZE_MIN || pageSize > CommonConstant.PAGE_SIZE_MAX)
            throw new ArgumentException($"Page size must be between {CommonConstant.PAGE_SIZE_MIN} and {CommonConstant.PAGE_SIZE_MAX}.", nameof(pageSize));

        _logger.LogDebug("Getting tenants - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        
        return await _context.Set<Tenant>()
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        _logger.LogDebug("Adding new tenant: {TenantName} with host: {Host}", tenant.Name, tenant.Host);
        
        var entry = await _context.Set<Tenant>().AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully added tenant with ID: {TenantId}", tenant.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        _logger.LogDebug("Updating tenant: {TenantId}", tenant.Id);
        
        var entry = _context.Set<Tenant>().Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully updated tenant: {TenantId}", tenant.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting tenant: {TenantId}", id);
        
        var tenant = await _context.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for deletion: {TenantId}", id);
            return false;
        }

        _context.Set<Tenant>().Remove(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully deleted tenant: {TenantId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByHostAsync(string host, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        _logger.LogDebug("Checking if host exists: {Host}, excluding ID: {ExcludeId}", host, excludeId);
        
        var normalizedHost = host.Trim().ToLowerInvariant();
        var query = _context.Set<Tenant>().Where(t => t.Host == normalizedHost);
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        _logger.LogDebug("Checking if email exists: {Email}, excluding ID: {ExcludeId}", email, excludeId);
        
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = _context.Set<Tenant>().Where(t => t.Email == normalizedEmail);
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
