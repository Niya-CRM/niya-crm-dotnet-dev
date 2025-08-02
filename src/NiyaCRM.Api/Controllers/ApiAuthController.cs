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

namespace NiyaCRM.Api.Controllers
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

        public ApiAuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtHelper jwtHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            if (user.IsActive != "Y")
            {
                return Unauthorized(new { Message = "Account is deactivated. Please contact Support." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

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
