﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserAPI.DataModels;

namespace UserAPI.Services
{
    public class AuthService : IAuthService

    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public AuthService(UserManager<ApplicationUser> userManager,IConfiguration config,RoleManager<IdentityRole> rolemanager,IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _roleManager = rolemanager;
            _config = config;
            _serviceProvider = serviceProvider;
        }


        public async Task<( string ErrorMessage, ApplicationUser User)> RegisterUser(LoginUser user)
        {
            var identityUser = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                IsOnline = false,
            };
            var result = await _userManager.CreateAsync(identityUser, user.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                var message = string.Join(", ", errors);
                return (  message, identityUser);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                await _userManager.AddToRoleAsync(identityUser, "User");
            return ("Kayıt başarılı.", identityUser);

        }
        public async Task<(string Token, string ErrorMessage)> LoginUser(LoginUser user)
        {
            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            if (identityUser is null)
            {
                return (null, "User not found");
            }
            var isValidPassword = await _userManager.CheckPasswordAsync(identityUser, user.Password);
            if (!isValidPassword)
            {
                return (null, "Invalid password");
            }
            var roles = await _userManager.GetRolesAsync(identityUser);
            if (!roles.Contains("User"))
            {
                return (null, "This account is not a user account");
            }
            var token = await GenerateTokenString(identityUser);

            identityUser.IsOnline = true;
            await _userManager.UpdateAsync(identityUser);

            var hubContext = _serviceProvider.GetService<IHubContext<NotificationHub>>();
            await hubContext.Clients.All.SendAsync("UserOnline", identityUser.Id);

            return (token, null);
        }
        public async Task<(string Token, string ErrorMessage)> LoginAdmin(LoginUser user)
        {
            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            if (identityUser is null )
            {
                return (null,"Admin not found");
            }
            var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, user.Password);
            if (!isPasswordValid )
            {
                return (null, "Invalid password");
            }
            var roles = await _userManager.GetRolesAsync(identityUser);
            if (!roles.Contains("Admin"))
            {
                return (null,"This account is not a admin account");
            }
            var token = await GenerateTokenString(identityUser);
            return (token,null);
        }

        public async Task<string> GenerateTokenString(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            if (role == null)
            {
                throw new Exception("User does not have any roles.");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email,user.UserName),
                new Claim(ClaimTypes.Role,role),
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));
            var signingCred = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha512Signature);
            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                issuer: _config.GetSection("Jwt:Issuer").Value,
                audience: _config.GetSection("Jwt:Audience").Value,
                signingCredentials: signingCred
                );
            return new JwtSecurityTokenHandler().WriteToken(securityToken);
   
        }
    }
}
