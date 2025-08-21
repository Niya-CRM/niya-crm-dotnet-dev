using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using System.Linq;

namespace OXDesk.Api.Factories.Tenants;

public class TenantFactory : ITenantFactory
{
    private readonly IUserService _userService;

    public TenantFactory(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<PagedListWithRelatedResponse<TenantResponse>> BuildListAsync(IEnumerable<Tenant> tenants, int pageNumber, CancellationToken cancellationToken = default)
    {
        var list = tenants?.ToList() ?? new List<Tenant>();
        var dtoList = new List<TenantResponse>(list.Count);

        // Collect userIds for enrichment
        var userIds = list
            .SelectMany(t => new[] { t.CreatedBy, t.LastModifiedBy })
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
                LastModifiedAt = t.LastModifiedAt,
                DeletedAt = t.DeletedAt,
                CreatedBy = t.CreatedBy,
                LastModifiedBy = t.LastModifiedBy,
                CreatedByText = usersLookup.TryGetValue(t.CreatedBy, out var cu) ? BuildDisplayName(cu) : null,
                LastModifiedByText = usersLookup.TryGetValue(t.LastModifiedBy, out var mu) ? BuildDisplayName(mu) : null
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

    public async Task<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>> BuildDetailsAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var createdByText = await _userService.GetUserNameByIdAsync(tenant.CreatedBy, cancellationToken);
        var modifiedByText = await _userService.GetUserNameByIdAsync(tenant.LastModifiedBy, cancellationToken);

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
            LastModifiedAt = tenant.LastModifiedAt,
            DeletedAt = tenant.DeletedAt,
            CreatedBy = tenant.CreatedBy,
            LastModifiedBy = tenant.LastModifiedBy,
            CreatedByText = createdByText,
            LastModifiedByText = modifiedByText
        };

        return new EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>
        {
            Data = dto,
            Related = new TenantDetailsRelated()
        };
    }
}
