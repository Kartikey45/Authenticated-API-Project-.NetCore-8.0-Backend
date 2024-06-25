﻿using AuthenticatedWebAPI.Models;
using AuthenticatedWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthenticatedWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserService _userService; 

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] SignUpUser signUpUser)
        {
            string message = "";
            IdentityResult result = new();
            try
            {
                var user = new User()
                {
                    Name = signUpUser.Name,
                    Email = signUpUser.Email,
                    UserName = signUpUser.Email,
                    IsAdmin = signUpUser.IsAdmin
                };
                result = await _userManager.CreateAsync(user, signUpUser.Password).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }
                message = "registered successfull.";
            }
            catch (Exception ex)
            {
                return BadRequest("something went wrong, please try again." + ex.Message);
            }
            return Ok(new { message = message , result = result });
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody] SignInUser login)
        {
            string message = string.Empty;
            try
            {
                var _user = await _userManager.FindByEmailAsync(login.Email).ConfigureAwait(false);
                if (_user != null && !_user.EmailConfirmed)
                {
                    _user.EmailConfirmed = true;
                }
                var result = await _signInManager.PasswordSignInAsync(_user, login.Password, login.RememberMe, false).ConfigureAwait(false);

                if (!result.Succeeded)
                {
                    return Unauthorized("check your login credentials and try again.");
                }
                _user.LastLogin = DateTime.Now;
                var _updateResult = await _userManager.UpdateAsync(_user).ConfigureAwait(false);

                message = "login successfull.";
            }
            catch (Exception ex)
            {
                return BadRequest("something went wrong, please try again." + ex.Message);
            }

            return Ok(new { message = message });
        }

        [HttpGet("logout"), Authorize]
        public async Task<ActionResult> LogoutUser()
        {
            string message = "You are free to go !";
            try
            {
                await _signInManager.SignOutAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return BadRequest("something went wrong, please try again." + ex.Message);
            }

            return Ok(new { message = message });
        }

        [HttpGet("admin"), Authorize]
        public ActionResult AdminPage()
        {
            string[] partners =
            {
                "Raja", "Bill Gates", "Elon Mask", "Taylor Swift", "Jeff Bezos", "Mark Zukerberg", "Joe Bidden", "Putin"
            };
            return Ok(new { trustedPartners = partners });
        }

        [HttpGet("home/{email}"), Authorize]
        public async Task<ActionResult> HomePage(string email)
        {
            var _userInfo = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (_userInfo == null)
            {
                return BadRequest(new { message = "something went wrong, please try again." });
            }
            return Ok(new { userInfo = _userInfo });
        }

        [HttpGet("chkusr"), Authorize]
        public async Task<ActionResult> CheckUser()
        {
            string message = "logged in";
            User currentUser = new User();
            try
            {
                var _user = HttpContext.User;
                var principals = new ClaimsPrincipal(_user);
                //var result = _signInManager.IsSignedIn(principals);
                var result = _userService.IsAuthenticated();
                if (result)
                {
                    currentUser = await _signInManager.UserManager.GetUserAsync(principals).ConfigureAwait(false);
                }
                else
                {
                    return Forbid("access denied");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("something went wrong , please try again" + ex.Message);
            }

            return Ok(new { message = message, user = currentUser });
        }

        [HttpPost("change-password"), Authorize]
        public async Task<ActionResult> ChangePassword(ChangePassword model)
        {
            IdentityResult result = null;
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new {message = "invalid parameters details"});
                }
                var userId = _userService.GetUserId();
                var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
                if (user != null) { 
                    result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword).ConfigureAwait(false);
                }
                else
                {
                    return NotFound("not found");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("something went wrong, please try again." + ex.Message);
            }

            return Ok(new { message = "password changed successfully", result = result });
        }

    }
}
