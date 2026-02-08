using System;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Identity.Services;

/// <summary>
/// Service implementation for managing user signatures.
/// </summary>
public class UserSignatureService : IUserSignatureService
{
    private readonly IUserSignatureRepository _repository;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSignatureService"/> class.
    /// </summary>
    /// <param name="repository">The user signature repository.</param>
    /// <param name="currentUser">The current user accessor.</param>
    public UserSignatureService(
        IUserSignatureRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
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

        var existing = await _repository.GetByUserIdAsync(userId);

        if (existing != null)
        {
            existing.ComplimentaryClose = request.ComplimentaryClose;
            existing.FullName = request.FullName;
            existing.JobTitle = request.JobTitle;
            existing.Company = request.Company;
            existing.Department = request.Department;
            existing.AddressLine1 = request.AddressLine1;
            existing.AddressLine2 = request.AddressLine2;
            existing.AddressLine3 = request.AddressLine3;
            existing.Telephone = request.Telephone;
            existing.Mobile = request.Mobile;
            existing.Email = request.Email;
            existing.Website = request.Website;
            existing.UpdatedAt = now;
            existing.UpdatedBy = currentUserId;

            return await _repository.UpdateAsync(existing);
        }

        var entity = new UserSignature
        {
            UserId = userId,
            ComplimentaryClose = request.ComplimentaryClose,
            FullName = request.FullName,
            JobTitle = request.JobTitle,
            Company = request.Company,
            Department = request.Department,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            AddressLine3 = request.AddressLine3,
            Telephone = request.Telephone,
            Mobile = request.Mobile,
            Email = request.Email,
            Website = request.Website,
            CreatedAt = now,
            CreatedBy = currentUserId,
            UpdatedAt = now,
            UpdatedBy = currentUserId
        };

        return await _repository.AddAsync(entity);
    }
}
