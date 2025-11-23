using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using System.Linq;

namespace OXDesk.Tenant.Factories;

public class TenantFactory : ITenantFactory
{
    private readonly IUserService _userService;

    public TenantFactory(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<PagedListWithRelatedResponse<TenantResponse>> BuildListAsync(IEnumerable<OXDesk.Core.Tenants.Tenant> tenants, int pageNumber, CancellationToken cancellationToken = default)
    {
        var list = tenants?.ToList() ?? new List<OXDesk.Core.Tenants.Tenant>();
        var dtoList = new List<TenantResponse>(list.Count);

        // Collect userIds for enrichment
        var userIds = list
            .SelectMany(t => new[] { t.CreatedBy, t.UpdatedBy })
            .Distinct()
            .ToArray();

        var usersLookup = await _userService.GetUsersLookupByIdsAsync(userIds, cancellationToken);

        static string BuildDisplayName(ApplicationUser u)
        {
            var first = u.FirstName?.Trim();
            var last = u.LastName?.Trim();
            var full = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(full)) return full;
            return u.Email ?? u.UserName ?? u.Id.ToString();
        }

        foreach (var t in list)
        {
            dtoList.Add(new TenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                Host = t.Host,
                Email = t.Email,
                TimeZone = t.TimeZone,
                UserId = t.UserId,
                DatabaseName = t.DatabaseName,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                DeletedAt = t.DeletedAt,
                CreatedBy = t.CreatedBy,
                UpdatedBy = t.UpdatedBy,
                CreatedByText = usersLookup.TryGetValue(t.CreatedBy, out var cu) ? BuildDisplayName(cu) : null,
                UpdatedByText = usersLookup.TryGetValue(t.UpdatedBy, out var mu) ? BuildDisplayName(mu) : null
            });
        }

        return new PagedListWithRelatedResponse<TenantResponse>
        {
            Data = dtoList,
            PageNumber = pageNumber,
            RowCount = dtoList.Count,
            Related = Array.Empty<object>()
        };
    }

    public async Task<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>> BuildDetailsAsync(OXDesk.Core.Tenants.Tenant tenant, CancellationToken cancellationToken = default)
    {
        var createdByText = await _userService.GetUserNameByIdAsync(tenant.CreatedBy, cancellationToken);
        var updatedByText = await _userService.GetUserNameByIdAsync(tenant.UpdatedBy, cancellationToken);

        var dto = new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Host = tenant.Host,
            Email = tenant.Email,
            TimeZone = tenant.TimeZone,
            UserId = tenant.UserId,
            DatabaseName = tenant.DatabaseName,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            DeletedAt = tenant.DeletedAt,
            CreatedBy = tenant.CreatedBy,
            UpdatedBy = tenant.UpdatedBy,
            CreatedByText = createdByText,
            UpdatedByText = updatedByText
        };

        return new EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>
        {
            Data = dto,
            Related = new TenantDetailsRelated()
        };
    }

    public TenantPublicResponse BuildPublicResponse(OXDesk.Core.Tenants.Tenant tenant)
    {
        return new TenantPublicResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Host = tenant.Host,
            TimeZone = tenant.TimeZone,
            IsActive = tenant.IsActive
        };
    }
}
