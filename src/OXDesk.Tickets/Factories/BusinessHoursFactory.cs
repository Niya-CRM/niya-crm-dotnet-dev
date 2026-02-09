using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Tickets;
using OXDesk.Core.Tickets.DTOs;

namespace OXDesk.Tickets.Factories;

/// <summary>
/// Factory implementation for building business hours DTOs.
/// </summary>
public class BusinessHoursFactory : IBusinessHoursFactory
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessHoursFactory"/> class.
    /// </summary>
    /// <param name="userService">The user service for resolving display names.</param>
    public BusinessHoursFactory(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <inheritdoc />
    public async Task<List<BusinessHoursResponse>> BuildListAsync(
        IEnumerable<BusinessHours> businessHours,
        CancellationToken cancellationToken = default)
    {
        var list = businessHours?.ToList() ?? new List<BusinessHours>();
        var dtoList = new List<BusinessHoursResponse>(list.Count);

        var userIds = list
            .SelectMany(b => new[] { b.CreatedBy, b.UpdatedBy })
            .Distinct()
            .ToArray();

        var usersLookup = await _userService.GetUsersLookupByIdsAsync(userIds, cancellationToken);

        foreach (var b in list)
        {
            dtoList.Add(new BusinessHoursResponse
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                IsDefault = b.IsDefault,
                TimeZone = b.TimeZone,
                BusinessHoursType = b.BusinessHoursType,
                DeletedAt = b.DeletedAt,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                CreatedBy = b.CreatedBy,
                UpdatedBy = b.UpdatedBy,
                CreatedByText = usersLookup.TryGetValue(b.CreatedBy, out var cu) ? BuildDisplayName(cu) : null,
                UpdatedByText = usersLookup.TryGetValue(b.UpdatedBy, out var mu) ? BuildDisplayName(mu) : null
            });
        }

        return dtoList;
    }

    /// <inheritdoc />
    public async Task<EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>> BuildDetailsAsync(
        BusinessHours businessHours,
        IEnumerable<CustomBusinessHours> customBusinessHours,
        IEnumerable<Holiday> holidays,
        CancellationToken cancellationToken = default)
    {
        var userIds = new[] { businessHours.CreatedBy, businessHours.UpdatedBy }.Distinct().ToArray();
        var usersLookup = await _userService.GetUsersLookupByIdsAsync(userIds, cancellationToken);

        var dto = new BusinessHoursResponse
        {
            Id = businessHours.Id,
            Name = businessHours.Name,
            Description = businessHours.Description,
            IsDefault = businessHours.IsDefault,
            TimeZone = businessHours.TimeZone,
            BusinessHoursType = businessHours.BusinessHoursType,
            DeletedAt = businessHours.DeletedAt,
            CreatedAt = businessHours.CreatedAt,
            UpdatedAt = businessHours.UpdatedAt,
            CreatedBy = businessHours.CreatedBy,
            UpdatedBy = businessHours.UpdatedBy,
            CreatedByText = usersLookup.TryGetValue(businessHours.CreatedBy, out var cu) ? BuildDisplayName(cu) : null,
            UpdatedByText = usersLookup.TryGetValue(businessHours.UpdatedBy, out var mu) ? BuildDisplayName(mu) : null
        };

        var dayOrder = BusinessHoursConstant.DayOfWeekNames.All;

        var related = new BusinessHoursDetailsRelated
        {
            CustomBusinessHours = customBusinessHours
                .OrderBy(c => Array.IndexOf(dayOrder, c.Day))
                .ThenBy(c => c.StartTime)
                .Select(c => new CustomBusinessHoursResponse
                {
                    Id = c.Id,
                    BusinessHourId = c.BusinessHourId,
                    Day = c.Day,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy,
                    UpdatedBy = c.UpdatedBy
                }).ToList(),
            Holidays = holidays
                .OrderBy(h => h.Date)
                .Select(h => new HolidayResponse
                {
                    Id = h.Id,
                    BusinessHourId = h.BusinessHourId,
                    Name = h.Name,
                    Date = h.Date,
                    CreatedAt = h.CreatedAt,
                    UpdatedAt = h.UpdatedAt,
                    CreatedBy = h.CreatedBy,
                    UpdatedBy = h.UpdatedBy
                }).ToList()
        };

        return new EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>
        {
            Data = dto,
            Related = related
        };
    }

    private static string BuildDisplayName(ApplicationUser u)
    {
        var first = u.FirstName?.Trim();
        var last = u.LastName?.Trim();
        var full = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrEmpty(s)));
        if (!string.IsNullOrEmpty(full)) return full;
        return u.Email ?? u.UserName ?? u.Id.ToString();
    }
}
