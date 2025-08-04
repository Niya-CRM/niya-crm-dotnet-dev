using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NiyaCRM.Api.Helpers;
using NiyaCRM.Core.Auth.Constants;
using NiyaCRM.Core.Auth.DTOs;
using NiyaCRM.Core.Identity;

namespace NiyaCRM.Api.Controllers.Auth
{
    /// <summary>
    /// API Authentication controller for external clients
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for external API authentication using JWT tokens
    /// </remarks>
    [Route("auth")]
    [ApiController]
    [AllowAnonymous]
    [Produces("application/json")]
    public class ApiAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<ApiAuthController> _logger;

        public ApiAuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtHelper jwtHelper,
            ILogger<ApiAuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
            _logger = logger;
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
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            if (user.IsActive != "Y")
            {
                _logger.LogWarning("User account is deactivated: {Email}", model.Email);
                return Unauthorized(new { Message = "Account is deactivated. Please contact Support." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Invalid password for user: {Email}", model.Email);
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            _logger.LogInformation("User authenticated successfully: {Email}", model.Email);

            var token = await _jwtHelper.GenerateJwtToken(user);
            
            // Return token in JSON response using the TokenResponse model
            return Ok(new TokenResponse
            { 
                Token = token,
                ExpiresIn = AuthConstants.Jwt.TokenExpiryHours * 3600, // Convert hours to seconds
                TokenType = AuthConstants.Jwt.TokenType
            });
        }
    }
}
