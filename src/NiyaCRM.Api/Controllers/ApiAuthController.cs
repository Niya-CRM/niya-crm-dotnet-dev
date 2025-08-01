using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NiyaCRM.Api.Helpers;
using NiyaCRM.Core.Auth.Constants;
using NiyaCRM.Core.Auth.DTOs;
using NiyaCRM.Core.Identity;

namespace NiyaCRM.Api.Controllers
{
    [Route("auth")]
    [ApiController]
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
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token if authentication is successful</returns>
        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
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
            
            // Return token in JSON response
            return Ok(new 
            { 
                Token = token,
                ExpiresIn = AuthConstants.Jwt.TokenExpiryHours * 3600, // Convert hours to seconds
                AuthConstants.Jwt.TokenType
            });
        }
    }
}
