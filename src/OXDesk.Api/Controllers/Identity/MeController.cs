using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Api.Common;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Api.Controllers.Identity
{
    /// <summary>
    /// Provides the authenticated user's profile.
    /// </summary>
    [ApiController]
    [Route("me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserFactory _userFactory;
        private readonly ILogger<MeController> _logger;

        public MeController(
            IUserService userService,
            IUserFactory userFactory,
            ILogger<MeController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userFactory = userFactory ?? throw new ArgumentNullException(nameof(userFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current authenticated user's profile with related reference data.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<UserResponse, UserDetailsRelated>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken = default)
        {
            // Ensure we have an authenticated user
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }

            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                _logger.LogDebug("Fetching profile for current user: {UserId}", currentUserId);

                var entity = await _userService.GetUserByIdAsync(currentUserId, cancellationToken);
                if (entity == null)
                {
                    return this.CreateNotFoundProblem("Current user was not found.");
                }

                var response = await _userFactory.BuildDetailsAsync(entity, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // In case current user cannot be resolved
                _logger.LogWarning(ex, "Failed to resolve current user from claims.");
                return Unauthorized();
            }
        }
    }
}
