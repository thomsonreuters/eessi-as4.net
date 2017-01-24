using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly ITokenService tokenService;
        private readonly UserManager<ApplicationUser> userManager;

        public AuthenticationController(ITokenService tokenService, UserManager<ApplicationUser> userManager)
        {
            this.tokenService = tokenService;
            this.userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var user = await userManager.FindByNameAsync(login.Username);
            var result = await userManager.CheckPasswordAsync(user, login.Password);
            if (result)
                return new OkObjectResult(new
                {
                    access_token = tokenService.GenerateToken()
                });

            return new UnauthorizedResult();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("externallogin")]
        public async Task<IActionResult> ExternalLogin(string provider = null)
        {
            await HttpContext.Authentication.ChallengeAsync(provider, new AuthenticationProperties {RedirectUri = "http://localhost:3000/#/login?callback=true"});
            return new OkResult();
        }

        [HttpGet]
        [Authorize]
        [Route("externallogincallback")]
        public async Task<IActionResult> ExternalLoginCallback(string provider)
        {
            var isAuthenticated = await HttpContext.Authentication.GetAuthenticateInfoAsync(provider);
            if (isAuthenticated.Principal?.Identity?.IsAuthenticated != true) return new UnauthorizedResult();
            await HttpContext.Authentication.SignOutAsync("Cookies");
            return new OkObjectResult(new
            {
                access_token = tokenService.GenerateToken()
            });
        }
    }
}