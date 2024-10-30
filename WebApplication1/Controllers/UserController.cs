using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRabbitMqService _rabbitMqService;

        public UserController(IAuthService authService, UserManager<ApplicationUser> userManager, IRabbitMqService rabbitMqService)
        {
            _authService = authService;
            _userManager = userManager;
            _rabbitMqService = rabbitMqService;
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
               var notifications = new List<RabbitMqData>();
               var cancellationTokenSource = new CancellationTokenSource();
               var delay = TimeSpan.FromMilliseconds(2000);
               notifications.Clear();

               _rabbitMqService.StartConsuming(identityUser.Id, notification =>
               {
                notifications.Add(notification);
                cancellationTokenSource.CancelAfter(delay);

                });
                cancellationTokenSource.CancelAfter(delay);
                try
                {
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
                return Ok(new { Token = tokenString, Mail = user.Email, Role = role, Id = identityUser.Id, Notifications = notifications });
            }
            return BadRequest(errorMessage);
            
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(LogoutUser user)

        {
            var identityUser = await _userManager.FindByEmailAsync(user.Email);

            if (identityUser == null)
            {
                return BadRequest("User not found");
            }

            identityUser.IsOnline = false;
            await _userManager.UpdateAsync(identityUser);
            return Ok(new { Message = "Logout successful" });
        }
    }
}
