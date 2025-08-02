using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NiyaCRM.Api.Helpers;
using NiyaCRM.Core.Auth.Constants;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Auth.DTOs;

namespace NiyaCRM.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            JwtHelper jwtHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _jwtHelper = jwtHelper;
        }

        [HttpGet]
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

        [HttpPost]
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

            var token = await _jwtHelper.GenerateJwtToken(user);
            
            // Store token in cookie
            HttpContext.Response.Cookies.Append(AuthConstants.Cookie.AccessTokenName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Enum.Parse<SameSiteMode>(AuthConstants.Cookie.SameSiteMode),
                Expires = DateTime.UtcNow.AddHours(AuthConstants.Cookie.ExpiryHours)
            });
            
            // Redirect to home page or dashboard
            return Redirect("/");
        }

        [HttpPost("auth/logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(AuthConstants.Cookie.AccessTokenName);
            return Ok();
        }


    }


}
