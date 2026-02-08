using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Core.ValueLists;
using OXDesk.Core.ValueLists.DTOs;
using OXDesk.Shared.Helpers;

namespace OXDesk.Identity.Factories;

/// <summary>
/// Builds user response payloads with enrichment, avoiding N+1 lookups.
/// </summary>
public sealed class UserFactory : IUserFactory
{
    private readonly IUserService _userService;
    private readonly IValueListService _valueListService;
    private readonly IUserSignatureService _signatureService;
    private readonly IUserSignatureFactory _signatureFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFactory"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="valueListService">The value list service.</param>
    /// <param name="signatureService">The user signature service.</param>
    /// <param name="signatureFactory">The user signature factory.</param>
    public UserFactory(
        IUserService userService,
        IValueListService valueListService,
        IUserSignatureService signatureService,
        IUserSignatureFactory signatureFactory)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _valueListService = valueListService ?? throw new ArgumentNullException(nameof(valueListService));
        _signatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));
        _signatureFactory = signatureFactory ?? throw new ArgumentNullException(nameof(signatureFactory));
    }

    /// <summary>
    /// Maps an ApplicationUser entity to a UserResponse DTO.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>The user response DTO.</returns>
    private static UserResponse MapToUserResponse(ApplicationUser user) => new UserResponse
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        UserName = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FirstName + " " + user.LastName,
        Location = user.Location,
        TimeZone = user.TimeZone ?? string.Empty,
        CountryCode = user.CountryCode,
        PhoneNumber = user.PhoneNumber,
        Profile = user.Profile,
        IsActive = user.IsActive == "Y",
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        CreatedBy = user.CreatedBy,
        UpdatedBy = user.UpdatedBy
    };

    /// <inheritdoc/>
    public async Task<PagedListWithRelatedResponse<UserResponse>> BuildListAsync(IEnumerable<ApplicationUser> users, CancellationToken cancellationToken = default)
    {
        var list = users.Select(MapToUserResponse).ToList();

        // Build related payload similar to UsersController: profiles + statuses
        var profiles = (await _valueListService.GetUserProfilesAsync(cancellationToken))
            .ToArray();
        var statuses = _valueListService.GetStatuses().ToArray();

        if (list.Count == 0)
        {
            return new PagedListWithRelatedResponse<UserResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = 0,
                Related = [
                    new UsersListRelated
                    {
                        Profiles = profiles,
                        Statuses = statuses
                    }
                ]
            };
        }

        // Value list lookups
        var countriesLookup = await _valueListService.GetCountriesLookupAsync(cancellationToken);
        var profilesLookup = await _valueListService.GetUserProfilesLookupAsync(cancellationToken);

        // Batch audit user lookup
        var auditUserIds = list
            .SelectMany(u => new[] { u.CreatedBy, u.UpdatedBy })
            .Where(id => id != 0)
            .Distinct()
            .ToArray();
        var auditUsers = await _userService.GetUsersLookupByIdsAsync(auditUserIds, cancellationToken);

        static string BuildDisplayName(ApplicationUser u)
        {
            var first = u.FirstName?.Trim();
            var last = u.LastName?.Trim();
            var full = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(full)) return full;
            return u.Email ?? u.UserName ?? u.Id.ToString();
        }

        foreach (var u in list)
        {
            u.CountryCodeText = !string.IsNullOrEmpty(u.CountryCode) && countriesLookup.TryGetValue(u.CountryCode, out var countryItem)
                ? countryItem.ItemName
                : null;
            u.ProfileText = !string.IsNullOrEmpty(u.Profile) && profilesLookup.TryGetValue(u.Profile, out var profileItem)
                ? profileItem.ItemName
                : null;

            if (auditUsers.TryGetValue(u.CreatedBy, out var createdByUser))
                u.CreatedByText = BuildDisplayName(createdByUser);
            else
                u.CreatedByText = await _userService.GetUserNameByIdAsync(u.CreatedBy, cancellationToken);

            if (auditUsers.TryGetValue(u.UpdatedBy, out var updatedByUser))
                u.UpdatedByText = BuildDisplayName(updatedByUser);
            else
                u.UpdatedByText = await _userService.GetUserNameByIdAsync(u.UpdatedBy, cancellationToken);
        }

        return new PagedListWithRelatedResponse<UserResponse>
        {
            Data = list,
            PageNumber = 1,
            RowCount = list.Count,
            Related =
            [
                new UsersListRelated
                {
                    Profiles = profiles,
                    Statuses = statuses
                }
            ]
        };
    }

    /// <inheritdoc/>
    public async Task<UserResponse> BuildItemAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var dto = MapToUserResponse(user);

        // Lookups
        var countriesLookup = await _valueListService.GetCountriesLookupAsync(cancellationToken);
        var profilesLookup = await _valueListService.GetUserProfilesLookupAsync(cancellationToken);

        dto.CountryCodeText = !string.IsNullOrEmpty(dto.CountryCode) && countriesLookup.TryGetValue(dto.CountryCode, out var countryItem)
            ? countryItem.ItemName
            : null;
        dto.ProfileText = !string.IsNullOrEmpty(dto.Profile) && profilesLookup.TryGetValue(dto.Profile, out var profileItem)
            ? profileItem.ItemName
            : null;

        // Audit names (batch-friendly even for single)
        var ids = new[] { dto.CreatedBy, dto.UpdatedBy }.Where(g => g != 0).Distinct();
        var lookup = await _userService.GetUsersLookupByIdsAsync(ids, cancellationToken);
        static string BuildDisplayName(ApplicationUser u)
        {
            var first = u.FirstName?.Trim();
            var last = u.LastName?.Trim();
            var full = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(full)) return full;
            return u.Email ?? u.UserName ?? u.Id.ToString();
        }
        if (lookup.TryGetValue(dto.CreatedBy, out var createdByUser))
            dto.CreatedByText = BuildDisplayName(createdByUser);
        else
            dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);

        if (lookup.TryGetValue(dto.UpdatedBy, out var updatedByUser))
            dto.UpdatedByText = BuildDisplayName(updatedByUser);
        else
            dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);

        return dto;
    }

    /// <inheritdoc/>
    public async Task<EntityWithRelatedResponse<UserResponse, UserDetailsRelated>> BuildDetailsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var dto = await BuildItemAsync(user, cancellationToken);

        // Related lists
        var countries = (await _valueListService.GetCountriesAsync(cancellationToken)).ToArray();
        var profiles = (await _valueListService.GetUserProfilesAsync(cancellationToken)).ToArray();
        var timeZones = TimeZoneHelper.GetAllIanaTimeZones()
            .Select(tz => new StringOption { Value = tz.Key, Name = tz.Value })
            .ToArray();
        var statuses = _valueListService.GetStatuses().ToArray();

        // Fetch user signature if exists
        var signatureEntity = await _signatureService.GetByUserIdAsync(user.Id, cancellationToken);
        var signatureDto = signatureEntity != null
            ? await _signatureFactory.BuildResponseAsync(signatureEntity, cancellationToken)
            : null;

        return new EntityWithRelatedResponse<UserResponse, UserDetailsRelated>
        {
            Data = dto,
            Related = new UserDetailsRelated
            {
                Countries = countries,
                Profiles = profiles,
                TimeZones = timeZones,
                Statuses = statuses,
                Signature = signatureDto
            }
        };
    }
}
