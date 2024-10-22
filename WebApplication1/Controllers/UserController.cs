﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using UserAPI.DataModels;
using UserAPI.Services;

namespace UserAPI.Controllers
{
    [Route("user/[controller]")]
    [ApiController]
    public class UserController : ControllerBase

    {
        private readonly IAuthService _authService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(IAuthService authService, UserManager<IdentityUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }
     
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUser user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state.");
            }
            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            if (identityUser == null)
            {
                return BadRequest("User not found");
            }
            var roles = await _userManager.GetRolesAsync(identityUser);
            var role = roles.FirstOrDefault();
            var (tokenString, errorMessage) = await _authService.LoginUser(user);
            if (tokenString != null)
            {
                return Ok(new { Token = tokenString, Mail = user.Email, Role = role , Id = identityUser.Id });
            }
            return BadRequest(errorMessage);
        }
    }
}
