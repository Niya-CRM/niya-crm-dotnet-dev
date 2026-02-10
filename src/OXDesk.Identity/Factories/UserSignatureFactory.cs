using System;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Core.Settings;

namespace OXDesk.Identity.Factories;

/// <summary>
/// Builds UserSignature response DTOs from entities.
/// </summary>
public sealed class UserSignatureFactory : IUserSignatureFactory
{
    private readonly IUserService _userService;
    private readonly ISettingService _settingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSignatureFactory"/> class.
    /// </summary>
    /// <param name="userService">The user service for resolving display names.</param>
    /// <param name="settingService">The setting service for retrieving signature settings.</param>
    public UserSignatureFactory(IUserService userService, ISettingService settingService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
    }

    /// <inheritdoc/>
    public async Task<UserSignatureResponse> BuildResponseAsync(UserSignature userSignature, CancellationToken cancellationToken = default)
    {
        var dto = new UserSignatureResponse
        {
            Id = userSignature.Id,
            UserId = userSignature.UserId,
            ComplimentaryClose = userSignature.ComplimentaryClose,
            FullName = userSignature.FullName,
            JobTitle = userSignature.JobTitle,
            Company = userSignature.Company,
            Department = userSignature.Department,
            AddressLine1 = userSignature.AddressLine1,
            AddressLine2 = userSignature.AddressLine2,
            AddressLine3 = userSignature.AddressLine3,
            Telephone = userSignature.Telephone,
            Mobile = userSignature.Mobile,
            Email = userSignature.Email,
            Website = userSignature.Website,
            FreeStyleSignature = userSignature.FreeStyleSignature,
            CreatedBy = userSignature.CreatedBy,
            CreatedAt = userSignature.CreatedAt,
            UpdatedBy = userSignature.UpdatedBy,
            UpdatedAt = userSignature.UpdatedAt
        };

        dto.SignatureSetting = await _settingService.GetSignatureAsync(cancellationToken);
        dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
        dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);

        return dto;
    }
}
