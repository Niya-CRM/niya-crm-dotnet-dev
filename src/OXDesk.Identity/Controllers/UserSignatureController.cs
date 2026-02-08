using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OXDesk.Shared.Extensions.Http;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Identity.Controllers;

/// <summary>
/// Controller for managing user signatures.
/// </summary>
[ApiController]
[Route("users/{userId:int}/signature")]
[Authorize]
public sealed class UserSignatureController : ControllerBase
{
    private readonly IUserSignatureService _signatureService;
    private readonly IUserSignatureFactory _signatureFactory;
    private readonly IValidator<UpsertUserSignatureRequest> _upsertValidator;
    private readonly ILogger<UserSignatureController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSignatureController"/> class.
    /// </summary>
    /// <param name="signatureService">The user signature service.</param>
    /// <param name="signatureFactory">The user signature factory.</param>
    /// <param name="upsertValidator">Validator for upsert requests.</param>
    /// <param name="logger">The logger.</param>
    public UserSignatureController(
        IUserSignatureService signatureService,
        IUserSignatureFactory signatureFactory,
        IValidator<UpsertUserSignatureRequest> upsertValidator,
        ILogger<UserSignatureController> logger)
    {
        _signatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));
        _signatureFactory = signatureFactory ?? throw new ArgumentNullException(nameof(signatureFactory));
        _upsertValidator = upsertValidator ?? throw new ArgumentNullException(nameof(upsertValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the signature for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user signature if found.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(UserSignatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _signatureService.GetByUserIdAsync(userId, cancellationToken);
        if (entity == null)
            return this.CreateNotFoundProblem($"Signature for user with ID '{userId}' was not found.");

        var response = await _signatureFactory.BuildResponseAsync(entity, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Creates or updates the signature for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The upsert request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created or updated user signature.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(UserSignatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertAsync(int userId, [FromBody] UpsertUserSignatureRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _upsertValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return this.CreateBadRequestProblem(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        try
        {
            var entity = await _signatureService.UpsertAsync(userId, request, cancellationToken);
            var response = await _signatureFactory.BuildResponseAsync(entity, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Upsert user signature failed: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
    }

    /// <summary>
    /// Deleting user signatures is not allowed.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status405MethodNotAllowed)]
    public IActionResult DeleteAsync(int userId)
    {
        return this.CreateMethodNotAllowedProblem("Deleting user signatures is not allowed.");
    }
}
