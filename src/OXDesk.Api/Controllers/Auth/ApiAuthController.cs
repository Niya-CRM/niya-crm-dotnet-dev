using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OXDesk.Api.Helpers;
using OXDesk.Core.Auth.Constants;
using OXDesk.Core.Auth.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;

namespace OXDesk.Api.Controllers.Auth
{
    /// <summary>
    /// API Authentication controller for external clients
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for external API authentication using JWT tokens
    /// </remarks>
    [Route("auth")]
    [ApiController]
    [Produces("application/json")]
    public class ApiAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<ApiAuthController> _logger;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuditLogService _auditLogService;

        public ApiAuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtHelper jwtHelper,
            IAuditLogService auditLogService,
            ILogger<ApiAuthController> logger,
            IUserRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
            _auditLogService = auditLogService;
            _logger = logger;
            _refreshTokenRepository = refreshTokenRepository;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="model">Login credentials containing email and password</param>
        /// <returns>JWT token response with token, expiry time, and token type</returns>
        /// <response code="200">Returns the JWT token information</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If authentication fails or account is deactivated</response>
        [HttpPost("token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] ApiLoginDto model)
        {
            _logger.LogInformation("Authenticating user with email: {Email}", model.Email);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Email}", model.Email);
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            if (user.IsActive != "Y")
            {
                _logger.LogWarning("User account is deactivated: {Email}", model.Email);
                // Audit: Login denied due to inactive account
                await _auditLogService.CreateAuditLogAsync(
                    objectKey: CommonConstant.MODULE_USER,
                    @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
                    objectItemId: user.Id,
                    ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                    data: "Login Denied - Account not Active",
                    createdBy: user.Id,
                    cancellationToken: default
                );
                return Unauthorized(new { Message = "Account is deactivated. Please contact Support." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Invalid password for user: {Email}", model.Email);
                // Audit: Invalid credential attempt
                await _auditLogService.CreateAuditLogAsync(
                    objectKey: CommonConstant.MODULE_USER,
                    @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
                    objectItemId: user.Id,
                    ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                    data: "Invalid Credential",
                    createdBy: user.Id,
                    cancellationToken: default
                );
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            _logger.LogInformation("User authenticated successfully: {Email}", model.Email);
            // Audit: Successful login
            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_USER,
                @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
                objectItemId: user.Id,
                ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                data: "Login Successful",
                createdBy: user.Id,
                cancellationToken: default
            );
            var tokenResponse = await _jwtHelper.BuildTokenResponse(user);

            // Return token and user info in JSON response using the TokenResponse model
            return Ok(tokenResponse);
        }

        /// <summary>
        /// Refreshes the access token using a valid refresh token. Issues a new refresh token as well.
        /// </summary>
        /// <param name="request">The refresh token request.</param>
        /// <returns>New access and refresh token pair.</returns>
        /// <response code="200">Returns a new TokenResponse</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If the refresh token is invalid</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Refreshing token");

            var response = await _jwtHelper.RefreshAsync(request.RefreshToken);
            if (response == null)
            {
                return Unauthorized(new { Message = "Invalid refresh token" });
            }

            return Ok(response);
        }

        /// <summary>
        /// Logs out the current user by revoking all their refresh tokens.
        /// Requires a valid access token.
        /// </summary>
        /// <returns>No content on success.</returns>
        /// <response code="204">All refresh tokens revoked</response>
        /// <response code="401">If not authenticated</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var userId) || userId <= 0)
            {
                return Unauthorized();
            }

            await _refreshTokenRepository.DeleteByUserIdAsync(userId);
            await _signInManager.SignOutAsync();
            return NoContent();
        }
    }
}
