using System;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Core.Settings;

namespace OXDesk.Identity.Services;

/// <summary>
/// Service implementation for managing user signatures.
/// </summary>
public class UserSignatureService : IUserSignatureService
{
    private readonly IUserSignatureRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly ISettingService _settingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSignatureService"/> class.
    /// </summary>
    /// <param name="repository">The user signature repository.</param>
    /// <param name="currentUser">The current user accessor.</param>
    /// <param name="settingService">The setting service for retrieving signature settings.</param>
    public UserSignatureService(
        IUserSignatureRepository repository,
        ICurrentUser currentUser,
        ISettingService settingService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
    }

    private int GetCurrentUserId()
    {
        return _currentUser.Id ?? throw new InvalidOperationException("Current user ID is null.");
    }

    /// <inheritdoc/>
    public async Task<UserSignature?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByUserIdAsync(userId);
    }

    /// <inheritdoc/>
    public async Task<UserSignature> UpsertAsync(int userId, UpsertUserSignatureRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentUserId = GetCurrentUserId();

        var signatureSetting = await _settingService.GetSignatureAsync(cancellationToken);
        var signatureType = string.IsNullOrWhiteSpace(signatureSetting?.SignatureType)
            ? SettingConstant.SignatureTypes.StandardFixed
            : signatureSetting.SignatureType;

        var isFreeStyle = signatureType == SettingConstant.SignatureTypes.FreeStyle;

        var existing = await _repository.GetByUserIdAsync(userId);

        if (existing != null)
        {
            ApplyFields(existing, request, isFreeStyle);
            existing.UpdatedAt = now;
            existing.UpdatedBy = currentUserId;

            return await _repository.UpdateAsync(existing);
        }

        var entity = new UserSignature
        {
            UserId = userId,
            CreatedAt = now,
            CreatedBy = currentUserId,
            UpdatedAt = now,
            UpdatedBy = currentUserId
        };

        ApplyFields(entity, request, isFreeStyle);

        return await _repository.AddAsync(entity);
    }

    /// <summary>
    /// Applies request fields to the entity based on the active signature type.
    /// </summary>
    private static void ApplyFields(UserSignature entity, UpsertUserSignatureRequest request, bool isFreeStyle)
    {
        if (isFreeStyle)
        {
            entity.FreeStyleSignature = request.FreeStyleSignature;
            entity.ComplimentaryClose = null;
            entity.FullName = null;
            entity.JobTitle = null;
            entity.Company = null;
            entity.Department = null;
            entity.AddressLine1 = null;
            entity.AddressLine2 = null;
            entity.AddressLine3 = null;
            entity.Telephone = null;
            entity.Mobile = null;
            entity.Email = null;
            entity.Website = null;
        }
        else
        {
            entity.ComplimentaryClose = request.ComplimentaryClose;
            entity.FullName = request.FullName;
            entity.JobTitle = request.JobTitle;
            entity.Company = request.Company;
            entity.Department = request.Department;
            entity.AddressLine1 = request.AddressLine1;
            entity.AddressLine2 = request.AddressLine2;
            entity.AddressLine3 = request.AddressLine3;
            entity.Telephone = request.Telephone;
            entity.Mobile = request.Mobile;
            entity.Email = request.Email;
            entity.Website = request.Website;
            entity.FreeStyleSignature = null;
        }
    }
}
