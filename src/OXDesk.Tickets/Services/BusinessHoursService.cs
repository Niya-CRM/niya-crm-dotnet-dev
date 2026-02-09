using Microsoft.Extensions.Logging;
using OXDesk.Core;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.Identity;
using OXDesk.Core.Tickets;
using OXDesk.Core.Tickets.DTOs;

namespace OXDesk.Tickets.Services;

/// <summary>
/// Service implementation for business hours business operations.
/// </summary>
public class BusinessHoursService : IBusinessHoursService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IChangeHistoryLogService _changeHistoryLogService;
    private readonly ILogger<BusinessHoursService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessHoursService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="currentUser">The current user accessor.</param>
    /// <param name="changeHistoryLogService">The change history log service.</param>
    /// <param name="logger">The logger.</param>
    public BusinessHoursService(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IChangeHistoryLogService changeHistoryLogService,
        ILogger<BusinessHoursService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _changeHistoryLogService = changeHistoryLogService ?? throw new ArgumentNullException(nameof(changeHistoryLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private int GetCurrentUserId()
    {
        return _currentUser.Id ?? throw new InvalidOperationException("Current user ID is null.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BusinessHours>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<IBusinessHoursRepository>();
        return await repository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BusinessHours?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<IBusinessHoursRepository>();
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BusinessHours> CreateAsync(CreateBusinessHoursRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;
        var repository = _unitOfWork.GetRepository<IBusinessHoursRepository>();

        var anyExist = await repository.AnyExistAsync(cancellationToken);
        var isDefault = !anyExist || request.IsDefault;

        var entity = new BusinessHours
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = isDefault,
            TimeZone = request.TimeZone ?? TimeZoneInfo.Local.Id,
            BusinessHoursType = request.BusinessHoursType,
            CreatedBy = userId,
            UpdatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var created = await repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (created.IsDefault)
                await repository.ClearDefaultsAsync(created.Id, userId, cancellationToken);

            if (request.BusinessHoursType != BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven
                && request.CustomHours is { Count: > 0 })
            {
                var customHoursRepo = _unitOfWork.GetRepository<ICustomBusinessHoursRepository>();
                var customEntities = request.CustomHours.Select(item => new CustomBusinessHours
                {
                    BusinessHourId = created.Id,
                    Day = item.Day,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                }).ToList();
                await customHoursRepo.AddRangeAsync(customEntities, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                objectKey: DynamicObjectConstants.DynamicObjectKeys.BusinessHours,
                objectItemId: created.Id,
                fieldName: "created",
                oldValue: null,
                newValue: created.Name,
                createdBy: userId,
                cancellationToken: cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Created business hours: {BusinessHoursId} - {Name}", created.Id, created.Name);
            return created;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BusinessHours?> UpdateAsync(int id, PatchBusinessHoursRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;
        var repository = _unitOfWork.GetRepository<IBusinessHoursRepository>();
        var customHoursRepo = _unitOfWork.GetRepository<ICustomBusinessHoursRepository>();

        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            return null;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.Name != null) existing.Name = request.Name;
            if (request.Description != null) existing.Description = request.Description;
            if (request.TimeZone != null) existing.TimeZone = request.TimeZone;

            if (request.IsDefault.HasValue)
                existing.IsDefault = request.IsDefault.Value;

            if (request.BusinessHoursType != null)
                existing.BusinessHoursType = request.BusinessHoursType;

            var effectiveType = existing.BusinessHoursType;

            if (effectiveType == BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven)
            {
                await customHoursRepo.SoftDeleteByBusinessHourIdAsync(id, userId, cancellationToken);
            }
            else if (request.CustomHours != null)
            {
                await customHoursRepo.SoftDeleteByBusinessHourIdAsync(id, userId, cancellationToken);

                if (request.CustomHours.Count > 0)
                {
                    var customEntities = request.CustomHours.Select(item => new CustomBusinessHours
                    {
                        BusinessHourId = id,
                        Day = item.Day,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    }).ToList();
                    await customHoursRepo.AddRangeAsync(customEntities, cancellationToken);
                }
            }

            existing.UpdatedBy = userId;
            existing.UpdatedAt = now;

            await repository.UpdateAsync(existing, cancellationToken);

            if (existing.IsDefault)
                await repository.ClearDefaultsAsync(existing.Id, userId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                objectKey: DynamicObjectConstants.DynamicObjectKeys.BusinessHours,
                objectItemId: id,
                fieldName: "updated",
                oldValue: null,
                newValue: existing.Name,
                createdBy: userId,
                cancellationToken: cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Updated business hours: {Id}", id);
            return existing;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var businessHoursRepo = _unitOfWork.GetRepository<IBusinessHoursRepository>();
        var customHoursRepo = _unitOfWork.GetRepository<ICustomBusinessHoursRepository>();
        var holidayRepo = _unitOfWork.GetRepository<IHolidayRepository>();

        var existing = await businessHoursRepo.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            _logger.LogWarning("Business hours not found for deletion: {Id}", id);
            return false;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await customHoursRepo.SoftDeleteByBusinessHourIdAsync(id, userId, cancellationToken);
            await holidayRepo.SoftDeleteByBusinessHourIdAsync(id, userId, cancellationToken);
            await businessHoursRepo.SoftDeleteAsync(id, userId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                objectKey: DynamicObjectConstants.DynamicObjectKeys.BusinessHours,
                objectItemId: id,
                fieldName: "deleted",
                oldValue: existing.Name,
                newValue: null,
                createdBy: userId,
                cancellationToken: cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Soft deleted business hours and linked records: {Id}", id);
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Holiday> CreateHolidayAsync(CreateHolidayRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var entity = new Holiday
        {
            BusinessHourId = request.BusinessHourId,
            Name = request.Name,
            Date = request.Date,
            CreatedBy = userId,
            UpdatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var repository = _unitOfWork.GetRepository<IHolidayRepository>();
        var created = await repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created holiday: {Id} - {Name} for BusinessHour: {BusinessHourId}", created.Id, created.Name, created.BusinessHourId);
        return created;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteHolidayAsync(int businessHourId, int holidayId, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var repository = _unitOfWork.GetRepository<IHolidayRepository>();

        var result = await repository.SoftDeleteAsync(holidayId, userId, cancellationToken);
        if (!result)
        {
            _logger.LogWarning("Holiday not found for deletion: {HolidayId} under BusinessHour: {BusinessHourId}", holidayId, businessHourId);
            return false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Soft deleted holiday: {HolidayId} under BusinessHour: {BusinessHourId}", holidayId, businessHourId);
        return true;
    }

    /// <summary>
    /// Validates that no time ranges overlap on the same day within the given items.
    /// </summary>
    /// <param name="items">The custom business hours items to validate.</param>
    /// <returns>True if valid (no overlaps), false otherwise.</returns>
    public static bool ValidateNoTimeOverlap(IEnumerable<CustomBusinessHoursItem> items)
    {
        var grouped = items.GroupBy(i => i.Day, StringComparer.OrdinalIgnoreCase);
        foreach (var group in grouped)
        {
            var sorted = group.OrderBy(i => i.StartTime).ToList();
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i].StartTime < sorted[i - 1].EndTime)
                    return false;
            }
        }
        return true;
    }
}
