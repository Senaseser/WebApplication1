using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using UserAPI.DataModels;
using UserAPI.Services;

namespace UserAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthService _authService;

        public OperationsController( IAuthService authService, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _authService = authService;
        }
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
           
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                {
                    userList.Add(new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                    });
                }
            }

            return Ok(userList);
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser(LoginUser user)
        {
            var (token, errorMessage) = await _authService.RegisterUser(user);

            if (token != null)
            {
                var identityUser = await _userManager.FindByEmailAsync(user.Email);
                var roles = await _userManager.GetRolesAsync(identityUser);
                var role = roles.FirstOrDefault();
                return Ok(new { Token = token, Mail = user.Email, Role = role, Id = identityUser.Id });
            }
            return BadRequest(errorMessage);

        }
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult>UpdateUser(string id, [FromBody] UpdateUser updateUser)
        {
          
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound("User not found");
            }
            user.Email = updateUser.Email;
            user.UserName = updateUser.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return Ok(new{ Id=user.Id,Email=user.Email,UserName=user.UserName});


        }
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound("User not found");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
             return Ok(new { Message = "Delete successfully" });
        }
      
    }
}
