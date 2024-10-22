using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using UserAPI.DataModels;

namespace UserAPI.Services
{
    public interface IAuthService
    {
        Task<string> GenerateTokenString(IdentityUser user);
        Task<(string Token, string ErrorMessage)> RegisterUser(LoginUser user);

        Task<(string Token, string ErrorMessage)> LoginUser(LoginUser user);
       Task<(string Token, string ErrorMessage)> LoginAdmin(LoginUser user);
    }
}