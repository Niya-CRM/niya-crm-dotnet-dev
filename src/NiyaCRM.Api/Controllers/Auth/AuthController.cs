using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Api.Helpers;
using NiyaCRM.Core.Auth.Constants;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Auth.DTOs;

namespace NiyaCRM.Api.Controllers.Auth
{
    /// <summary>
    /// Controller for handling authentication operations.
    /// </summary>
    [Route("auth")]
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtHelper _jwtHelper;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtHelper jwtHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {

            // If we have a token cookie, the middleware will have validated it and set User.Identity.IsAuthenticated
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Redirect to / if already logged in
                return Redirect("/");
            }

            // Check if the user is already authenticated by looking for the access_token cookie
            if (Request.Cookies.ContainsKey("access_token") && !string.IsNullOrEmpty(Request.Cookies["access_token"]))
            {
                // TO DO: RENEW THE TOKEN IF SESSION IS STILL VALID
            }
            
            return View();
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(model);
            }

            if (user.IsActive != "Y")
            {
                ModelState.AddModelError(string.Empty, "Account is deactivated");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(model);
            }

            // Generate JWT token
            var token = await _jwtHelper.GenerateJwtToken(user);
            
            // Store token in cookie
            HttpContext.Response.Cookies.Append(AuthConstants.Cookie.AccessTokenName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Enum.Parse<SameSiteMode>(AuthConstants.Cookie.SameSiteMode),
                Expires = DateTime.UtcNow.AddHours(AuthConstants.Cookie.ExpiryHours)
            });
            
            // Sign in with ASP.NET Identity session
            await _signInManager.SignInAsync(user, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(AuthConstants.Session.ExpiryHours)
            });
            
            // Redirect to home page or dashboard
            return Redirect("/");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Delete JWT token cookie
            Response.Cookies.Delete(AuthConstants.Cookie.AccessTokenName);
            
            // Sign out from ASP.NET Identity session
            await _signInManager.SignOutAsync();
            
            return Ok();
        }


    }


}
