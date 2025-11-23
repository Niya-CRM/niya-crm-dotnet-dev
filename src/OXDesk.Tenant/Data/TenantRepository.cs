using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Tenants;
using OXDesk.Core.Common;
using OXDesk.Infrastructure.Data;

namespace OXDesk.Tenant.Data;

/// <summary>
/// Repository implementation for tenant data access operations using Entity Framework.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly TenantDbContext _dbContext;
    private readonly ILogger<TenantRepository> _logger;
    private readonly DbSet<OXDesk.Core.Tenants.Tenant> _dbSet;

    /// <summary>
    /// Initializes a new instance of the TenantRepository.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public TenantRepository(TenantDbContext dbContext, ILogger<TenantRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);
        
        _dbContext = dbContext;
        _logger = logger;
        _dbSet = dbContext.Set<OXDesk.Core.Tenants.Tenant>();
    }

    /// <inheritdoc />
    public async Task<OXDesk.Core.Tenants.Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by ID: {TenantId}", id);
        
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OXDesk.Core.Tenants.Tenant?> GetByHostAsync(string host, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        _logger.LogDebug("Getting tenant by host: {Host}", host);
        
        var normalizedHost = host.Trim().ToUpperInvariant();
        return await _dbSet.FirstOrDefaultAsync(t => t.Host == normalizedHost, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OXDesk.Core.Tenants.Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        _logger.LogDebug("Getting tenant by email: {Email}", email);
        
        var normalizedEmail = email.Trim().ToUpperInvariant();
        return await _dbSet.FirstOrDefaultAsync(t => t.Email == normalizedEmail, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OXDesk.Core.Tenants.Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active tenants");
        
        return await _dbSet.Where(t => t.IsActive == "Y")
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OXDesk.Core.Tenants.Tenant>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        if (pageNumber < CommonConstant.PAGE_NUMBER_DEFAULT)
            throw new ArgumentException($"Page number must be greater than {CommonConstant.PAGE_NUMBER_DEFAULT}.", nameof(pageNumber));
        
        if (pageSize < CommonConstant.PAGE_SIZE_MIN || pageSize > CommonConstant.PAGE_SIZE_MAX)
            throw new ArgumentException($"Page size must be between {CommonConstant.PAGE_SIZE_MIN} and {CommonConstant.PAGE_SIZE_MAX}.", nameof(pageSize));

        _logger.LogDebug("Getting tenants - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        
        return await _dbSet
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OXDesk.Core.Tenants.Tenant> AddAsync(OXDesk.Core.Tenants.Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        _logger.LogDebug("Adding new tenant: {TenantName} with host: {Host}", tenant.Name, tenant.Host);

        // Normalize Email & Host to UpperCase
        tenant.Host = tenant.Host.ToUpperInvariant();
        tenant.Email = tenant.Email.ToUpperInvariant();
        
        var entry = await _dbSet.AddAsync(tenant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully added tenant with ID: {TenantId}", tenant.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<OXDesk.Core.Tenants.Tenant> UpdateAsync(OXDesk.Core.Tenants.Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        _logger.LogDebug("Updating tenant: {TenantId}", tenant.Id);

        // Normalize Email & Host to UpperCase
        tenant.Host = tenant.Host.ToUpperInvariant();
        tenant.Email = tenant.Email.ToUpperInvariant();
        
        var entry = _dbSet.Update(tenant);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully updated tenant: {TenantId}", tenant.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByHostAsync(string host, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        _logger.LogDebug("Checking if host exists: {Host}, excluding ID: {ExcludeId}", host, excludeId);
        
        var normalizedHost = host.Trim().ToUpperInvariant();
        var query = _dbSet.Where(t => t.Host == normalizedHost);
        
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
        
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var query = _dbSet.Where(t => t.Email == normalizedEmail);
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
